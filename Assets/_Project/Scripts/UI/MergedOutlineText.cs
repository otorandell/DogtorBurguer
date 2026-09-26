using TMPro;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Whole-WORD sticker outline (artist note 2026-09-26): TMP strokes every glyph on its own, so
    /// tightly tracked letters draw their outline across their neighbour's fill and the word looks
    /// piled up. This puts a fill-only twin of the text on top of the stroked one: the twin's fills
    /// cover every inner outline, leaving a single silhouette stroke + shadow around the whole word,
    /// so labels can be tracked tighter. The stroked text stays the "real" label callers talk to —
    /// its text, size (auto-size included), fill colour and visibility are mirrored every frame.
    /// </summary>
    public class MergedOutlineText : MonoBehaviour
    {
        private TextMeshProUGUI _source;
        private TextMeshProUGUI _fill;
        private Color _fillColor;

        /// <summary>Adds the fill-only twin over <paramref name="source"/> (already styled with its
        /// border). The source keeps drawing its stroke + shadow; its own fill goes border-coloured
        /// so nothing of it peeks between the twin's letters.</summary>
        public static void Apply(TextMeshProUGUI source, Color32 border, float borderWidth)
        {
            GameObject obj = new GameObject(source.name + "_Fill");
            obj.transform.SetParent(source.transform, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            TextMeshProUGUI fill = obj.AddComponent<TextMeshProUGUI>();
            fill.raycastTarget = false;
            fill.font = source.font;
            fill.fontStyle = source.fontStyle;
            fill.alignment = source.alignment;
            fill.textWrappingMode = source.textWrappingMode;
            fill.overflowMode = source.overflowMode;
            fill.characterSpacing = source.characterSpacing;
            fill.lineSpacing = source.lineSpacing;
            fill.margin = source.margin;
            fill.enableAutoSizing = false; // mirrors the source's settled size instead
            // Thinned by the stroke width: TMP's stroke straddles the glyph edge, eating that much of
            // the source's fill — so the twin's fill lands exactly on the source's visible fill.
            UIFactory.StyleFillAndBorder(fill, source.color, border, 0f, shadow: false,
                faceDilate: UIStyles.TEXT_FACE_DILATE - borderWidth);

            MergedOutlineText merged = source.gameObject.AddComponent<MergedOutlineText>();
            merged._source = source;
            merged._fill = fill;
            merged._fillColor = source.color;
            source.color = (Color)border;
            merged.Sync();
        }

        private void LateUpdate() => Sync();

        private void Sync()
        {
            if (_source == null || _fill == null) return;
            // A caller recolouring the label means the FILL colour: take it, re-hide the source fill.
            Color border = _source.fontSharedMaterial.GetColor(ShaderUtilities.ID_OutlineColor);
            if (_source.color != border)
            {
                _fillColor = _source.color;
                _source.color = border;
            }
            if (_fill.color != _fillColor) _fill.color = _fillColor;
            if (_fill.text != _source.text) _fill.text = _source.text;
            if (!Mathf.Approximately(_fill.fontSize, _source.fontSize)) _fill.fontSize = _source.fontSize;
            if (_fill.enabled != _source.enabled) _fill.enabled = _source.enabled;
        }
    }
}
