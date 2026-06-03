from __future__ import annotations

import json
import sys
import time
from pathlib import Path

sys.path.insert(0, r"C:\Users\Administrator\.codex\skills\holopixai\scripts")
import holopixai_generate as h  # noqa: E402


BASE_URL = "https://api.holopix.cn"
OUT_DIR = Path(r"C:\Users\Administrator\Desktop\Dragonfall\AIResult\03_enemies_bosses")
REFERENCE_URL = "https://genai.holopix.cn/2026-06-02/17803967342511780396824165-1c34cff5cb06f2ad8e7e69f7b4368eed_00001_.png"
MODELS = [{"modelId": "NT4HQ78U2Q", "strength": 0.9}]

CHARACTER = (
    "same character as reference, abyss dragon seedling monster-girl boss for Dragonfall, "
    "white short hair, small mint sprout-like dragon horns, translucent aqua dragon wings, "
    "white and pale mint fantasy dress, white-gold shoulder armor, gold chest gemstone, cyan crystal accents, "
    "cute elegant Japanese anime monster-girl identity"
)

STYLE = (
    "4 to 5 heads tall semi-chibi Japanese anime game character, not super-deformed 2-head chibi, "
    "compact indie game character proportions, simplified readable costume blocks, clean silhouette, "
    "polished 2D anime game art, clean linework, soft cel shading, plain white background, full body visible, feet visible"
)

NEGATIVE = (
    "2-head chibi, tiny mascot, baby body, realistic adult body, normal tall 6-head body, multiple characters, duplicate body, clone, "
    "contact sheet, collage, grid, panel layout, text, label, logo, watermark, cropped body, cut off feet, half body, "
    "busy background, scene background, photorealistic, 3D, dark horror, black hair, red main color, nude, sexualized, giant breasts"
)

TASKS = [
    (
        "abyss_dragon_seedling_chibi_v1_idle",
        "neutral idle stance, gentle boss-like calm expression, both hands relaxed, wings lightly opened",
    ),
    (
        "abyss_dragon_seedling_chibi_v1_ready",
        "cute alert ready pose, one foot slightly forward, one hand near chest gemstone, wings raised slightly, cautious expression",
    ),
    (
        "abyss_dragon_seedling_chibi_v1_cast",
        "small magic casting pose, one hand forward releasing cyan crystal magic, confident soft smile, wings balanced behind her",
    ),
    (
        "abyss_dragon_seedling_chibi_v1_hurt",
        "cute staggered hurt reaction pose, leaning back slightly, surprised expression, wings tucked inward, still elegant and readable",
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


def generate(slug: str, action: str) -> dict:
    out_png = OUT_DIR / f"{slug}.png"
    payload_json = OUT_DIR / f"{slug}.payload.json"
    response_json = OUT_DIR / f"{slug}.response.json"
    if out_png.exists() and out_png.stat().st_size > 0:
        return {"asset": slug, "out": str(out_png), "payload_json": str(payload_json), "response_json": str(response_json), "client_id": "existing"}

    prompt = (
        f"{STYLE}. {CHARACTER}. "
        f"One single full-body character only, {action}. "
        "Keep the head-to-body ratio around 4 to 5 heads tall. "
        "Preserve the original design: white short hair, mint horns, aqua wings, mint-white dress, gold shoulder armor, chest gemstone. "
        "Suitable as a 16-bit-inspired indie action RPG enemy character concept, but drawn as clean 2D anime art."
    )
    payload = {
        "modelDetailList": MODELS,
        "prompt": prompt,
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
    canvas = Image.new("RGB", (1536, 1024), "white")
    draw = ImageDraw.Draw(canvas)
    for i, record in enumerate(records):
        img = Image.open(record["out"]).convert("RGB")
        img.thumbnail((650, 430), Image.Resampling.LANCZOS)
        col = i % 2
        row = i // 2
        x = 60 + col * 750 + (650 - img.width) // 2
        y = 35 + row * 485 + (430 - img.height) // 2
        canvas.paste(img, (x, y))
        draw.text((60 + col * 750, 455 + row * 485), record["asset"], fill=(50, 50, 50), font=font)
    out = OUT_DIR / "abyss_dragon_seedling_chibi_v1_action_contact.png"
    canvas.save(out)
    return str(out)


def main() -> int:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    records = []
    for slug, action in TASKS:
        print(f"Generating {slug}...", flush=True)
        records.append(generate(slug, action))
        save_json(OUT_DIR / "abyss_dragon_seedling_chibi_v1_manifest.json", records)
        time.sleep(1.2)
    contact = make_contact(records)
    summary = {"records": records, "contact": contact}
    save_json(OUT_DIR / "abyss_dragon_seedling_chibi_v1_summary.json", summary)
    print(json.dumps(summary, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
