"""Re-encode every project PNG carrying a non-sRGB ICC profile (the artist's Mac/iPad exports
embed Display P3) into sRGB, so Unity — which ignores ICC profiles and reads raw values as
sRGB — shows the colours the artist actually painted (2026-09-17: the whole game read duller
than the delivered sprites).

Pixels only: no resampling, alpha untouched, profile stripped, same file names — .meta files,
sprite rects and IDs stay put, Unity just reimports. Relative colorimetric via LittleCMS;
colours outside the sRGB gamut clip to the nearest representable one.

    python scratchpad/convert_p3_to_srgb.py            # convert in place
    python scratchpad/convert_p3_to_srgb.py --check    # list offenders only (run on new art drops)
    python scratchpad/convert_p3_to_srgb.py --backup DIR   # copy originals to DIR first
"""
import io
import shutil
import sys
from pathlib import Path

from PIL import Image, ImageCms

ROOT = Path(__file__).resolve().parents[1] / "Assets" / "_Project"
SRGB = ImageCms.createProfile("sRGB")
SRGB_NAMES = ("srgb",)


def profile_name(im):
    icc = im.info.get("icc_profile")
    if not icc:
        return None
    try:
        return ImageCms.getProfileDescription(ImageCms.ImageCmsProfile(io.BytesIO(icc))).strip()
    except Exception:
        return "unreadable profile"


def needs_conversion(name):
    return name is not None and not any(s in name.lower() for s in SRGB_NAMES)


def convert(path):
    im = Image.open(path)
    src = ImageCms.ImageCmsProfile(io.BytesIO(im.info["icc_profile"]))
    mode = im.mode
    if mode not in ("RGB", "RGBA"):
        im = im.convert("RGBA")
    if im.mode == "RGBA":
        rgb, alpha = im.convert("RGB"), im.getchannel("A")
    else:
        rgb, alpha = im, None
    out = ImageCms.profileToProfile(rgb, src, SRGB, renderingIntent=ImageCms.Intent.RELATIVE_COLORIMETRIC)
    if alpha is not None:
        out.putalpha(alpha)
    out.save(path, format="PNG", optimize=True)  # no icc_profile kwarg -> profile dropped


def main(argv):
    check = "--check" in argv
    backup = Path(argv[argv.index("--backup") + 1]) if "--backup" in argv else None
    converted = skipped = 0
    for path in sorted(ROOT.rglob("*.png")):
        name = profile_name(Image.open(path))
        if not needs_conversion(name):
            skipped += 1
            continue
        rel = path.relative_to(ROOT.parent.parent)
        print(f"{name:>24}  {rel}")
        if check:
            continue
        if backup:
            dest = backup / rel
            dest.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(path, dest)
        convert(path)
        converted += 1
    print(f"\n{'would convert' if check else 'converted'} {converted if not check else '?'}, already sRGB/untagged {skipped}")


if __name__ == "__main__":
    main(sys.argv[1:])
