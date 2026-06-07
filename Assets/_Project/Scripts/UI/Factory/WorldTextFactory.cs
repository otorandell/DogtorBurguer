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
        /// Pass <paramref name="outlineWidth"/> &gt; 0 to enable a black outline.
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

            if (outlineWidth > 0f)
            {
                tmp.outlineWidth = outlineWidth;
                tmp.outlineColor = UIStyles.OUTLINE_COLOR;
            }

            tmp.rectTransform.sizeDelta = size;
            return tmp;
        }
    }
}
