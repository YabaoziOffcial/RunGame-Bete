from pathlib import Path
from collections import deque
from PIL import Image


SRC = Path(r"C:\Users\18022\Downloads\经验矿石，晶核.png")
OUT_DIR = Path(r"D:\UnityProject\Vampire-Survivor-like\AIResult\exp_crystals_64")
TARGET = 64


def is_background(r, g, b):
    # The generated sheet uses a white/gray checkerboard. Keep dark outlines,
    # saturated crystal colors, and bright highlights while dropping low-sat bg.
    mx = max(r, g, b)
    mn = min(r, g, b)
    sat = mx - mn
    if mx > 185 and sat < 38:
        return True
    if mx > 210 and sat < 55:
        return True
    return False


def build_mask(img):
    w, h = img.size
    px = img.load()
    mask = bytearray(w * h)
    for y in range(h):
        for x in range(w):
            r, g, b = px[x, y]
            if not is_background(r, g, b):
                mask[y * w + x] = 1
    return mask


def components(mask, w, h):
    seen = bytearray(w * h)
    found = []
    for y in range(h):
        for x in range(w):
            idx = y * w + x
            if not mask[idx] or seen[idx]:
                continue
            q = deque([(x, y)])
            seen[idx] = 1
            min_x = max_x = x
            min_y = max_y = y
            count = 0
            while q:
                cx, cy = q.popleft()
                count += 1
                if cx < min_x:
                    min_x = cx
                if cx > max_x:
                    max_x = cx
                if cy < min_y:
                    min_y = cy
                if cy > max_y:
                    max_y = cy
                for nx, ny in ((cx + 1, cy), (cx - 1, cy), (cx, cy + 1), (cx, cy - 1)):
                    if nx < 0 or ny < 0 or nx >= w or ny >= h:
                        continue
                    ni = ny * w + nx
                    if mask[ni] and not seen[ni]:
                        seen[ni] = 1
                        q.append((nx, ny))
            bw = max_x - min_x + 1
            bh = max_y - min_y + 1
            if count > 3000 and bw > 80 and bh > 80:
                found.append((min_x, min_y, max_x + 1, max_y + 1, count))
    return found


def merge_boxes(boxes):
    # Component detection may split highlights from outlines. Merge by coarse 3x3
    # spatial slots, then use the union box in each slot.
    boxes = sorted(boxes, key=lambda b: b[4], reverse=True)
    boxes = boxes[:30]
    centers = [((b[0] + b[2]) / 2, (b[1] + b[3]) / 2, b) for b in boxes]
    xs = sorted(c[0] for c in centers)
    ys = sorted(c[1] for c in centers)
    # The useful objects occupy the central sheet area; split into approximate thirds.
    x_cuts = [(xs[2] + xs[-3]) / 2] if len(xs) < 9 else [700, 1280]
    y_cuts = [760, 1260]
    slots = {}
    for cx, cy, b in centers:
        if cy > 1800 or cx > 1850:
            continue
        col = 0 if cx < x_cuts[0] else 1 if cx < x_cuts[1] else 2
        row = 0 if cy < y_cuts[0] else 1 if cy < y_cuts[1] else 2
        key = (row, col)
        cur = slots.get(key)
        if cur is None:
            slots[key] = list(b[:4])
        else:
            cur[0] = min(cur[0], b[0])
            cur[1] = min(cur[1], b[1])
            cur[2] = max(cur[2], b[2])
            cur[3] = max(cur[3], b[3])
    return [tuple(slots[(r, c)]) for r in range(3) for c in range(3)]


def crop_with_alpha(img, box):
    pad = 16
    x1, y1, x2, y2 = box
    x1 = max(0, x1 - pad)
    y1 = max(0, y1 - pad)
    x2 = min(img.width, x2 + pad)
    y2 = min(img.height, y2 + pad)
    crop = img.crop((x1, y1, x2, y2)).convert("RGBA")
    px = crop.load()
    for y in range(crop.height):
        for x in range(crop.width):
            r, g, b, a = px[x, y]
            if is_background(r, g, b):
                px[x, y] = (r, g, b, 0)
    return crop


def fit_64(crop):
    bbox = crop.getbbox()
    if bbox:
        crop = crop.crop(bbox)
    scale = min((TARGET - 8) / crop.width, (TARGET - 8) / crop.height)
    nw = max(1, int(crop.width * scale))
    nh = max(1, int(crop.height * scale))
    resized = crop.resize((nw, nh), Image.Resampling.LANCZOS)
    out = Image.new("RGBA", (TARGET, TARGET), (0, 0, 0, 0))
    out.alpha_composite(resized, ((TARGET - nw) // 2, (TARGET - nh) // 2))
    return out


def main():
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    img = Image.open(SRC).convert("RGB")
    mask = build_mask(img)
    comps = components(mask, img.width, img.height)
    boxes = merge_boxes(comps)
    names = [
        "crystal_blue_01_64.png",
        "crystal_green_01_64.png",
        "crystal_green_02_64.png",
        "crystal_blue_02_64.png",
        "crystal_green_03_64.png",
        "crystal_black_01_64.png",
        "crystal_red_01_64.png",
        "crystal_black_02_64.png",
        "crystal_black_03_64.png",
    ]
    outputs = []
    for name, box in zip(names, boxes):
        out = fit_64(crop_with_alpha(img, box))
        path = OUT_DIR / name
        out.save(path)
        outputs.append(path)

    preview = Image.new("RGBA", (3 * 96, 3 * 96), (35, 35, 35, 255))
    for i, path in enumerate(outputs):
        tile = Image.open(path).convert("RGBA")
        preview.alpha_composite(tile, ((i % 3) * 96 + 16, (i // 3) * 96 + 16))
    preview.save(OUT_DIR / "preview_3x3.png")
    for path in outputs:
        print(path)
    print(OUT_DIR / "preview_3x3.png")


if __name__ == "__main__":
    main()
