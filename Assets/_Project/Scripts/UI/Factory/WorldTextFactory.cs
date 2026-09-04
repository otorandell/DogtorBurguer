using TMPro;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Constructs world-space TextMeshPro labels (the 3D component) — the counterpart to
    /// UIFactory, which only covers screen-space TextMeshProUGUI. Centralizes the repeated
    /// AddComponent → font / color / alignment / wrapping / sorting / outline / sizeDelta
    /// boilerplate used by the world-space popups.
    ///
    /// Sibling of UIFactory (screen-space UGUI) and SpriteFactory (procedural sprites).
    /// </summary>
    public static class WorldTextFactory
    {
        /// <summary>
        /// Adds a configured TextMeshPro to <paramref name="target"/> and returns it. The
        /// caller owns the GameObject (its own or a child), world positioning, and animation.
        /// Pass <paramref name="outlineWidth"/> &gt; 0 for the sticker lettering (shared
        /// fill + stroke + shadow material, same treatment as the UI text).
        /// </summary>
        public static TextMeshPro Create(
            GameObject target,
            string text,
            float fontSize,
            Color color,
            int sortingOrder,
            Vector2 size,
            FontStyles fontStyle = FontStyles.Normal,
            float outlineWidth = 0f,
            TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            TextMeshPro tmp = target.AddComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.fontStyle = fontStyle;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.sortingOrder = sortingOrder;

            // The shared sticker material — the old per-component outline setters never rendered
            // reliably on runtime-created TMPs (the long-standing world-outline known issue).
            if (outlineWidth > 0f)
                UIFactory.StyleFillAndBorder(tmp, color, UIStyles.HUD_TEXT_STROKE,
                    UIStyles.HUD_TEXT_BORDER_WIDTH, UIStyles.HUD_TEXT_BORDER);

            tmp.rectTransform.sizeDelta = size;
            return tmp;
        }

        /// <summary>
        /// Attaches a halftone glow plate (the 2026-09-04 popup blobs, Resources/UI) behind a
        /// world popup: a child SpriteRenderer one sorting step under the text, height-normalized.
        /// Returns it so the popup can join its fade into the exit tween.
        /// </summary>
        public static SpriteRenderer AttachPlate(GameObject owner, string art, float worldHeight,
            int textSortingOrder, Vector2 localOffset)
        {
            GameObject plateObj = new GameObject("Plate");
            plateObj.transform.SetParent(owner.transform, false);
            plateObj.transform.localPosition = localOffset;
            SpriteRenderer plate = plateObj.AddComponent<SpriteRenderer>();
            plate.sprite = UiArt.Load(art);
            plate.sortingOrder = textSortingOrder - 1;
            SpriteFit.Height(plate, worldHeight);
            return plate;
        }
    }
}
