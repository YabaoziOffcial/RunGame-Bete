from __future__ import annotations

import json
import sys
import time
from pathlib import Path

sys.path.insert(0, r"C:\Users\Administrator\.codex\skills\holopixai\scripts")
import holopixai_generate as h  # noqa: E402


BASE_URL = "https://api.holopix.cn"
OUT_DIR = Path(r"C:\Users\Administrator\Desktop\Dragonfall\AIResult\03_enemies_bosses")
POSE_REFERENCE_URL = "https://genai.holopix.cn/2026-06-02/17804108073521780410947129-2d1755aea4799d22d317f5f242bf7149_00001_.png"
MODELS = [{"modelId": "NT4HQ78U2Q", "strength": 0.9}]

CHARACTER = (
    "same Dragonfall abyss dragon seedling monster-girl character, white short bob hair, "
    "small mint-green sprout dragon horns, aqua dragon ears, translucent aqua dragon wings, "
    "white and pale mint fantasy dress with teal skirt, white-gold shoulder armor, "
    "small gold chest gemstone, cyan crystal highlights, elegant cute monster-girl boss"
)

STYLE = (
    "stylized indie game character illustration, 4 to 5 heads tall semi-chibi proportion, "
    "large head, compact torso, shorter simplified legs, readable sprite-like silhouette, "
    "bold clean outer contour, simplified inner linework, flat cel-shaded color blocks, "
    "limited teal aqua white gold palette, high contrast dark teal accents, soft paper-like texture, "
    "less rendered than anime key art, more like a polished 2D game enemy sprite concept, plain light background"
)

POSE = (
    "use the reference image pose: cute small magic casting pose, front-facing body, one hand raised forward, "
    "other arm relaxed near the wing, wings opened left and right, legs close together, confident playful expression"
)

NEGATIVE = (
    "two-head chibi, tiny mascot, very tall adult body, realistic anatomy, multiple characters, clone, duplicate, extra face, "
    "contact sheet, collage, grid, text, label, logo, watermark, cropped body, cut off feet, half body, sitting, kneeling, "
    "photorealistic, 3D render, over-detailed anime illustration, thin delicate rendering, dark horror, black hair, red outfit, "
    "nude, sexualized, giant breasts, busy background, scenery"
)

TASKS = [
    (
        "abyss_dragon_seedling_chibi_v2_flat_a",
        "most faithful to the casting pose, balanced cute boss expression, medium simplification",
    ),
    (
        "abyss_dragon_seedling_chibi_v2_flat_b",
        "more compact 4-head body, stronger bold outline, larger eyes, more sprite-like simplified costume shapes",
    ),
    (
        "abyss_dragon_seedling_chibi_v2_flat_c",
        "slightly darker teal accents, stronger fantasy enemy presence, still cute and readable, cleaner wing shape",
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
        return {
            "asset": slug,
            "out": str(out_png),
            "payload_json": str(payload_json),
            "response_json": str(response_json),
            "client_id": "existing",
        }

    prompt = (
        f"{CHARACTER}. {STYLE}. {POSE}. "
        f"Variant direction: {variant}. "
        "Draw one single full-body character only, centered, feet visible, no extra views. "
        "Keep the original pose, character identity, wings, horns, white hair, teal-white-gold colors, and casting gesture."
    )
    payload = {
        "modelDetailList": MODELS,
        "prompt": prompt,
        "negativePrompt": NEGATIVE,
        "seed": -1,
        "aspectRatios": "1:1",
        "simpleBackground": True,
        "batchSize": 1,
        "imageReference": POSE_REFERENCE_URL,
        "referenceMode": "standard",
        "referenceWeight": 1,
    }
    save_json(payload_json, payload)
    client_id = submit_and_wait(payload, out_png, response_json)
    return {
        "asset": slug,
        "out": str(out_png),
        "payload_json": str(payload_json),
        "response_json": str(response_json),
        "client_id": client_id,
    }


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
    out = OUT_DIR / "abyss_dragon_seedling_chibi_v2_flatstyle_contact.png"
    canvas.save(out)
    return str(out)


def main() -> int:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    records = []
    for slug, variant in TASKS:
        print(f"Generating {slug}...", flush=True)
        records.append(generate(slug, variant))
        save_json(OUT_DIR / "abyss_dragon_seedling_chibi_v2_flatstyle_manifest.json", records)
        time.sleep(1.2)
    contact = make_contact(records)
    summary = {"records": records, "contact": contact}
    save_json(OUT_DIR / "abyss_dragon_seedling_chibi_v2_flatstyle_summary.json", summary)
    print(json.dumps(summary, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
