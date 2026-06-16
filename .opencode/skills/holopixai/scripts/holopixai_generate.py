#!/usr/bin/env python3
"""HolopixAI API helper for Codex.

Implements the AK/SK signing protocol from the HolopixAI docs:
signature = HMAC-SHA256(secret_key_utf8, timestamp_ms + "=" + compact_json_body).hex()
"""

from __future__ import annotations

import argparse
import base64
import hashlib
import hmac
import json
import os
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
import uuid
from pathlib import Path
from typing import Any


DEFAULT_BASE_URL = "https://api.holopix.cn"

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def env(name: str, default: str | None = None) -> str | None:
    value = os.environ.get(name)
    return value if value not in (None, "") else default


def compact_json(data: Any) -> str:
    return json.dumps(data, ensure_ascii=False, separators=(",", ":"))


def get_path(data: Any, dotted: str | None) -> Any:
    if not dotted:
        return None
    cur = data
    for part in dotted.split("."):
        if isinstance(cur, dict):
            cur = cur.get(part)
        elif isinstance(cur, list):
            try:
                cur = cur[int(part)]
            except (ValueError, IndexError):
                return None
        else:
            return None
    return cur


def build_url(base_url: str, path: str) -> str:
    if path.startswith("http://") or path.startswith("https://"):
        return path
    return urllib.parse.urljoin(base_url.rstrip("/") + "/", path.lstrip("/"))


def make_signature(timestamp_ms: str, body: dict[str, Any], secret_key: str) -> str:
    string_to_sign = timestamp_ms + "=" + compact_json(body)
    return hmac.new(
        secret_key.encode("utf-8"),
        string_to_sign.encode("utf-8"),
        hashlib.sha256,
    ).hexdigest()


def auth_headers(body: dict[str, Any]) -> dict[str, str]:
    access_key = env("HOLOPIXAI_ACCESS_KEY")
    secret_key = env("HOLOPIXAI_SECRET_KEY")
    if not access_key or not secret_key:
        raise RuntimeError("Missing HOLOPIXAI_ACCESS_KEY or HOLOPIXAI_SECRET_KEY.")

    timestamp_ms = str(int(time.time() * 1000))
    nonce = str(uuid.uuid4())
    signature = make_signature(timestamp_ms, body, secret_key)
    return {
        "X-Access-Key": access_key,
        "X-Signature": signature,
        "X-Timestamp": timestamp_ms,
        "X-Nonce": nonce,
    }


def request_json(base_url: str, path: str, body: dict[str, Any]) -> Any:
    url = build_url(base_url, path)
    data = compact_json(body).encode("utf-8")
    request = urllib.request.Request(
        url,
        data=data,
        method="POST",
        headers={
            "Accept": "application/json",
            "Content-Type": "application/json",
            **auth_headers(body),
        },
    )
    try:
        with urllib.request.urlopen(request, timeout=120) as response:
            raw = response.read().decode("utf-8")
            return json.loads(raw) if raw else {}
    except urllib.error.HTTPError as exc:
        detail = exc.read().decode("utf-8", errors="replace")
        raise RuntimeError(f"HTTP {exc.code} from {url}: {detail}") from exc
    except urllib.error.URLError as exc:
        raise RuntimeError(f"Network error calling {url}: {exc}") from exc


def first_image_value(data: Any) -> str | None:
    candidate_paths = [
        env("HOLOPIXAI_IMAGE_FIELD"),
        "data.clientList.0.imgUrls.0",
        "data.imgUrls.0",
        "data.0.url",
        "data.0.image_url",
        "data.0.b64_json",
        "data.0.base64",
        "data.0.image",
        "result.url",
        "result.image_url",
        "result.b64_json",
        "result.base64",
        "result.image",
        "image_url",
        "url",
        "b64_json",
        "base64",
        "image",
    ]
    for path in candidate_paths:
        value = get_path(data, path)
        if isinstance(value, str) and value.strip():
            return value.strip()
    return None


def first_client_id(data: Any) -> str | None:
    candidate_paths = [
        env("HOLOPIXAI_TASK_ID_FIELD"),
        "data.clientId",
        "data.clientIds.0",
        "clientId",
    ]
    for path in candidate_paths:
        value = get_path(data, path)
        if isinstance(value, str) and value.strip():
            return value.strip()
    return None


def save_image(value: str, out_path: Path) -> None:
    out_path.parent.mkdir(parents=True, exist_ok=True)
    if value.startswith("data:image"):
        _, encoded = value.split(",", 1)
        out_path.write_bytes(base64.b64decode(encoded))
        return
    if value.startswith("http://") or value.startswith("https://"):
        with urllib.request.urlopen(value, timeout=180) as response:
            out_path.write_bytes(response.read())
        return

    compact = value.strip()
    if len(compact) > 100 and all(c.isalnum() or c in "+/=\n\r" for c in compact[:200]):
        out_path.write_bytes(base64.b64decode(compact))
        return

    raise RuntimeError("Could not identify image output as URL, data URI, or base64.")


