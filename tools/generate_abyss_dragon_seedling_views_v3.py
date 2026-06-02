from __future__ import annotations

import json
import sys
import time
from pathlib import Path

sys.path.insert(0, r"C:\Users\Administrator\.codex\skills\holopixai\scripts")
import holopixai_generate as h  # noqa: E402


BASE_URL = "https://api.holopix.cn"
OUT_DIR = Path(r"C:\Users\Administrator\Desktop\Dragonfall\AIResult\03_enemies_bosses")
REFERENCE_URL = "https://genai.holopix.cn/2026-06-02/17803951517201780395221331-1c34cff5cb06f2ad8e7e69f7b4368eed_00001_.png"
MODELS = [{"modelId": "NT4HQ78U2Q", "strength": 0.9}]

IDENTITY = (
    "single character only, Dragonfall abyss dragon seedling anthro monster-girl boss, "
    "young elegant Japanese anime girl, short white hair, small mint-green dragon horns shaped like sprouts, "
    "large translucent cyan-green dragon wings, pale mint and white long fantasy dress, "
    "white and gold shoulder armor, gold gemstone on chest, cyan crystal magic details, "
    "soft cute but boss-like aura, clear monster-girl identity"
)

STYLE = (
    "Dragonfall game character production art, polished Japanese anime 2D game illustration, "
    "clean linework, soft cel shading, light fantasy mood, white or very light neutral background, "
    "full body, centered, no scene background, no text, no labels"
)

NEGATIVE = (
    "multiple characters, two girls, three girls, duplicate body, clone, companion, servant, pet, mascot, "
    "chibi mascot, group composition, small inset panels, detail windows, split-screen collage, comic panels, "
    "unrelated headshots, text, labels, logo, watermark, busy background, landscape background, cropped body, "
    "half body, sitting pose, dynamic battle pose, perspective distortion, fisheye, black hair, red main color, "
    "dark gritty fantasy, photorealistic, 3D render, excessive fanservice, nude, giant breasts, horror monster"
)

