"""
Pre-tint affinity result icons for Prey Eyes 2.
Multiplies bright pixels by the tint color, leaving dark frame pixels untouched.
Output goes to Mods/icons/smt3/results/
"""
from PIL import Image
import os, sys

SRC_DIR = os.path.join(os.path.dirname(__file__), "..", "..", "ui models", "affinity result icons")
OUT_DIR = os.path.join(os.path.dirname(__file__), "..", "..", "Mods", "icons", "smt3", "results")

# Each result icon gets exactly one tint color (R, G, B floats 0-1)
TINTS = {
    "weak":    (0.2, 1.0, 0.15),   # green
    "resist":  (1.0, 0.9, 0.0),    # yellow
    "null":    (1.0, 0.15, 0.15),  # red
    "reflect": (1.0, 0.15, 0.15),  # red
    "drain":   (1.0, 0.15, 0.15),  # red
    "normal":  None,                # no tint, copy as-is
    "unknown": None,                # no tint, copy as-is
}

def tint_image(img, tint_rgb, dark_limit=30, blend_end=80):
    """
    Full multiplicative tint for smooth anti-aliased edges, but desaturate
    the darkest frame pixels back to neutral grey so the frame stays clean.
    - Below dark_limit: fully desaturated (neutral grey)
    - dark_limit to blend_end: blend from neutral to tinted
    - Above blend_end: fully tinted (glyph + anti-aliased edges)
    """
    img = img.convert("RGBA")
    pixels = img.load()
    w, h = img.size
    tr, tg, tb = tint_rgb
    blend_range = blend_end - dark_limit
    for y in range(h):
        for x in range(w):
            r, g, b, a = pixels[x, y]
            # Full multiplicative tint
            rt, gt, bt = int(r * tr), int(g * tg), int(b * tb)
            lum = (r + g + b) / 3.0
            if lum <= dark_limit:
                # Darkest frame pixels — desaturate to neutral grey
                avg = (rt + gt + bt) // 3
                pixels[x, y] = (avg, avg, avg, a)
            elif lum < blend_end:
                # Blend zone — smooth transition from neutral to tinted
                t = (lum - dark_limit) / blend_range
                avg = (rt + gt + bt) // 3
                pixels[x, y] = (
                    int(avg * (1 - t) + rt * t),
                    int(avg * (1 - t) + gt * t),
                    int(avg * (1 - t) + bt * t),
                    a
                )
            else:
                # Bright pixels — fully tinted
                pixels[x, y] = (rt, gt, bt, a)
    return img

def main():
    os.makedirs(OUT_DIR, exist_ok=True)

    for name, tint in TINTS.items():
        src_path = os.path.join(SRC_DIR, f"{name}.png")
        out_path = os.path.join(OUT_DIR, f"{name}.png")

        if not os.path.exists(src_path):
            print(f"  SKIP {name}.png — not found at {src_path}")
            continue

        img = Image.open(src_path)

        if tint is None:
            # No tint — copy as-is
            img.save(out_path)
            print(f"  COPY {name}.png ({img.size[0]}x{img.size[1]})")
        else:
            tinted = tint_image(img, tint)
            tinted.save(out_path)
            print(f"  TINT {name}.png -> ({tint[0]:.2f}, {tint[1]:.2f}, {tint[2]:.2f}) ({img.size[0]}x{img.size[1]})")

    print(f"\nDone. Output: {OUT_DIR}")

if __name__ == "__main__":
    main()
