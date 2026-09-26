"""Measure the FACE of each kit button blank: the coloured body inside the dark brown outline
(transparent canvas margins and the outline excluded; the bottom bevel stays IN, it is handled by
one global knob, UIStyles.BUTTON_FACE_LIP_FRAC, so every blank places its word the same way — a
per-art "darker band" test mis-read two-tone faces like the red CREDITS blank, 2026-09-17).
Prints C# initializer lines for UI/ButtonFace.cs (fractions of the canvas, left/top/right/bottom).
Re-run when a blank changes.

    python scratchpad/measure_button_faces.py
"""
from PIL import Image
from pathlib import Path

UI = Path(__file__).resolve().parents[1] / "Assets/_Project/Resources/UI"
ARTS = ["ui_play_button", "ui_btn_blue_wide", "ui_btn_green_wide", "ui_menu_btn_credits", "ui_menu_btn_shop",
        "ui_btn_green_big", "ui_btn_green", "ui_btn_yellow", "ui_btn_confirm_buy", "ui_btn_confirm_cancel",
        "ui_btn_blue_watch", "ui_btn_cream"]


def value(px):
    return max(px[0], px[1], px[2])


def face(im):
    im = im.convert("RGBA")
    w, h = im.size
    px = im.load()
    cx, cy = w // 2, h // 2
    OUTLINE_MAX_VALUE = 110           # the kit's outline is a near-black brown; every fill/bevel is brighter
    def bright(x, y):
        p = px[x, y]
        return p[3] > 200 and value(p) > OUTLINE_MAX_VALUE
    # scan outward from the centre along the middle row / column
    l = cx
    while l > 0 and bright(l - 1, cy): l -= 1
    r = cx
    while r < w - 1 and bright(r + 1, cy): r += 1
    t = cy
    while t > 0 and bright(cx, t - 1): t -= 1
    b = cy
    while b < h - 1 and bright(cx, b + 1): b += 1
    return l / w, t / h, (w - 1 - r) / w, (h - 1 - b) / h


for n in ARTS:
    p = UI / f"{n}.png"
    if not p.exists():
        print(f"// {n}: missing"); continue
    l, t, r, b = face(Image.open(p))
    print(f'            ["{n}"] = new Insets({l:.3f}f, {t:.3f}f, {r:.3f}f, {b:.3f}f),')