def parse_extra(values: list[str]) -> dict[str, Any]:
    body: dict[str, Any] = {}
    for item in values:
        if "=" not in item:
            raise ValueError(f"--extra must be key=value, got: {item}")
        key, raw = item.split("=", 1)
        try:
            value: Any = json.loads(raw)
        except json.JSONDecodeError:
            value = raw
        body[key] = value
    return body


def set_if_present(payload: dict[str, Any], key: str, value: Any) -> None:
    if value is not None:
        payload[key] = value


def set_if_flag(payload: dict[str, Any], key: str, value: bool) -> None:
    if value:
        payload[key] = True


def save_response(response: Any, path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(response, ensure_ascii=False, indent=2), encoding="utf-8")


def command_call(args: argparse.Namespace) -> int:
    payload = json.loads(args.data_json) if args.data_json else parse_extra(args.extra)
    body = payload if args.raw_body else {"data": payload}
    response = request_json(args.base_url, args.path, body)
    if args.response_json:
        save_response(response, Path(args.response_json))
    print(json.dumps(response, ensure_ascii=False, indent=2))
    return 0


def command_rights(args: argparse.Namespace) -> int:
    response = request_json(args.base_url, "/v1/apiUser/rights", {"data": {}})
    if args.response_json:
        save_response(response, Path(args.response_json))
    print(json.dumps(response, ensure_ascii=False, indent=2))
    return 0


def command_models(args: argparse.Namespace) -> int:
    response = request_json(args.base_url, "/v1/model/apiList", {"data": {}})
    if args.response_json:
        save_response(response, Path(args.response_json))
    print(json.dumps(response, ensure_ascii=False, indent=2))
    return 0


def command_model_detail(args: argparse.Namespace) -> int:
    response = request_json(args.base_url, "/v1/model/apiDetail", {"data": {"id": args.id}})
    if args.response_json:
        save_response(response, Path(args.response_json))
    print(json.dumps(response, ensure_ascii=False, indent=2))
    return 0


def command_query_progress(args: argparse.Namespace) -> int:
    response = request_json(args.base_url, "/v1/images/generations/queryProgress", {"data": {"clientIds": args.client_id}})
    if args.response_json:
        save_response(response, Path(args.response_json))
    print(json.dumps(response, ensure_ascii=False, indent=2))

    if args.out:
        image_value = first_image_value(response)
        if not image_value:
            raise RuntimeError("No completed image URL/base64 found in query response.")
        save_image(image_value, Path(args.out))
    return 0


def command_generate(args: argparse.Namespace) -> int:
    payload = parse_extra(args.extra)
    payload["prompt"] = args.prompt
    set_if_present(payload, "negativePrompt", args.negative_prompt)
    set_if_present(payload, "aspectRatios", args.aspect_ratios)
    if args.model_id:
        try:
            model_id: int | str = int(args.model_id)
        except ValueError:
            model_id = args.model_id
        model_detail: dict[str, Any] = {"modelId": model_id}
        if not args.no_model_strength:
            model_detail["strength"] = args.model_strength
        payload["modelDetailList"] = [model_detail]
    if args.seed is not None:
        payload["seed"] = args.seed
    set_if_present(payload, "callback_url", args.callback_url)
    set_if_present(payload, "sourceImage", args.source_image)
    set_if_present(payload, "imageMode", args.image_mode)
    set_if_present(payload, "imageColor", args.image_color)
    set_if_present(payload, "imageWeight", args.image_weight)
    set_if_present(payload, "imageReference", args.image_reference)
    set_if_present(payload, "referenceMode", args.reference_mode)
    set_if_present(payload, "referenceWeight", args.reference_weight)
    set_if_present(payload, "imageGuidanceWeights", args.image_guidance_weights)
    set_if_present(payload, "hdScale", args.hd_scale)
    set_if_present(payload, "perturb", args.perturb)
    set_if_present(payload, "characterPose", args.character_pose)
    set_if_present(payload, "batchSize", args.batch_size)
    set_if_flag(payload, "faceDetail", args.face_detail)
    set_if_flag(payload, "hdFix", args.hd_fix)
    set_if_flag(payload, "simpleBackground", args.simple_background)
    set_if_flag(payload, "enablePerturb", args.enable_perturb)

    if args.path:
        create_path = args.path
    elif args.mode == "i2i":
        create_path = env("HOLOPIXAI_I2I_PATH", "/v1/images/generations/i2i")
    else:
        create_path = env("HOLOPIXAI_T2I_PATH", "/v1/images/generations/t2i")

    create_response = request_json(args.base_url, create_path, {"data": payload})
    response_json = Path(args.response_json) if args.response_json else Path(args.out).with_suffix(Path(args.out).suffix + ".response.json")
    save_response(create_response, response_json)

    client_id = first_client_id(create_response)
    if not client_id:
        image_value = first_image_value(create_response)
        if image_value:
            save_image(image_value, Path(args.out))
            print(json.dumps({"out": args.out, "response_json": str(response_json), "mode": "direct"}, ensure_ascii=False, indent=2))
            return 0
        print(json.dumps(create_response, ensure_ascii=False, indent=2))
        raise RuntimeError(f"No client id or image URL found. Saved response JSON: {response_json}")

    if not args.wait:
        print(json.dumps({"client_id": client_id, "response_json": str(response_json), "mode": "submitted", "create_path": create_path}, ensure_ascii=False, indent=2))
        return 0

    deadline = time.time() + args.max_wait_seconds
    last_response = create_response
    while time.time() < deadline:
        time.sleep(args.poll_seconds)
        last_response = request_json(args.base_url, "/v1/images/generations/queryProgress", {"data": {"clientIds": [client_id]}})
        save_response(last_response, response_json)
        status = get_path(last_response, "data.clientList.0.status")
        if status == "succeed":
            image_value = first_image_value(last_response)
            if not image_value:
                raise RuntimeError(f"Task succeeded but no image URL found. Saved response JSON: {response_json}")
            save_image(image_value, Path(args.out))
            print(json.dumps({"out": args.out, "response_json": str(response_json), "client_id": client_id, "mode": "async", "create_path": create_path}, ensure_ascii=False, indent=2))
            return 0
        if status == "failed":
            raise RuntimeError(f"HolopixAI task failed: {json.dumps(last_response, ensure_ascii=False)}")

    raise RuntimeError(f"Timed out waiting for HolopixAI task {client_id}. Last response saved: {response_json}")


