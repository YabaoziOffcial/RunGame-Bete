from __future__ import annotations

import json
import sys
import time
from pathlib import Path

sys.path.insert(0, r"C:\Users\Administrator\.codex\skills\holopixai\scripts")
import holopixai_generate as h  # noqa: E402


OUT_DIR = Path(r"C:\Users\Administrator\Desktop\Dragonfall\AIResult\12_boss_rerun_monster_only")
BASE_URL = "https://api.holopix.cn"

BOSS_MODELS = [
    {"modelId": "2F4G6YWUH3", "strength": 0.85},
    {"modelId": "U3NA67CT2K", "strength": 0.25},
]
PIXEL_MODEL = [{"modelId": "ed45p7kmN3", "strength": 0.95}]

NEGATIVE = (
    "人类, 美少女, 女孩, 女性身体, 人形身体, 人类四肢, 人类皮肤, 魔物娘, 拟人化少女, "
    "龙娘, 角娘, 女仆, JK, 裙子, 胸部, 长发, 人脸美少女, 站立少女, Q版女孩, "
    "过度性感, 暴露, 写实照片, 3D, 复杂背景, 文字, logo, 水印, 裁切主体, 低质量, 模糊"
)

STYLE = (
    "Dragonfall游戏Boss怪物设计，日式二次元幻想游戏美术，干净线稿，柔和赛璐璐上色，"
    "明亮温暖但有Boss压迫感，轮廓清晰，适合幸存者类俯视战斗识别，白色或浅色纯背景。"
)

BOSSES = {
    "king_slime_boss": {
        "name": "史莱姆王 Boss",
        "core": (
            "非人形巨型史莱姆Boss，巨大半透明蓝绿色果冻身体，顶部戴金色王冠，身体内部有多个小史莱姆核心、气泡和水晶状黏液团，"
            "表情可爱但有威胁感，圆润厚重的怪物轮廓，森林Boss气质。严格不是少女，没有头发，没有裙子，没有胸部，没有人类手脚。"
        ),
    },
    "crystal_skull_boss": {
        "name": "水晶骷髅 Boss",
        "core": (
            "非人形漂浮巨型水晶骷髅Boss，骨白色头骨结构嵌入深紫水晶，眼窝发出紫红光，周围漂浮破碎水晶、骨片和柔和魔法雾气，"
            "轻度可爱幻想恐怖但不是黑暗写实，轮廓像完整骷髅Boss。严格没有人类少女脸、身体、头发、角娘元素。"
        ),
    },
    "abyss_dragon_boss": {
        "name": "深渊黑龙 Boss",
        "core": (
            "非人形西方巨龙Boss，完整怪物龙身体，黑曜石鳞片，橙红熔岩裂纹，巨大蝙蝠翼，龙头、犄角、利爪、长尾清晰可见，"
            "火山最终Boss剪影，威严但仍符合二次元幻想游戏。严格不是龙娘，没有人类躯干，没有人类脸，没有头发。"
        ),
    },
}

ASSETS = [
    {
        "suffix": "monster_design",
        "ratio": "3:4",
        "models": BOSS_MODELS,
        "prompt": "单体Boss怪物设定立绘，全身完整展示，正面偏三分之二角度，主体占画面中央，清楚展示材质、体块和核心特征。",
    },
    {
        "suffix": "monster_turnaround",
        "ratio": "3:2",
        "models": BOSS_MODELS,
        "prompt": "Boss怪物三视图设定表，front view、side view、back view 横向排列，同一只怪物，比例、颜色、轮廓、装饰完全一致，无文字标签。",
    },
    {
        "suffix": "pixel_boss_sheet_16bit",
        "ratio": "1:1",
        "models": PIXEL_MODEL,
        "prompt": "16bit像素游戏Boss素材表，真正像素画风，清晰硬边像素块，包含idle待机、attack攻击、hit受击、death死亡四个小动作姿态，白色背景，适合独立游戏素材制作。",
    },
]


def save_json(path: Path, data: object) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
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
    raise RuntimeError(f"Timed out: {client_id}")


def generate_one(slug: str, boss: dict, asset: dict) -> dict:
    out_png = OUT_DIR / f"{slug}_{asset['suffix']}.png"
    response_json = OUT_DIR / f"{slug}_{asset['suffix']}.response.json"
    payload_json = OUT_DIR / f"{slug}_{asset['suffix']}.payload.json"

    prompt = (
        f"{STYLE}\n"
        f"主题：{boss['name']}。\n"
        f"核心设计：{boss['core']}\n"
        f"画面要求：{asset['prompt']}\n"
        "关键约束：非人形怪物主体，不是魔物娘，不是美少女，不是人类角色。主体必须一眼看出是Boss怪物。"
    )
    payload = {
        "modelDetailList": asset["models"],
        "prompt": prompt,
        "negativePrompt": NEGATIVE,
        "seed": -1,
        "aspectRatios": asset["ratio"],
        "simpleBackground": True,
        "batchSize": 1,
    }
    save_json(payload_json, payload)

    try:
        client_id = submit_and_wait(payload, out_png, response_json)
    except Exception as exc:
        if asset["models"] == BOSS_MODELS:
            payload["modelDetailList"] = [BOSS_MODELS[0]]
            save_json(payload_json, payload)
            client_id = submit_and_wait(payload, out_png, response_json)
        else:
            raise exc

    return {
        "slug": slug,
        "asset": asset["suffix"],
        "out": str(out_png),
        "response_json": str(response_json),
        "payload_json": str(payload_json),
        "client_id": client_id,
        "models": payload["modelDetailList"],
    }


def make_contact_sheet(records: list[dict]) -> Path | None:
    try:
        from PIL import Image, ImageDraw, ImageFont
    except Exception:
        return None

    thumbs = []
    for record in records:
        path = Path(record["out"])
        if not path.exists():
            continue
        img = Image.open(path).convert("RGB")
        img.thumbnail((360, 280), Image.Resampling.LANCZOS)
        thumbs.append((record, img.copy()))

    if not thumbs:
        return None

    cols = 3
    cell_w, cell_h = 420, 340
    rows = (len(thumbs) + cols - 1) // cols
    sheet = Image.new("RGB", (cols * cell_w, rows * cell_h), "white")
    draw = ImageDraw.Draw(sheet)
    font = ImageFont.load_default()
    for i, (record, img) in enumerate(thumbs):
        x = (i % cols) * cell_w
        y = (i // cols) * cell_h
        sheet.paste(img, (x + (cell_w - img.width) // 2, y + 18))
        draw.text((x + 12, y + 300), f"{record['slug']} / {record['asset']}", fill=(20, 20, 20), font=font)

    out = OUT_DIR / "boss_rerun_overview_contact_sheet.jpg"
    sheet.save(out, quality=92)
    return out


def main() -> int:
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    records = []
    for slug, boss in BOSSES.items():
        for asset in ASSETS:
            print(f"Generating {slug} {asset['suffix']}...", flush=True)
            record = generate_one(slug, boss, asset)
            records.append(record)
            save_json(OUT_DIR / "boss_rerun_manifest.json", records)
            time.sleep(1.2)

    contact = make_contact_sheet(records)
    summary = {"records": records, "contact_sheet": str(contact) if contact else None}
    save_json(OUT_DIR / "boss_rerun_summary.json", summary)
    print(json.dumps(summary, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
