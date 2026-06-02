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

MODELS = [
    {"modelId": "NT4HQ78U2Q", "strength": 0.82},
    {"modelId": "y345YW8pyT", "strength": 0.22},
]

NEGATIVE = (
    "换角色, 黑发, 红色主色, 暗黑厚重, 写实照片, 3D, 低质量, 模糊, 裁切身体, 多人主角, "
    "过度性感, 暴露, 裸体, 巨乳, 低俗姿势, 复杂背景, 文字说明, logo, 水印, 丢失白发, 丢失青绿色龙翼, "
    "丢失浅薄荷绿色裙摆, 丢失小型使魔, 丢失金色胸口装饰"
)

BASE_PROMPT = (
    "Dragonfall日式二次元魔物娘Boss角色，严格参考给定角色图进行延展。"
    "角色核心：白色短发少女，闭眼温柔表情，青绿色半透明龙翼，浅薄荷青白色大裙摆，"
    "金色胸口宝石装饰，白金肩甲，小型白发龙娘使魔围绕，清透水晶/幼龙/芽龙气质。"
    "画风：唯美日系插画立绘，干净线稿，柔和赛璐璐加轻伪厚涂，明亮清透，白色或极浅色背景。"
    "角色不可换人，配色、发型、龙翼、裙摆、胸口金色宝石和使魔必须一致。"
)

ASSETS = [
    {
        "slug": "abyss_dragon_seedling_character_design_sheet",
        "ratio": "3:2",
        "prompt": (
            "生成角色设定图。画面包含：中央完整全身立绘一张，旁边展示头部特写、龙翼局部、胸口金色宝石局部、裙摆材质局部、"
            "小型使魔局部。所有元素都属于同一角色设计。无文字标签，白色背景，设定图排版清晰。"
        ),
    },
    {
        "slug": "abyss_dragon_seedling_turnaround_sheet",
        "ratio": "3:2",
        "prompt": (
            "生成三视图设定表。左到右为正面、侧面、背面，三个视图必须是同一个角色。"
            "中性站姿，双臂自然下垂或轻放身前，完整展示白发、青绿色半透明龙翼、浅薄荷青白裙摆、金色胸口宝石、白金肩甲。"
            "背面要清楚展示龙翼根部、裙摆后摆和发型。无文字标签，白色背景。"
        ),
    },
    {
        "slug": "abyss_dragon_seedling_expression_variants",
        "ratio": "3:2",
        "prompt": (
            "生成角色差分表。保持同一角色、同一服装、同一配色和同一龙翼设计，排列6个半身或膝上差分："
            "温柔闭眼微笑、睁眼平静、害羞脸红、Boss压迫感微笑、受击惊讶、释放技能。"
            "差分只改变表情、手势和少量特效，不改变角色设计。无文字标签，白色背景。"
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


def generate(asset: dict) -> dict:
    out_png = OUT_DIR / f"{asset['slug']}.png"
    payload_json = OUT_DIR / f"{asset['slug']}.payload.json"
    response_json = OUT_DIR / f"{asset['slug']}.response.json"
    payload = {
        "modelDetailList": MODELS,
        "prompt": BASE_PROMPT + asset["prompt"],
        "negativePrompt": NEGATIVE,
        "seed": -1,
        "aspectRatios": asset["ratio"],
        "simpleBackground": True,
        "batchSize": 1,
        "imageReference": REFERENCE_URL,
        "referenceMode": "standard",
        "referenceWeight": 1,
    }
    save_json(payload_json, payload)
    try:
        client_id = submit_and_wait(payload, out_png, response_json)
    except Exception:
        payload["modelDetailList"] = [MODELS[0]]
        save_json(payload_json, {**payload, "fallback": "single_model"})
        client_id = submit_and_wait(payload, out_png, response_json)
    return {
        "asset": asset["slug"],
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
        img.thumbnail((420, 280), Image.Resampling.LANCZOS)
        thumbs.append((record, img.copy()))
    cell_w, cell_h = 480, 350
    sheet = Image.new("RGB", (cell_w * len(thumbs), cell_h), "white")
    draw = ImageDraw.Draw(sheet)
    font = ImageFont.load_default()
    for i, (record, img) in enumerate(thumbs):
        x = i * cell_w
        sheet.paste(img, (x + (cell_w - img.width) // 2, 18))
        draw.text((x + 12, 310), record["asset"], fill=(20, 20, 20), font=font)
    out = OUT_DIR / "abyss_dragon_seedling_sheets_contact.jpg"
    sheet.save(out, quality=92)
    return out


def main() -> int:
    records = []
    for asset in ASSETS:
        print(f"Generating {asset['slug']}...", flush=True)
        records.append(generate(asset))
        save_json(OUT_DIR / "abyss_dragon_seedling_sheets_manifest.json", records)
        time.sleep(1.2)
    contact = make_contact_sheet(records)
    summary = {"records": records, "contact_sheet": str(contact) if contact else None, "reference": REFERENCE_URL}
    save_json(OUT_DIR / "abyss_dragon_seedling_sheets_summary.json", summary)
    print(json.dumps(summary, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