def add_common(parser: argparse.ArgumentParser) -> None:
    parser.add_argument("--base-url", default=env("HOLOPIXAI_BASE_URL", DEFAULT_BASE_URL))
    parser.add_argument("--response-json", default=None)


def main() -> int:
    parser = argparse.ArgumentParser(description="Call HolopixAI APIs with official AK/SK signing.")
    sub = parser.add_subparsers(dest="command", required=True)

    p = sub.add_parser("call", help="Call an arbitrary HolopixAI POST endpoint with a data payload.")
    add_common(p)
    p.add_argument("--path", required=True)
    p.add_argument("--data-json", default=None, help="JSON object for body.data by default.")
    p.add_argument("--raw-body", action="store_true", help="Treat --data-json as the full request body.")
    p.add_argument("--extra", action="append", default=[], help="Additional body.data field as key=value.")
    p.set_defaults(func=command_call)

    p = sub.add_parser("rights", help="Query account rights and remaining points.")
    add_common(p)
    p.set_defaults(func=command_rights)

    p = sub.add_parser("models", help="Query available model list.")
    add_common(p)
    p.set_defaults(func=command_models)

    p = sub.add_parser("model-detail", help="Query model detail by model id.")
    add_common(p)
    p.add_argument("--id", required=True)
    p.set_defaults(func=command_model_detail)

    p = sub.add_parser("query-progress", help="Query image generation task progress.")
    add_common(p)
    p.add_argument("--client-id", action="append", required=True)
    p.add_argument("--out", default=None)
    p.set_defaults(func=command_query_progress)

    p = sub.add_parser("generate", help="Submit a HolopixAI one-click draft generation task.")
    add_common(p)
    p.add_argument("--mode", choices=["t2i", "i2i"], default="t2i", help="Use one-click text-to-image or image-to-image.")
    p.add_argument("--prompt", required=True)
    p.add_argument("--negative-prompt", default=None)
    p.add_argument("--out", required=True)
    p.add_argument("--path", default=None, help="Generation creation endpoint path.")
    p.add_argument("--aspect-ratios", default=None, help="HolopixAI aspectRatios value, such as 1:1 or 3:2.")
    p.add_argument("--model-id", default=None)
    p.add_argument("--model-strength", type=float, default=1.0)
    p.add_argument("--no-model-strength", action="store_true", help="Omit strength for ComposeStyle/combined models.")
    p.add_argument("--seed", type=int, default=-1)
    p.add_argument("--callback-url", default=None)
    p.add_argument("--source-image", default=None, help="Image URL/base64 for i2i sourceImage.")
    p.add_argument("--image-mode", choices=["linerSketch", "colorSketch"], default=None)
    p.add_argument("--image-color", type=json.loads, default=None, help="true/false for i2i imageColor.")
    p.add_argument("--image-weight", type=float, default=None)
    p.add_argument("--image-reference", default=None)
    p.add_argument("--reference-mode", choices=["standard", "color"], default=None)
    p.add_argument("--reference-weight", type=float, default=None)
    p.add_argument("--image-guidance-weights", type=int, default=None)
    p.add_argument("--face-detail", action="store_true")
    p.add_argument("--hd-fix", action="store_true")
    p.add_argument("--hd-scale", type=float, default=None)
    p.add_argument("--simple-background", action="store_true")
    p.add_argument("--enable-perturb", action="store_true")
    p.add_argument("--perturb", type=float, default=None)
    p.add_argument("--character-pose", default=None)
    p.add_argument("--batch-size", type=int, default=None)
    p.add_argument("--extra", action="append", default=[], help="Additional body.data field as key=value.")
    p.add_argument("--wait", action="store_true")
    p.add_argument("--poll-seconds", type=float, default=2)
    p.add_argument("--max-wait-seconds", type=float, default=300)
    p.set_defaults(func=command_generate)

    args = parser.parse_args()
    return args.func(args)


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        raise SystemExit(1)
