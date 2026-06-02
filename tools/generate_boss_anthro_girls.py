from __future__ import annotations

import base64
import json
import mimetypes
import sys
import time
from pathlib import Path

sys.path.insert(0, r"C:\Users\Administrator\.codex\skills\holopixai\scripts")
import holopixai_generate as h  # noqa: E402


BASE_URL = "https://api.holopix.cn"
OUT_DIR = Path(r"C:\Users\Administrator\Desktop\Dragonfall\AIResult\03_enemies_bosses")

MODEL_LIST = [
    {"modelId": "NT4HQ78U2Q", "strength": 0.78},
    {"modelId": "y345YW8pyT", "strength": 0.35},
]

NEGATIVE = (
    "写实照片, 3D, 厚重暗黑写实, 恐怖猎奇, 老年, 男性, 低质量, 模糊, 裁切身体, 多人, "
    "复杂背景, 文字, logo, 水印, 过度性感, 暴露, 裸体, 巨乳, 夸张胸部, 低俗姿势, "
    "完全丢失参考图配色, 完全丢失参考图轮廓元素"
)

BASE_PROMPT = (
    "Dragonfall敌方Boss拟人魔物娘立绘，参考图生图重绘。"
    "日式二次元魔物娘风格，唯美日系插画立绘，干净线稿，柔和赛璐璐与轻伪厚涂结合，"
    "软萌、清透、幻想、可爱但有Boss气场。完整单人全身立绘，白色或极浅色背景，游戏角色设定图。"
    "角色为年轻女性魔物娘比例，约5-6头身，表情可爱但带一点危险感。"
    "服装不过度暴露，偏幻想学院/轻礼服/怪物材质融合设计。"
)

TASKS = [
    {
        "slug": "king_slime_blue_crystal_anthro_girl",
        "source": OUT_DIR / "king_slime_boss_v2_monster_design.png",
        "prompt": (
            "把参考图中的蓝色透明果冻晶体糖果形态拟人化为蓝色史莱姆系魔物娘。"
            "保留参考图的两个圆润透明蓝色核心、尖角水晶翼片、玻璃果冻高光和深蓝描边。"
            "设计为蓝发/青蓝透明发梢，头饰像两颗透明史莱姆晶核，肩部或裙摆有尖角水晶翅片。"
            "服装为白蓝色轻幻想连衣裙，裙摆和袖口像透明凝胶与水晶糖纸融合。"
        ),
    },
    {
        "slug": "crystal_skull_soft_anthro_girl",
        "source": OUT_DIR / "crystal_skull_boss_v2_monster_turnaround.png",
        "prompt": (
            "把参考图中的白色软壳、深蓝面罩和粉色眼睛拟人化为水晶骷髅系魔物娘。"
            "保留白色圆润外壳轮廓、深蓝面罩区域、粉色发光眼睛、青蓝边线。"
            "角色戴白色骨壳兜帽或披肩，脸部有深蓝面罩式装饰，眼睛为粉紫发光感。"
            "整体不是恐怖骷髅，而是可爱、安静、带Boss神秘感的骨晶魔物娘。"
        ),
    },
    {
        "slug": "abyss_dragon_seedling_anthro_girl",
        "source": OUT_DIR / "abyss_dragon_boss_v2_monster_design.png",
        "prompt": (
            "把参考图中小型淡黄色圆体和薄荷绿色叶冠拟人化为深渊幼龙/芽龙魔物娘。"
            "保留淡黄色圆润主体、薄荷绿色尖叶状头冠、柔和浅色配色和小巧可爱轮廓。"
            "角色有浅金色短发或团子发，头顶薄荷绿色龙角/叶冠，背后小型叶片龙翼，尾巴短小。"
            "服装为淡黄与薄荷绿的幻想轻装，带幼龙Boss的徽记和可爱压迫感。"
        ),
    },
    {
        "slug": "king_slime_blue_white_anthro_girl",
        "source": OUT_DIR / "king_slime_boss_v2_monster_turnaround.png",
        "prompt": (
            "把参考图中的蓝白色圆润史莱姆/海兽形态拟人化为蓝白史莱姆魔物娘。"
            "保留白色上半圆盖、蓝色下半透明身体、深蓝小眼点、扁平柔软轮廓和水滴感。"
            "角色有白到蓝渐变长发，发尾像流动史莱姆，头部或披肩像白色软盖。"
            "服装为蓝白水流质感连衣裙，裙摆像软史莱姆波浪，整体温柔、软萌、适合Boss娘化立绘。"
        ),
    },
]


def image_to_data_uri(path: Path) -> str:
    mime = mimetypes.guess_type(path.name)[0] or "image/png"
    encoded = base64.b64encode(path.read_bytes()).decode("ascii")
    return f"data:{mime};base64,{encoded}"


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


def generate(task: dict) -> dict:
    out_png = OUT_DIR / f"{task['slug']}.png"
    payload_json = OUT_DIR / f"{task['slug']}.payload.json"
    response_json = OUT_DIR / f"{task['slug']}.response.json"
    prompt = BASE_PROMPT + task["prompt"]
    payload = {
        "modelDetailList": MODEL_LIST,
        "prompt": prompt,
        "negativePrompt": NEGATIVE,
        "seed": -1,
        "aspectRatios": "3:4",
        "simpleBackground": True,
        "batchSize": 1,
    }
    save_json(payload_json, {**payload, "reference_source": str(task["source"]), "mode": "t2i_from_reference_description"})
    try:
        client_id = submit_and_wait(payload, out_png, response_json)
    except Exception:
        payload["modelDetailList"] = [MODEL_LIST[0]]
        save_json(payload_json, {**payload, "reference_source": str(task["source"]), "mode": "t2i_from_reference_description", "fallback": "single_model"})
        client_id = submit_and_wait(payload, out_png, response_json)
    return {
        "slug": task["slug"],
        "source": str(task["source"]),
        "out": str(out_png),
        "payload_json": str(payload_json),
        "response_json": str(response_json),
        "client_id": client_id,
    }


def make_contact_sheet(records: list[dict]) -> Path | None:
    try:
        from PIL import Image, ImageDraw, ImageFont
    except Exception:
        return None

    thumbs = []
    for record in records:
        img = Image.open(record["out"]).convert("RGB")
        img.thumbnail((300, 360), Image.Resampling.LANCZOS)
        thumbs.append((record, img.copy()))
    cell_w, cell_h = 360, 430
    cols = 4
    sheet = Image.new("RGB", (cols * cell_w, cell_h), "white")
    draw = ImageDraw.Draw(sheet)
    font = ImageFont.load_default()
    for i, (record, img) in enumerate(thumbs):
        x = i * cell_w
        sheet.paste(img, (x + (cell_w - img.width) // 2, 18))
        draw.text((x + 12, 386), record["slug"], fill=(20, 20, 20), font=font)
    out = OUT_DIR / "boss_anthro_girls_contact_sheet.jpg"
    sheet.save(out, quality=92)
    return out


def main() -> int:
    records = []
    for task in TASKS:
        print(f"Generating {task['slug']}...", flush=True)
        records.append(generate(task))
        save_json(OUT_DIR / "boss_anthro_girls_manifest.json", records)
        time.sleep(1.2)
    contact = make_contact_sheet(records)
    summary = {"records": records, "contact_sheet": str(contact) if contact else None}
    save_json(OUT_DIR / "boss_anthro_girls_summary.json", summary)
    print(json.dumps(summary, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
