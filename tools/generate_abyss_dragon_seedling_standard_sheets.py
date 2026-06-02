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

MODELS = [{"modelId": "NT4HQ78U2Q", "strength": 0.86}]

NEGATIVE = (
    "多人, 两个角色, 三个角色, 分身, 使魔, 小人偶, chibi mascot, 宠物, 群像, 双人构图, "
    "重复人物, 克隆角色, 半身裁切, 大头贴, 复杂背景, 场景背景, 文字标签, logo, 水印, "
    "动态战斗姿势, 坐姿, 俯视, 透视夸张, 换装, 换发色, 黑发, 红色主色, 暗黑写实, 3D, 写实照片, "
    "过度性感, 暴露, 裸体, 巨乳, 低俗姿势, 丢失龙翼, 丢失白发, 丢失浅青白裙摆"
)

IDENTITY = (
    "同一名单人角色：白色短发少女，闭眼或温柔浅色眼睛，头顶小型薄荷青龙角/芽冠，"
    "背后大型青绿色半透明龙翼，浅薄荷青白色大裙摆，白金肩甲，胸口金色宝石装饰，"
    "整体是清透幼龙/芽龙魔物娘Boss。只画角色本人，不画小使魔、不画第二个人。"
)

STYLE = (
    "Dragonfall日式二次元魔物娘角色设定，唯美日系插画立绘，干净线稿，柔和赛璐璐加轻伪厚涂，"
    "白色纯背景，生产用角色设计稿，轮廓清楚，服装结构稳定。"
)

ASSETS = [
    {
        "slug": "abyss_dragon_seedling_standard_character_sheet",
        "ratio": "3:2",
        "prompt": (
            f"{STYLE}{IDENTITY}"
            "生成标准单人角色设定图：画面中央只有一张完整全身主立绘，角色中性站姿，双手自然放在身前。"
            "画面右侧可以放置局部细节小窗：头部、龙翼、胸口宝石、裙摆材质；局部小窗只显示部件，不出现第二个完整人物。"
            "不要表情差分，不要多个姿势，不要使魔，不要宠物。白色背景，无文字。"
        ),
    },
    {
        "slug": "abyss_dragon_seedling_standard_turnaround",
        "ratio": "3:2",
        "prompt": (
            f"{STYLE}{IDENTITY}"
            "生成标准三视图：同一个单人角色，严格横向排列三个完整全身正交视图。"
            "左：front view 正面；中：side view 侧面；右：back view 背面。"
            "三个视图必须相同身高、相同头身比例、相同服装、相同发型、相同龙翼和裙摆结构。"
            "中性站姿，双臂自然下垂或轻放身前，不能有动态姿势。"
            "背面必须清楚展示龙翼根部、后发、裙摆后摆。不要局部小窗，不要使魔，不要第二个人，不要文字标签，白色背景。"
        ),
    },
    {
        "slug": "abyss_dragon_seedling_standard_expression_sheet",
        "ratio": "3:2",
        "prompt": (
            f"{STYLE}{IDENTITY}"
            "生成标准单人表情差分表：同一个角色的6个头像或胸像格子，排列整齐。"
            "只改变表情，不改变服装、发型、角、配色。表情包括：温柔闭眼微笑、睁眼平静、害羞脸红、Boss压迫感微笑、受击惊讶、释放技能专注。"
            "每格只画同一角色的头肩部，不画全身，不画使魔，不画第二个人，不要文字标签，白色背景。"
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
        "prompt": asset["prompt"],
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
    out = OUT_DIR / "abyss_dragon_seedling_standard_sheets_contact.jpg"
    sheet.save(out, quality=92)
    return out


def main() -> int:
    records = []
    for asset in ASSETS:
        print(f"Generating {asset['slug']}...", flush=True)
        records.append(generate(asset))
        save_json(OUT_DIR / "abyss_dragon_seedling_standard_sheets_manifest.json", records)
        time.sleep(1.2)
    contact = make_contact_sheet(records)
    summary = {"records": records, "contact_sheet": str(contact) if contact else None, "reference": REFERENCE_URL}
    save_json(OUT_DIR / "abyss_dragon_seedling_standard_sheets_summary.json", summary)
    print(json.dumps(summary, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
