from __future__ import annotations

import json
import sys
import time
from pathlib import Path

sys.path.insert(0, r"C:\Users\Administrator\.codex\skills\holopixai\scripts")
import holopixai_generate as h  # noqa: E402


BASE_URL = "https://api.holopix.cn"
OUT_DIR = Path(r"C:\Users\Administrator\Desktop\Dragonfall\AIResult\03_enemies_bosses")
MODELS = [{"modelId": "NT4HQ78U2Q", "strength": 0.88}]

BASE_STYLE = (
    "Dragonfall semi-chibi enemy character concept, Japanese anime monster-girl, 4 to 5 heads tall, "
    "large readable head, compact torso, shorter simplified legs, full body visible, centered, plain light background, "
    "bold clean outer contour, simplified costume shapes, limited palette, flat cel shading, readable sprite-like silhouette, "
    "cute but enemy-like, suitable for 2D indie action RPG enemy sprite production"
)

NEGATIVE = (
    "multiple characters, duplicate, clone, extra head, extra body, inset portrait, contact sheet, grid, collage, text, label, logo, watermark, "
    "two-head chibi, tiny mascot, very tall adult body, realistic anatomy, over-detailed key visual, photorealistic, 3D render, "
    "busy background, scenery, cropped body, cut off feet, nude, sexualized, giant breasts, horror gore"
)

CHARACTERS = [
    {
        "slug": "king_slime",
        "reference": "https://genai.holopix.cn/2026-06-02/17803951206221780395127864-c415b48a9322acaa3e6352f1187cdb99_00001_.png",
        "identity": (
            "blue crystal king slime anthro monster-girl boss, translucent aqua slime body and hair, "
            "round jelly-like forms, small crystal crown, blue-white slime dress, watery glossy highlights, "
            "soft innocent face, pure cute boss aura"
        ),
        "variants": [
            ("idle_clean", "neutral idle pose, hands close to chest, shy gentle expression, small slime crown readable"),
            ("cast_splash", "cute slime magic casting pose, one hand forward, small aqua splash magic, confident soft smile"),
            ("hurt_wobble", "cute wobbling hurt reaction, body slightly leaning, watery skirt and hair bouncing, surprised face"),
        ],
    },
    {
        "slug": "crystal_skull",
        "reference": "https://genai.holopix.cn/2026-06-02/17803951364241780395143921-7c86022a86c19f16224c9d370f4f687d_00001_.png",
        "identity": (
            "crystal skull soft anthro monster-girl boss, white and pale violet hair, crystal skull motif, "
            "floating translucent crystals, bone-white and lavender outfit, soft ghostly but cute expression, "
            "not horror, elegant magical enemy"
        ),
        "variants": [
            ("idle_clean", "neutral floating idle pose, hands relaxed, calm mysterious expression, crystal skull motif clear"),
            ("cast_crystal", "small crystal magic casting pose, one hand raised, pale violet crystal shards floating nearby"),
            ("hurt_shatter", "cute startled hurt reaction, slight backward lean, tiny crystal fragments, still soft and readable"),
        ],
    },
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


def generate(character: dict, variant_slug: str, action: str) -> dict:
    slug = f"{character['slug']}_semichibi_v1_{variant_slug}"
    out_png = OUT_DIR / f"{slug}.png"
    payload_json = OUT_DIR / f"{slug}.payload.json"
    response_json = OUT_DIR / f"{slug}.response.json"
    if out_png.exists() and out_png.stat().st_size > 0:
        return {"asset": slug, "character": character["slug"], "out": str(out_png), "payload_json": str(payload_json), "response_json": str(response_json), "client_id": "existing"}

    prompt = (
        f"{BASE_STYLE}. "
        f"Character identity: {character['identity']}. "
        f"Action: {action}. "
        "Keep the same character identity and color palette from the reference image. "
        "Draw one single full-body character only, no extra views, no second character."
    )
    payload = {
        "modelDetailList": MODELS,
        "prompt": prompt,
        "negativePrompt": NEGATIVE,
        "seed": -1,
        "aspectRatios": "1:1",
        "simpleBackground": True,
        "batchSize": 1,
        "imageReference": character["reference"],
        "referenceMode": "standard",
        "referenceWeight": 1,
    }
    save_json(payload_json, payload)
    client_id = submit_and_wait(payload, out_png, response_json)
    return {"asset": slug, "character": character["slug"], "out": str(out_png), "payload_json": str(payload_json), "response_json": str(response_json), "client_id": client_id}


def make_contact(records: list[dict], slug: str) -> str:
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
    out = OUT_DIR / f"{slug}_semichibi_v1_contact.png"
    canvas.save(out)
    return str(out)


def make_overview(contact_paths: list[str]) -> str:
    from PIL import Image, ImageDraw, ImageFont

    font = ImageFont.load_default()
    canvas = Image.new("RGB", (1536, 1024), "white")
    draw = ImageDraw.Draw(canvas)
    for i, path in enumerate(contact_paths):
        img = Image.open(path).convert("RGB")
        img.thumbnail((1450, 460), Image.Resampling.LANCZOS)
        x = 40 + (1450 - img.width) // 2
        y = 30 + i * 500
        canvas.paste(img, (x, y))
        draw.text((40, y + 465), Path(path).stem, fill=(45, 45, 45), font=font)
    out = OUT_DIR / "enemy_semichibi_batch_v1_overview.png"
    canvas.save(out)
    return str(out)


def main() -> int:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    all_records = []
    contacts = []
    for character in CHARACTERS:
        char_records = []
        for variant_slug, action in character["variants"]:
            print(f"Generating {character['slug']} {variant_slug}...", flush=True)
            record = generate(character, variant_slug, action)
            char_records.append(record)
            all_records.append(record)
            save_json(OUT_DIR / "enemy_semichibi_batch_v1_manifest.json", all_records)
            time.sleep(1.2)
        contacts.append(make_contact(char_records, character["slug"]))
    overview = make_overview(contacts)
    summary = {"records": all_records, "contacts": contacts, "overview": overview}
    save_json(OUT_DIR / "enemy_semichibi_batch_v1_summary.json", summary)
    print(json.dumps(summary, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
