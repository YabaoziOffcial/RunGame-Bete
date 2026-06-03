from __future__ import annotations

import json
import sys
import time
from pathlib import Path

sys.path.insert(0, r"C:\Users\Administrator\.codex\skills\holopixai\scripts")
import holopixai_generate as h  # noqa: E402


BASE_URL = "https://api.holopix.cn"
OUT_DIR = Path(r"C:\Users\Administrator\Desktop\Dragonfall\AIResult\03_enemies_bosses")
REFERENCE_URL = "https://genai.holopix.cn/2026-06-03/17804183395901780418347259-c9b5310be573e08bf8c1f35372088a5f_00001_.png"
MODELS = [{"modelId": "NT4HQ78U2Q", "strength": 0.88}]

BASE_PROMPT = (
    "Dragonfall semi-chibi enemy character concept, Japanese anime monster-girl, 4 to 5 heads tall, "
    "one single full-body character, centered, plain light background, bold clean outer contour, flat cel shading, "
    "readable 2D indie game enemy silhouette. "
    "Character: cute skeleton girl enemy. Her head is a normal gentle anime girl head, not a skull, not a mask; "
    "soft young female face, pale skin, calm non-scary expression, short light hair or white hair. "
    "Her neck, torso, arms, hands, legs, and feet have no flesh: exposed ivory skeleton bones are clearly visible, "
    "visible rib cage, spine, shoulder bones, arm bones, finger bones, pelvis, leg bones. "
    "Use cloth only as draped black or dark purple fabric pieces around shoulders, waist, and hips, leaving the bone body readable. "
    "Optional small red flower ornaments and muted bronze armor accents. "
    "She carries a small round shield and a short sword like a cute skeleton guard. "
    "Overall mood: cute, elegant, monster-girl, not frightening, not gore."
)

NEGATIVE = (
    "skull head, skull face, skull mask, full flesh body, normal human arms, normal human legs, skin-covered torso, "
    "zombie flesh, blood, gore, horror, scary monster, realistic skeleton, multiple characters, duplicate, extra head, extra body, "
    "inset portrait, contact sheet, grid, collage, text, label, logo, watermark, two-head chibi, tiny mascot, very tall adult body, "
    "photorealistic, 3D render, busy background, nude, sexualized, giant breasts, cropped body, cut off feet"
)

TASKS = [
    (
        "skeleton_girl_semichibi_v3_a_guard",
        "guard idle pose, small round shield at one side, short sword lowered, exposed rib cage and skeletal limbs clearly visible",
    ),
    (
        "skeleton_girl_semichibi_v3_b_flower",
        "soft dark cloth and red flower ornament version, gentle side-facing calm expression, elegant undead girl, bones still fully visible",
    ),
    (
        "skeleton_girl_semichibi_v3_c_ready",
        "compact ready stance for game enemy, shield forward, sword raised diagonally, cute determined face, strong readable skeleton silhouette",
    ),
]


def save_json(path: Path, data: object) -> None:
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding="utf-8")


def submit_and_wait(payload: dict, out_png: Path, response_json: Path) -> str:
    create_response = h.request_json(BASE_URL, "/v1/images/generations/t2i", {"data": payload})
    save_json(response_json, create_response)
    client_id = h.first_client_id(create_response)
    if not client_id:
        image_value = h.first_image_value(create_response)
        if not image_value:
            raise RuntimeError(f"No clientId/image in response: {response_json}")
        h.save_image(image_value, out_png)
        return "direct"

    deadline = time.time() + 420
    while time.time() < deadline:
        time.sleep(4)
        progress = h.request_json(BASE_URL, "/v1/images/generations/queryProgress", {"data": {"clientIds": [client_id]}})
        save_json(response_json, progress)
        status = h.get_path(progress, "data.clientList.0.status")
        if status == "succeed":
            image_value = h.first_image_value(progress)
            if not image_value:
                raise RuntimeError(f"Task succeeded but no image URL: {response_json}")
            h.save_image(image_value, out_png)
            return client_id
        if status == "failed":
            raise RuntimeError(f"Holopix task failed: {json.dumps(progress, ensure_ascii=False)}")
    raise RuntimeError(f"Timed out waiting for {client_id}")


def generate(slug: str, variant: str) -> dict:
    out_png = OUT_DIR / f"{slug}.png"
    payload_json = OUT_DIR / f"{slug}.payload.json"
    response_json = OUT_DIR / f"{slug}.response.json"
    if out_png.exists() and out_png.stat().st_size > 0:
        return {"asset": slug, "out": str(out_png), "payload_json": str(payload_json), "response_json": str(response_json), "client_id": "existing"}

    payload = {
        "modelDetailList": MODELS,
        "prompt": f"{BASE_PROMPT} Variant: {variant}.",
        "negativePrompt": NEGATIVE,
        "seed": -1,
        "aspectRatios": "1:1",
        "simpleBackground": True,
        "batchSize": 1,
        "imageReference": REFERENCE_URL,
        "referenceMode": "standard",
        "referenceWeight": 1,
    }
    save_json(payload_json, payload)
    client_id = submit_and_wait(payload, out_png, response_json)
    return {"asset": slug, "out": str(out_png), "payload_json": str(payload_json), "response_json": str(response_json), "client_id": client_id}


def make_contact(records: list[dict]) -> str:
    from PIL import Image, ImageDraw, ImageFont

    font = ImageFont.load_default()
    canvas = Image.new("RGB", (1536, 768), "white")
    draw = ImageDraw.Draw(canvas)
    for i, record in enumerate(records):
        img = Image.open(record["out"]).convert("RGB")
        img.thumbnail((460, 650), Image.Resampling.LANCZOS)
        x = 45 + i * 500 + (460 - img.width) // 2
        y = 30 + (650 - img.height) // 2
        canvas.paste(img, (x, y))
        draw.text((45 + i * 500, 700), record["asset"], fill=(45, 45, 45), font=font)
    out = OUT_DIR / "skeleton_girl_semichibi_v3_contact.png"
    canvas.save(out)
    return str(out)


def main() -> int:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    records = []
    for slug, variant in TASKS:
        print(f"Generating {slug}...", flush=True)
        records.append(generate(slug, variant))
        save_json(OUT_DIR / "skeleton_girl_semichibi_v3_manifest.json", records)
        time.sleep(1.2)
    contact = make_contact(records)
    summary = {"records": records, "contact": contact}
    save_json(OUT_DIR / "skeleton_girl_semichibi_v3_summary.json", summary)
    print(json.dumps(summary, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
