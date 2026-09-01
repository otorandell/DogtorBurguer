# Builds the one Shop piece still DERIVED from the kit: the wide green cell pill. The 2026-09-01
# kit drop has every other shop piece (see gen_shop_art.ps1) but no wide green blank, so
# ui_btn_green_wide.png is Settings_Button.png (the wide blue blank) hue-shifted to the kit green;
# the dark brown outline/shadow and the near-white highlight are left untouched (value < 0.35 or
# sat < 0.08). Output goes to OUT; import with a gen_*_art.ps1 recipe. Requires Pillow.
# (build_page / the red pill were used by the pre-kit stand-in shop and are kept for reference.)
import colorsys, os, sys
from PIL import Image

KIT = r'C:\Users\oscar\Documents\Repos\DogtorBurguer\Assets\RawArt\Dogtor Burger-20260720T104655Z-1-001\Dogtor Burger\Assets\Settings'
OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.dirname(os.path.abspath(__file__))

def build_page():
    im = Image.open(os.path.join(KIT, 'Settings_Background.png')).convert('RGBA')
    W, H = im.size
    top = im.crop((0, 0, W, 3100))
    chunk = im.crop((0, 2400, W, 3100))
    part = im.crop((0, 2400, W, 2680))
    bottom = im.crop((0, 3100, W, H))
    pieces = (top, chunk, chunk, part, bottom)
    out = Image.new('RGBA', (W, sum(p.size[1] for p in pieces)))
    y = 0
    for p in pieces:
        out.paste(p, (0, y)); y += p.size[1]
    crop = out.crop((0, 1000, W, y - (H - 3300)))
    crop = crop.resize((int(crop.size[0] * 0.8), int(crop.size[1] * 0.8)), Image.LANCZOS)
    crop.save(os.path.join(OUT, 'ui_shop_page.png'))
    print('ui_shop_page', crop.size)

def hue_shift(img, dh, sat=1.0):
    o = Image.new('RGBA', img.size); src = img.load(); dst = o.load()
    for yy in range(img.size[1]):
        for xx in range(img.size[0]):
            r, g, b, a = src[xx, yy]
            if a == 0:
                dst[xx, yy] = (0, 0, 0, 0); continue
            h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
            if v < 0.35 or s < 0.08:
                dst[xx, yy] = (r, g, b, a); continue
            rr, gg, bb = colorsys.hsv_to_rgb((h + dh) % 1.0, min(1, s * sat), v)
            dst[xx, yy] = (int(rr * 255), int(gg * 255), int(bb * 255), a)
    return o

def build_buttons():
    btn = Image.open(os.path.join(KIT, 'Settings_Button.png')).convert('RGBA')
    hue_shift(btn, -0.36).save(os.path.join(OUT, 'ui_btn_green_wide.png'))
    hue_shift(btn, -0.59, 1.05).save(os.path.join(OUT, 'ui_btn_red_wide.png'))
    print('buttons ok')

if __name__ == '__main__':
    build_page()
    build_buttons()
