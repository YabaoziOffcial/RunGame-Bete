from __future__ import annotations

import json
import sys
import time
from pathlib import Path

sys.path.insert(0, r"C:\Users\Administrator\.codex\skills\holopixai\scripts")
import holopixai_generate as h  # noqa: E402


BASE_URL = "https://api.holopix.cn"
OUT_DIR = Path(r"C:\Users\Administrator\Desktop\Dragonfall\AIResult\03_enemies_bosses")
MODELS = [{"modelId": "NT4HQ78U2Q", "strength": 0.92}]

CHARACTER = (
    "solo young female abyss dragon seedling monster girl for Dragonfall, "
    "Japanese anime game art, white short hair, two small mint sprout-shaped dragon horns, "
    "large translucent aqua dragon wings, white and pale mint fantasy dress, "
    "white-gold shoulder armor, small gold gemstone at chest, cyan crystal accents, "
    "cute elegant monster-girl boss, soft clean face, blue-green eyes"
)

STYLE = (
    "polished 2D anime game character art, clean linework, soft cel shading, "
    "light neutral plain white background, centered composition, full body visible, feet visible"
)

NEGATIVE = (
    "multiple people, second character, third character, clone, duplicate, extra body, extra head, companion, servant, pet, mascot, "
    "small inset image, panel, collage, contact sheet, turnaround sheet, character sheet, expression sheet, grid, comparison layout, "
    "text, label, logo, watermark, cropped body, cut off feet, half body, sitting, kneeling, lying, battle action, dramatic pose, "
    "side-by-side views, front and back together, chibi, photorealistic, 3D, dark horror, black hair, red costume, nude, sexualized"
)


TASKS = [
    {
        "slug": "abyss_dragon_seedling_v4_design_solo",
        "ratio": "2:3",
        "prompt": (
            f"{STYLE}. {CHARACTER}. "
            "One single complete character only. She stands calmly facing the viewer in a neutral front three-quarter pose. "
            "No other figure, no small portrait, no decorative panel. Keep the image as one centered full-body solo illustration."
        ),
    },
    {
        "slug": "abyss_dragon_seedling_v4_front",
        "ratio": "2:3",
        "prompt": (
            f"{STYLE}. {CHARACTER}. "
            "One single complete character only. The character directly faces the viewer from the front. "
            "Neutral standing pose, arms relaxed downward, symmetrical silhouette, feet visible. "
            "No extra views, no extra heads, no inset images."
        ),
    },
    {
        "slug": "abyss_dragon_seedling_v4_side",
        "ratio": "2:3",
        "prompt": (
            f"{STYLE}. {CHARACTER}. "
            "One single complete character only. Pure left-facing side profile view, body shown from the side. "
            "Neutral standing pose, arms relaxed downward, feet visible. "
            "Show the side shape of the wing, dress, hair, horns, and shoulder armor. No extra views."
        ),
    },
    {
        "slug": "abyss_dragon_seedling_v4_back",
        "ratio": "2:3",
        "prompt": (
            f"{STYLE}. {CHARACTER}. "
            "One single complete character only. The character is turned away from the viewer and only her back is visible. "
            "Neutral standing pose, arms relaxed downward, feet visible. "
            "Show wing roots, back hair shape, rear dress design, and back armor. No front face, no side view, no extra views."
        ),
    },
]