TASKS = [
    {
        "slug": "abyss_dragon_seedling_v3_single_design",
        "ratio": "3:4",
        "use_reference": True,
        "prompt": (
            f"{STYLE}. {IDENTITY}. "
            "Standard single-person character design illustration. Draw exactly one complete full-body character, "
            "front three-quarter neutral standing pose, arms relaxed, feet visible. "
            "Clearly show hairstyle, sprout dragon horns, translucent wings, shoulder armor, chest gemstone, "
            "long dress silhouette, and cyan crystal details. This is one main design figure, not a sheet layout."
        ),
    },
    {
        "slug": "abyss_dragon_seedling_v3_front_view",
        "ratio": "3:4",
        "use_reference": True,
        "prompt": (
            f"{STYLE}. {IDENTITY}. "
            "Strict orthographic front view turnaround reference. Draw exactly one complete full-body character. "
            "The character faces the viewer directly, neutral standing pose, arms naturally lowered, feet visible, "
            "symmetrical readable silhouette. Do not add extra poses, panels, portraits, or duplicate characters."
        ),
    },
    {
        "slug": "abyss_dragon_seedling_v3_side_view",
        "ratio": "3:4",
        "use_reference": False,
        "prompt": (
            f"{STYLE}. {IDENTITY}. "
            "Strict orthographic side view turnaround reference. Draw exactly one complete full-body character in pure profile, "
            "facing left, neutral standing pose, arms naturally lowered, feet visible. "
            "Clearly show wing thickness from the side, dress side silhouette, short hair profile, horn profile, "
            "and shoulder armor profile. Do not add any other character or inset detail."
        ),
    },
    {
        "slug": "abyss_dragon_seedling_v3_back_view",
        "ratio": "3:4",
        "use_reference": False,
        "prompt": (
            f"{STYLE}. {IDENTITY}. "
            "Strict orthographic back view turnaround reference. Draw exactly one complete full-body character from behind, "
            "neutral standing pose, arms naturally lowered, feet visible. "
            "Clearly show wing roots, back of the dress, back hair shape, rear armor structure, and back silhouette. "
            "Do not add front view, side view, extra characters, portraits, panels, or labels."
        ),
    },
    {
        "slug": "abyss_dragon_seedling_v3_expression_grid",
        "ratio": "3:2",
        "use_reference": True,
        "prompt": (
            f"{STYLE}. {IDENTITY}. "
            "Expression variant sheet for the same single character. Six head-and-shoulder portraits only, arranged in a clean 3 by 2 grid. "
            "Expressions: gentle closed-eye smile, calm neutral, shy blush, boss-like confident smile, surprised hurt reaction, focused casting magic. "
            "Keep hairstyle, horns, color palette, eye shape, and costume collar identical in every portrait. "
            "No full-body figures, no second character, no mascot, no text labels."
        ),
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

    deadline = time.time() + 480
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


def generate(task: dict) -> dict:
    out_png = OUT_DIR / f"{task['slug']}.png"
    payload_json = OUT_DIR / f"{task['slug']}.payload.json"
    response_json = OUT_DIR / f"{task['slug']}.response.json"
    if out_png.exists() and out_png.stat().st_size > 0:
        return {
            "asset": task["slug"],
            "out": str(out_png),
            "payload_json": str(payload_json),
            "response_json": str(response_json),
            "client_id": "existing",
        }
    payload = {
        "modelDetailList": MODELS,
        "prompt": task["prompt"],
        "negativePrompt": NEGATIVE,
        "seed": -1,
        "aspectRatios": task["ratio"],
        "simpleBackground": True,
        "batchSize": 1,
    }
    if task.get("use_reference"):
        payload.update(
            {
                "imageReference": REFERENCE_URL,
                "referenceMode": "standard",
                "referenceWeight": 1,
            }
        )
    save_json(payload_json, payload)
    client_id = submit_and_wait(payload, out_png, response_json)
    return {
        "asset": task["slug"],
        "out": str(out_png),
        "payload_json": str(payload_json),
        "response_json": str(response_json),
        "client_id": client_id,
    }


def make_sheets(records: list[dict]) -> dict[str, str]:
    from PIL import Image, ImageDraw, ImageFont

    def thumb(path: str, size: tuple[int, int]) -> Image.Image:
        img = Image.open(path).convert("RGB")
        img.thumbnail(size, Image.Resampling.LANCZOS)
        return img

    by_asset = {r["asset"]: r for r in records}
    font = ImageFont.load_default()

    view_keys = [
        "abyss_dragon_seedling_v3_front_view",
        "abyss_dragon_seedling_v3_side_view",
        "abyss_dragon_seedling_v3_back_view",
    ]
    turn = Image.new("RGB", (1536, 1024), "white")
    draw = ImageDraw.Draw(turn)
    for i, key in enumerate(view_keys):
        img = thumb(by_asset[key]["out"], (460, 900))
        x = 48 + i * 500 + (460 - img.width) // 2
        y = 24 + (900 - img.height) // 2
        turn.paste(img, (x, y))
        draw.text((48 + i * 500, 960), ["front view", "side view", "back view"][i], fill=(60, 60, 60), font=font)
    turnaround = OUT_DIR / "abyss_dragon_seedling_v3_assembled_turnaround.png"
    turn.save(turnaround)

    contact = Image.new("RGB", (1600, 760), "white")
    draw = ImageDraw.Draw(contact)
    for i, record in enumerate(records):
        img = thumb(record["out"], (300, 620))
        x = 20 + i * 315 + (300 - img.width) // 2
        y = 20 + (620 - img.height) // 2
        contact.paste(img, (x, y))
        draw.text((20 + i * 315, 670), record["asset"], fill=(40, 40, 40), font=font)
    contact_path = OUT_DIR / "abyss_dragon_seedling_v3_contact.jpg"
    contact.save(contact_path, quality=92)
    return {"assembled_turnaround": str(turnaround), "contact_sheet": str(contact_path)}


def main() -> int:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    records = []
    for task in TASKS:
        print(f"Generating {task['slug']}...", flush=True)
        records.append(generate(task))
        save_json(OUT_DIR / "abyss_dragon_seedling_v3_manifest.json", records)
        time.sleep(1.2)
    sheets = make_sheets(records)
    summary = {"records": records, **sheets}
    save_json(OUT_DIR / "abyss_dragon_seedling_v3_summary.json", summary)
    print(json.dumps(summary, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
