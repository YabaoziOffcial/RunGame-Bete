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

KING_SLIME_REFERENCE = "https://genai.holopix.cn/2026-06-02/17803951206221780395127864-c415b48a9322acaa3e6352f1187cdb99_00001_.png"

BASE_STYLE = (
    "Dragonfall semi-chibi enemy character concept, Japanese anime monster-girl, 4 to 5 heads tall, "
    "large readable head, compact torso, simplified limbs, full body visible, centered, plain light background, "
    "bold clean outer contour, simplified costume shapes, limited palette, flat cel shading, readable sprite-like silhouette, "
    "cute but enemy-like, suitable for 2D indie action RPG enemy sprite production"
)

NEGATIVE = (
    "multiple characters, duplicate, clone, extra head, extra body, inset portrait, contact sheet, grid, collage, text, label, logo, watermark, "
    "two-head chibi, tiny mascot, very tall adult body, realistic anatomy, over-detailed key visual, photorealistic, 3D render, "
    "busy background, scenery, cropped body, cut off important silhouette, nude, sexualized, giant breasts, gore, horror realism"
)

CHARACTERS = [
    {
        "slug": "slime_mermaid",
        "reference": KING_SLIME_REFERENCE,
        "identity": (
            "blue slime mermaid monster-girl, adapted from the blue crystal king slime girl, "
            "translucent aqua slime hair and watery body, jelly-like mermaid tail instead of legs, "
            "small blue slime crown, blue-white aquatic dress details, glossy water highlights, "
            "cute gentle sea-monster boss for chapter 2"
        ),
        "variants": [
            ("idle_float", "floating idle pose, hands near chest, mermaid tail curled gently, shy gentle expression"),
            ("cast_wave", "cute water magic casting pose, one hand forward, small splash wave magic, confident soft smile"),
            ("hurt_splash", "cute wobbling hurt reaction, watery tail and hair bouncing, surprised face, readable full silhouette"),
        ],
    },
    {
        "slug": "skeleton_knight",
        "reference": None,
        "identity": (
            "anthropomorphized skeleton knight monster-girl based on a cute fantasy skeleton warrior reference, "
            "ivory skull-mask or skull-like pale face with dark round eye sockets softened into anime eyes, "
            "visible rib-bone armor motif, segmented spine collar detail, one bronze shoulder pauldron, "
            "purple cloth underlayer, small round shield, short sword, leather straps, bone-white and muted purple palette, "
            "cute undead guard enemy, not horror, not crystal ghost"
        ),
        "variants": [
            ("idle_guard", "guard idle pose, small shield held at side, short sword lowered, calm blank cute expression"),
            ("ready_sword", "ready stance, sword raised diagonally, shield forward, compact readable silhouette"),
            ("hurt_stagger", "cute staggered hurt reaction, shield tilted, sword lowered, surprised dark-eye expression"),
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
    slug = f"{character['slug']}_semichibi_v2_{variant_slug}"
    out_png = OUT_DIR / f"{slug}.png"
    payload_json = OUT_DIR / f"{slug}.payload.json"
    response_json = OUT_DIR / f"{slug}.response.json"
    if out_png.exists() and out_png.stat().st_size > 0:
        return {"asset": slug, "character": character["slug"], "out": str(out_png), "payload_json": str(payload_json), "response_json": str(response_json), "client_id": "existing"}

    prompt = (
        f"{BASE_STYLE}. "
        f"Character identity: {character['identity']}. "
        f"Action: {action}. "
        "Draw one single full-body character only, no extra views, no second character."
    )
    if character["reference"]:
        prompt += " Keep the same soft blue slime identity and aquatic color palette from the reference image."

    payload = {
        "modelDetailList": MODELS,
        "prompt": prompt,
        "negativePrompt": NEGATIVE,
        "seed": -1,
        "aspectRatios": "1:1",
        "simpleBackground": True,
        "batchSize": 1,
    }
    if character["reference"]:
        payload.update(
            {
                "imageReference": character["reference"],
                "referenceMode": "standard",
                "referenceWeight": 1,
            }
        )
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
    out = OUT_DIR / f"{slug}_semichibi_v2_contact.png"
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
    out = OUT_DIR / "enemy_semichibi_batch_v2_mermaid_skeleton_overview.png"
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
            save_json(OUT_DIR / "enemy_semichibi_batch_v2_mermaid_skeleton_manifest.json", all_records)
            time.sleep(1.2)
        contacts.append(make_contact(char_records, character["slug"]))
    overview = make_overview(contacts)
    summary = {"records": all_records, "contacts": contacts, "overview": overview}
    save_json(OUT_DIR / "enemy_semichibi_batch_v2_mermaid_skeleton_summary.json", summary)
    print(json.dumps(summary, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