EXPRESSIONS = [
    ("gentle_smile", "gentle closed-eye smile"),
    ("neutral", "calm neutral expression"),
    ("shy_blush", "shy blush, fragile and cute"),
    ("boss_smile", "quiet confident boss-like smile"),
    ("hurt_surprise", "surprised hurt reaction"),
    ("casting_focus", "focused magic casting expression"),
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


def generate(slug: str, ratio: str, prompt: str) -> dict:
    out_png = OUT_DIR / f"{slug}.png"
    payload_json = OUT_DIR / f"{slug}.payload.json"
    response_json = OUT_DIR / f"{slug}.response.json"
    if out_png.exists() and out_png.stat().st_size > 0:
        return {"asset": slug, "out": str(out_png), "payload_json": str(payload_json), "response_json": str(response_json), "client_id": "existing"}

    payload = {
        "modelDetailList": MODELS,
        "prompt": prompt,
        "negativePrompt": NEGATIVE,
        "seed": -1,
        "aspectRatios": ratio,
        "simpleBackground": True,
        "batchSize": 1,
    }
    save_json(payload_json, payload)
    client_id = submit_and_wait(payload, out_png, response_json)
    return {"asset": slug, "out": str(out_png), "payload_json": str(payload_json), "response_json": str(response_json), "client_id": client_id}


def make_sheets(records: list[dict]) -> dict[str, str]:
    from PIL import Image, ImageDraw, ImageFont

    def thumb(path: str, size: tuple[int, int]) -> Image.Image:
        img = Image.open(path).convert("RGB")
        img.thumbnail(size, Image.Resampling.LANCZOS)
        return img

    by_asset = {r["asset"]: r for r in records}
    font = ImageFont.load_default()

    turn = Image.new("RGB", (1536, 1024), "white")
    draw = ImageDraw.Draw(turn)
    for i, key in enumerate(["abyss_dragon_seedling_v4_front", "abyss_dragon_seedling_v4_side", "abyss_dragon_seedling_v4_back"]):
        img = thumb(by_asset[key]["out"], (430, 900))
        x = 70 + i * 500 + (430 - img.width) // 2
        y = 30 + (900 - img.height) // 2
        turn.paste(img, (x, y))
        draw.text((70 + i * 500, 960), ["front", "side", "back"][i], fill=(70, 70, 70), font=font)
    turnaround = OUT_DIR / "abyss_dragon_seedling_v4_standard_turnaround.png"
    turn.save(turnaround)

    expr = Image.new("RGB", (1536, 1024), "white")
    draw = ImageDraw.Draw(expr)
    for i, (name, _) in enumerate(EXPRESSIONS):
        key = f"abyss_dragon_seedling_v4_expr_{name}"
        img = thumb(by_asset[key]["out"], (460, 430))
        col = i % 3
        row = i // 3
        x = 40 + col * 500 + (460 - img.width) // 2
        y = 35 + row * 485 + (430 - img.height) // 2
        expr.paste(img, (x, y))
        draw.text((40 + col * 500, 455 + row * 485), name, fill=(70, 70, 70), font=font)
    expr_sheet = OUT_DIR / "abyss_dragon_seedling_v4_expression_variants.png"
    expr.save(expr_sheet)

    contact = Image.new("RGB", (1600, 920), "white")
    draw = ImageDraw.Draw(contact)
    for i, record in enumerate(records[:10]):
        img = thumb(record["out"], (290, 380))
        col = i % 5
        row = i // 5
        x = 20 + col * 315 + (290 - img.width) // 2
        y = 20 + row * 445 + (380 - img.height) // 2
        contact.paste(img, (x, y))
        draw.text((20 + col * 315, 400 + row * 445), record["asset"], fill=(40, 40, 40), font=font)
    contact_path = OUT_DIR / "abyss_dragon_seedling_v4_contact.jpg"
    contact.save(contact_path, quality=92)
    return {"turnaround": str(turnaround), "expression_sheet": str(expr_sheet), "contact": str(contact_path)}


def main() -> int:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    records = []
    for task in TASKS:
        print(f"Generating {task['slug']}...", flush=True)
        records.append(generate(task["slug"], task["ratio"], task["prompt"]))
        save_json(OUT_DIR / "abyss_dragon_seedling_v4_manifest.json", records)
        time.sleep(1.2)

    for name, expression in EXPRESSIONS:
        slug = f"abyss_dragon_seedling_v4_expr_{name}"
        prompt = (
            f"polished 2D Japanese anime game portrait, plain white background. {CHARACTER}. "
            f"One single head-and-shoulders portrait only, {expression}. "
            "Same hairstyle, horns, eyes, costume collar, and color palette. No body sheet, no extra faces, no grid, no text."
        )
        print(f"Generating {slug}...", flush=True)
        records.append(generate(slug, "1:1", prompt))
        save_json(OUT_DIR / "abyss_dragon_seedling_v4_manifest.json", records)
        time.sleep(1.2)

    sheets = make_sheets(records)
    summary = {"records": records, **sheets}
    save_json(OUT_DIR / "abyss_dragon_seedling_v4_summary.json", summary)
    print(json.dumps(summary, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
