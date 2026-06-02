from __future__ import annotations

import json
import shutil
from pathlib import Path


ROOT = Path(r"C:\Users\Administrator\Desktop\Dragonfall\AIResult")

TARGETS = {
    "00_references": [
        "00_references_originals",
    ],
    "01_prompts_docs": [
        "01_style_prompts_docs",
    ],
    "02_characters": [
        "02_slime_girls",
        "04_detective_girl",
        "05_deleijieen",
        "06_jellyfish_girl",
    ],
    "03_enemies_bosses": [
        "03_enemy_pixel_characters",
        "11_batch_character_designs",
        "12_boss_rerun_monster_only",
    ],
    "04_ui_vfx": [
        "07_ui_vfx",
    ],
    "05_api_metadata_tools": [
        "08_holopix_metadata",
        "09_scripts",
    ],
}


def unique_destination(target_dir: Path, src: Path) -> Path:
    candidate = target_dir / src.name
    if not candidate.exists():
        return candidate
    stem = src.stem
    suffix = src.suffix
    index = 2
    while True:
        candidate = target_dir / f"{stem}_{index}{suffix}"
        if not candidate.exists():
            return candidate
        index += 1


def move_contents(src_dir: Path, target_dir: Path) -> list[dict[str, str]]:
    moved = []
    if not src_dir.exists():
        return moved
    target_dir.mkdir(parents=True, exist_ok=True)
    for item in src_dir.iterdir():
        dest = unique_destination(target_dir, item)
        shutil.move(str(item), str(dest))
        moved.append({"from": str(item), "to": str(dest)})
    try:
        src_dir.rmdir()
    except OSError:
        pass
    return moved


def main() -> int:
    report = []
    for target_name, source_names in TARGETS.items():
        target_dir = ROOT / target_name
        for source_name in source_names:
            src_dir = ROOT / source_name
            if src_dir.resolve() == target_dir.resolve():
                continue
            report.extend(move_contents(src_dir, target_dir))

    report_path = ROOT / "05_api_metadata_tools" / "airesult_consolidation_report.json"
    report_path.parent.mkdir(parents=True, exist_ok=True)
    report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps({"moved": len(report), "report": str(report_path)}, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
