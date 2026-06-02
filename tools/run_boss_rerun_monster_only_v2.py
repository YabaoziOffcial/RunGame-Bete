from __future__ import annotations

import json
import sys
import time
from pathlib import Path

sys.path.insert(0, r"C:\Users\Administrator\.codex\skills\holopixai\scripts")
import holopixai_generate as h  # noqa: E402


OUT_DIR = Path(r"C:\Users\Administrator\Desktop\Dragonfall\AIResult\12_boss_rerun_monster_only")
BASE_URL = "https://api.holopix.cn"

ILLUSTRATION_MODEL = [{"modelId": "y345YW8pyT", "strength": 0.72}]
PIXEL_MODEL = [{"modelId": "ed45p7kmN3", "strength": 0.98}]

NEGATIVE = (
    "人类, 骑士, 冒险者, 美少女, 女孩, 女性身体, 人形身体, 人类四肢, 人类皮肤, 魔物娘, 拟人化少女, "
    "龙娘, 角娘, 女仆, JK, 裙子, 胸部, 长发, 人脸美少女, 站立少女, Q版女孩, 战斗场景, 城堡背景, "
    "士兵, 武器持有者, 过度性感, 暴露, 写实照片, 3D, 复杂背景, 文字, logo, 水印, 裁切主体, 低质量, 模糊"
)

STYLE = (
    "Dragonfall游戏Boss怪物资产，非人形怪物，不是魔物娘，不是人类角色。"
    "日式二次元幻想游戏美术，干净线稿，柔和赛璐璐上色，清晰轮廓，白色纯背景，单独展示。"
)

BOSSES = {
    "king_slime_boss": {
        "name": "史莱姆王 Boss",
        "core": (
            "主体必须是一个巨大的圆润史莱姆团块，半透明蓝绿色果冻材质，顶部一顶金色王冠，体内有气泡、小史莱姆核心和水滴状高光。"
            "没有骨骼，没有鳞片，没有翅膀，没有爪子，没有人形躯干。"
        ),
        "identity_lock": "画面第一印象必须是：王冠蓝绿色史莱姆Boss。",
    },
    "crystal_skull_boss": {
        "name": "水晶骷髅 Boss",
        "core": (
            "主体必须是漂浮的巨大骷髅头，骨白色头骨，嵌入深紫色晶体，紫红发光眼窝，周围只允许漂浮水晶碎片和少量魔法雾。"
            "没有龙身体，没有兽爪，没有翅膀，没有人类少女脸，没有头发。"
        ),
        "identity_lock": "画面第一印象必须是：紫色水晶骷髅头Boss。",
    },
    "abyss_dragon_boss": {
        "name": "深渊黑龙 Boss",
        "core": (
            "主体必须是完整西方黑龙，黑曜石鳞片，橙红熔岩裂纹，巨大蝙蝠翼，龙头、龙角、四足、利爪、长尾完整可见。"
            "没有人类骑士，没有人类上半身，没有少女脸，没有头发。"
        ),
        "identity_lock": "画面第一印象必须是：黑色熔岩裂纹西方巨龙Boss。",
    },
}

ASSETS = [
    {
        "suffix": "v2_monster_design",
        "ratio": "3:4",
        "models": ILLUSTRATION_MODEL,
        "prompt": "单体Boss设定立绘，全身完整，正面偏三分之二角度，主体居中，纯白背景，无场景，无其他角色。",
    },
    {
        "suffix": "v2_monster_turnaround",
        "ratio": "3:2",
        "models": ILLUSTRATION_MODEL,
        "prompt": "三视图设定表：左正面、中侧面、右背面，三个都是同一只Boss，同一比例同一配色，正交视角，纯白背景，无文字标签，无场景。",
    },
    {
        "suffix": "v2_pixel_boss_sheet_16bit",
        "ratio": "1:1",
        "models": PIXEL_MODEL,
        "prompt": "16bit像素游戏Boss动作素材表，真正像素画风，硬边像素块，包含idle待机、attack攻击、hit受击、death死亡四个姿态，白色背景。",
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
        f"造型锁定：{boss['identity_lock']}\n"
        f"核心设计：{boss['core']}\n"
        f"画面要求：{asset['prompt']}\n"
        "严格要求：只画Boss本体，不能出现人类、骑士、少女、背景故事场景。"
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
    client_id = submit_and_wait(payload, out_png, response_json)
    return {
        "slug": slug,
        "asset": asset["suffix"],
        "out": str(out_png),
        "response_json": str(response_json),
        "payload_json": str(payload_json),
        "client_id": client_id,
        "models": asset["models"],
    }


def make_contact_sheet(records: list[dict]) -> Path | None:
    try:
        from PIL import Image, ImageDraw, ImageFont
    except Exception:
        return None
    thumbs = []
    for record in records:
        img_path = Path(record["out"])
        if img_path.exists():
            img = Image.open(img_path).convert("RGB")
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
    out = OUT_DIR / "boss_rerun_v2_overview_contact_sheet.jpg"
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
            save_json(OUT_DIR / "boss_rerun_v2_manifest.json", records)
            time.sleep(1.2)
    contact = make_contact_sheet(records)
    save_json(OUT_DIR / "boss_rerun_v2_summary.json", {"records": records, "contact_sheet": str(contact) if contact else None})
    print(json.dumps({"records": records, "contact_sheet": str(contact) if contact else None}, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
