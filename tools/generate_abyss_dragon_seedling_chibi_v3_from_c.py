from __future__ import annotations

import json
import sys
import time
from pathlib import Path

sys.path.insert(0, r"C:\Users\Administrator\.codex\skills\holopixai\scripts")
import holopixai_generate as h  # noqa: E402


BASE_URL = "https://api.holopix.cn"
OUT_DIR = Path(r"C:\Users\Administrator\Desktop\Dragonfall\AIResult\03_enemies_bosses")
REFERENCE_URL = "https://genai.holopix.cn/2026-06-02/17804115615681780411641897-246b28d86145b9c7bf7c86858b97c4c4_00001_.png"
MODELS = [{"modelId": "NT4HQ78U2Q", "strength": 0.88}]

BASE_PROMPT = (
    "Refine the referenced character into a cleaner Dragonfall semi-chibi enemy character. "
    "Keep the same single character, same pose, same front-facing magic casting gesture, same white short hair, "
    "mint-green dragon horns, aqua dragon wings, teal-white dress, gold armor accents, aqua tail, and cute boss expression. "
    "Use 4 to 5 heads tall proportions, large readable head, compact torso, shorter legs, full body visible, centered. "
    "Make it suitable as a 2D indie game enemy character concept: bold outer contour, simplified costume shapes, "
    "limited teal aqua white gold palette, flat cel shading, clean readable silhouette, plain light background."
)

NEGATIVE = (
    "multiple characters, second character, duplicate, clone, extra head, extra body, inset portrait, small mascot, contact sheet, grid, "
    "two-head chibi, tiny mascot, very tall adult body, realistic anatomy, over-detailed key visual, delicate painterly rendering, "
    "busy background, text, label, logo, watermark, cropped body, cut off feet, sitting, kneeling, photorealistic, 3D, "
    "black hair, red outfit, nude, sexualized, giant breasts, horror"
)

TASKS = [
    (
        "abyss_dragon_seedling_chibi_v3_c1_clean",
        "conservative cleanup, preserve C closely, cleaner silhouette, less tiny decoration, stronger readable outline",
    ),
    (
        "abyss_dragon_seedling_chibi_v3_c2_sprite_base",
        "more sprite-ready base design, simpler wing membranes, simpler skirt layers, chunkier shapes, lower detail density",
    ),
    (
        "abyss_dragon_seedling_chibi_v3_c3_enemy_cute",
        "slightly stronger enemy presence, darker teal accents, confident cute boss expression, still soft and non-horror",
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
        "prompt": f"{BASE_PROMPT} Variant: {variant}. Draw one single full-body character only.",
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
    out = OUT_DIR / "abyss_dragon_seedling_chibi_v3_from_c_contact.png"
    canvas.save(out)
    return str(out)


def main() -> int:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    records = []
    for slug, variant in TASKS:
        print(f"Generating {slug}...", flush=True)
        records.append(generate(slug, variant))
        save_json(OUT_DIR / "abyss_dragon_seedling_chibi_v3_from_c_manifest.json", records)
        time.sleep(1.2)
    contact = make_contact(records)
    summary = {"records": records, "contact": contact}
    save_json(OUT_DIR / "abyss_dragon_seedling_chibi_v3_from_c_summary.json", summary)
    print(json.dumps(summary, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
