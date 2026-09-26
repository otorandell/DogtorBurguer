using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

namespace DogtorBurguer
{
    /// <summary>
    /// Shared factory for programmatic UI construction.
    /// Eliminates duplicated canvas, text, button, panel, and overlay creation
    /// across GameHUD, GameOverPanel, MainMenuUI, SettingsPanel, and the Shop screen.
    /// </summary>
    public static class UIFactory
    {
        /// <summary>
        /// Creates a screen-space canvas with standard scaler settings.
        /// Pass a worldCamera to use Screen Space - Camera mode, which lets
        /// world sprites with a higher sorting order render in front of the
        /// canvas (e.g. fairies flying over the HUD). Without it, the canvas
        /// is a Screen Space Overlay and always draws on top of the world.
        /// </summary>
        public static Canvas CreateCanvas(Transform parent, string name, int sortingOrder, Camera worldCamera = null)
        {
            GameObject canvasObj = new GameObject(name);
            canvasObj.transform.SetParent(parent, false);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            if (worldCamera != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = worldCamera;
                canvas.planeDistance = 100f;
            }
            else
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = UIStyles.REFERENCE_RESOLUTION;
            scaler.matchWidthOrHeight = UIStyles.MATCH_WIDTH_OR_HEIGHT;

            canvasObj.AddComponent<GraphicRaycaster>();

            // The notch-safe container (see SafeAreaRoot): edge-anchored interactive chrome
            // parents to UIFactory.SafeRoot(canvas); full-bleed art stays on the canvas.
            GameObject safeObj = new GameObject("SafeRoot");
            safeObj.transform.SetParent(canvasObj.transform, false);
            RectTransform safeRect = safeObj.AddComponent<RectTransform>();
            safeRect.anchorMin = Vector2.zero;
            safeRect.anchorMax = Vector2.one;
            safeRect.sizeDelta = Vector2.zero;
            safeObj.AddComponent<SafeAreaRoot>();

            return canvas;
        }

        /// <summary>The canvas's safe-area container (created by CreateCanvas): parent
        /// edge-anchored interactive chrome here so a camera notch can never swallow it.</summary>
        public static Transform SafeRoot(Canvas canvas) => canvas.transform.Find("SafeRoot");

        /// <summary>
        /// Ensures an EventSystem exists in the scene (required for button input).
        /// </summary>
        public static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject obj = new GameObject("EventSystem");
                obj.AddComponent<EventSystem>();
                obj.AddComponent<InputSystemUIInputModule>();
            }
        }

        /// <summary>
        /// Creates a centered TextMeshProUGUI element with standard outline styling. Single-line
        /// (no wrapping) by default — pass <paramref name="wrap"/> for multi-line paragraphs.
        /// </summary>
        public static TextMeshProUGUI CreateText(
            Transform parent, string text, Vector2 position, Vector2 size,
            float fontSize, FontStyles style = FontStyles.Normal,
            Color? color = null, TextAlignmentOptions alignment = TextAlignmentOptions.Center,
            bool wrap = false)
        {
            GameObject textObj = new GameObject(text);
            textObj.transform.SetParent(parent, false);
            SetCenteredRect(textObj, position, size);
            return AddStyledText(textObj, text, fontSize, style, color ?? UIStyles.TEXT_UI, alignment, wrap);
        }

        /// <summary>
        /// Creates a button with centered text label and standard styling.
        /// Returns the GameObject, Button component, and label TextMeshProUGUI.
        /// </summary>
        public static (GameObject obj, Button button, TextMeshProUGUI label) CreateButton(
            Transform parent, string label, Vector2 position, Vector2 size,
            Color color, float fontSize, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(label);
            btnObj.transform.SetParent(parent, false);
            SetCenteredRect(btnObj, position, size);

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = color;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(() =>
            {
                if (TouchInputHandler.PressTakenByFairy) return; // the press collected a fairy flying over this button
                AudioManager.Instance?.PlayUiTap();
                onClick();
            });

            // Label stretches to fill the button.
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            SetStretchRect(textObj);
            TextMeshProUGUI tmp = AddStyledText(textObj, label, fontSize, FontStyles.Bold, UIStyles.TEXT_UI, TextAlignmentOptions.Center);

            return (btnObj, btn, tmp);
        }

        /// <summary>
        /// Creates a full-screen overlay with the given color. With <paramref name="blur"/> (the
        /// modal backdrop, 2026-09-17) a frosted snapshot of the screen sits under the tint
        /// (<see cref="BlurBackdrop"/>); <paramref name="hideDuringCapture"/> is what must be kept out
        /// of that snapshot — the modal's canvas when its content is a sibling of the overlay, or
        /// null for the overlay itself when the content is its child.
        /// </summary>
        public static GameObject CreateOverlay(Transform parent, Color color, bool blur = false,
            GameObject hideDuringCapture = null)
        {
            GameObject overlay = new GameObject("Overlay");
            overlay.transform.SetParent(parent, false);
            SetStretchRect(overlay);

            if (blur && UIStyles.MODAL_BLUR_ENABLED)
            {
                GameObject frosted = new GameObject("Blur");
                frosted.transform.SetParent(overlay.transform, false);
                SetStretchRect(frosted);
                RawImage raw = frosted.AddComponent<RawImage>();
                raw.raycastTarget = false;
                frosted.AddComponent<BlurBackdrop>().HideRoot = hideDuringCapture != null ? hideDuringCapture : overlay;
            }

            // The tint is a child too (not the overlay's own Image) so it draws OVER the blur.
            GameObject tint = new GameObject("Tint");
            tint.transform.SetParent(overlay.transform, false);
            SetStretchRect(tint);
            Image img = tint.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;

            // The overlay itself blocks taps into whatever is behind it.
            Image blocker = overlay.AddComponent<Image>();
            blocker.color = Color.clear;

            return overlay;
        }

        /// <summary>
        /// Creates a centered panel with the given size and background color.
        /// </summary>
        public static GameObject CreatePanel(Transform parent, Vector2 size, Color color)
        {
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(parent, false);
            SetCenteredRect(panel, Vector2.zero, size);

            Image img = panel.AddComponent<Image>();
            img.color = color;

            return panel;
        }

        /// <summary>
        /// Creates a sprite Image anchored to a point on its parent. Non-interactive (no raycast).
        /// If <paramref name="size"/> is zero, the sprite's native size is used.
        /// </summary>
        public static Image CreateImage(Transform parent, string name, Sprite sprite,
            Vector2 anchor, Vector2 anchoredPos, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;

            Image img = obj.AddComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = false;
            if (size == Vector2.zero && sprite != null)
                img.SetNativeSize();
            else
                rect.sizeDelta = size;

            return img;
        }

        /// <summary>
        /// Creates a sprite-backed button anchored to a point on its parent (no text label).
        /// Used for icon buttons like the top-bar shop/settings buttons.
        /// </summary>
        public static Button CreateSpriteButton(Transform parent, string name, Sprite sprite,
            Vector2 anchor, Vector2 anchoredPos, Vector2 size, UnityEngine.Events.UnityAction onClick)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);

            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = size;

            Image img = obj.AddComponent<Image>();
            img.sprite = sprite;

            Button btn = obj.AddComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null)
                btn.onClick.AddListener(() =>
                {
                    // A press that collected a fairy flying over this button is the fairy's, not ours.
                    if (TouchInputHandler.PressTakenByFairy) return;
                    // Every factory-made button carries the UI tap (2026-09-07); a missing
                    // AudioManager (none in a scene) just means silence.
                    AudioManager.Instance?.PlayUiTap();
                    onClick();
                });

            return btn;
        }

        // --- shared construction helpers ---

        private static void SetCenteredRect(GameObject obj, Vector2 position, Vector2 size)
        {
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void SetStretchRect(GameObject obj)
        {
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }

        private static TextMeshProUGUI AddStyledText(
            GameObject obj, string text, float fontSize, FontStyles style, Color color, TextAlignmentOptions alignment,
            bool wrap = false)
        {
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = alignment;
            // TMP defaults to wrapping ON (from TMP Settings); on a label-sized rect that renders
            // one character per line. Explicit newlines still break lines under NoWrap, so only
            // genuinely auto-wrapping paragraphs need wrap = true.
            tmp.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            tmp.characterSpacing = UIStyles.TEXT_CHARACTER_SPACING;
            // Paragraphs need real leading; the global trim is for single-line labels only.
            tmp.lineSpacing = wrap ? UIStyles.TEXT_LINE_SPACING_WRAP : UIStyles.TEXT_LINE_SPACING;

            // Localization shrink-to-fit (2026-09-08): translated labels can outgrow rects tuned
            // on English (SPEZIALBESTELLUNG...), so every single-line label caps at its requested
            // size and shrinks only when it must. Paragraphs (wrap: true) flow instead.
            if (!wrap)
            {
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = fontSize * 0.55f;
                tmp.fontSizeMax = fontSize;
            }

            // The weight trim (TEXT_FACE_DILATE) must reach PLAIN texts too — a cached
            // dilate-only clone of the font material. Styled texts replace it a moment later via
            // StyleFillAndBorder, which bakes the same dilate into its own cached materials.
            // On an INACTIVE hierarchy TMP hasn't run Awake yet and tmp.font is null (the
            // ModalPanel gotcha — bit the How-to bullets 2026-09-08), so assign the default
            // font explicitly instead of dereferencing it.
            TMP_FontAsset font = tmp.font != null ? tmp.font : TMP_Settings.defaultFontAsset;
            if (font != null)
            {
                tmp.font = font;
                tmp.fontSharedMaterial = PlainMaterial(font.material);
                tmp.UpdateMeshPadding();
            }
            return tmp;
        }

        // One dilate-only material per font material, shared by every plain text (see above).
        private static readonly Dictionary<Material, Material> _plainMaterials = new();

        private static Material PlainMaterial(Material fontMat)
        {
            if (!_plainMaterials.TryGetValue(fontMat, out Material mat))
            {
                mat = new Material(fontMat);
                mat.SetFloat(ShaderUtilities.ID_FaceDilate, UIStyles.TEXT_FACE_DILATE);
                _plainMaterials[fontMat] = mat;
            }
            return mat;
        }

        /// <summary>THE rule for a word on an authored button blank (2026-09-17): centred on the
        /// art's FACE (<see cref="ButtonFace"/> — the bright surface, not the canvas with its outline
        /// and bottom lip), cap-height centred (<c>Capline</c>, so the height does not drift with the
        /// font's metrics, the auto-sized point size or an accent — Midline would sink CRÉDITOS), HUD sticker lettering, and shrink-to-fit
        /// between <paramref name="minFontSize"/> and <paramref name="fontSize"/> with a guaranteed
        /// side gap (<see cref="UIStyles.BUTTON_LABEL_SIDE_PAD_FRAC"/> of the face width) so a long
        /// translation shrinks before it touches the outline. The CEILING is the size whose cap
        /// height fills the face minus its vertical pad (<see cref="CapFitFontSize"/>) — short words
        /// grow into the face instead of sitting at a designer number; pass a positive
        /// <paramref name="maxFontSize"/> only where the mock wants a specific size (0 = fit). The
        /// rect is the face's width but twice the button height, so only WIDTH drives the shrink
        /// (TMP would otherwise clamp on Baloo's 1.57 em line box, which is far taller than its caps).
        /// Replaces the per-screen "*_LABEL_NUDGE" knobs; <paramref name="nudge"/> is for a genuine
        /// per-art correction only.</summary>
        public static TextMeshProUGUI CreateFaceLabel(Transform button, Sprite art, Vector2 buttonSize,
            string text, float maxFontSize, float minFontSize, Vector2 nudge = default)
        {
            Rect face = ButtonFace.Of(art, buttonSize);
            float max = CapFitFontSize(face.height);
            if (maxFontSize > 0f) max = Mathf.Min(max, maxFontSize);
            TextMeshProUGUI tmp = CreateText(button, text, face.center + nudge, new Vector2(face.width, buttonSize.y * 2f),
                max, FontStyles.Bold, alignment: TextAlignmentOptions.Capline);
            StyleHudText(tmp);
            // The label's rect is deliberately twice the button's height (so auto-size only ever
            // shrinks for width) — as a raycast target it made that whole invisible box a click on
            // the button, stealing taps from whatever sits above it (the How-to pager arrows over
            // PLAY TUTORIAL, 2026-09-26). The button's own image is the hit area.
            tmp.raycastTarget = false;
            tmp.characterSpacing = UIStyles.BUTTON_LABEL_CHARACTER_SPACING;
            AutoFit(tmp, Mathf.Min(minFontSize, max), max);
            float pad = face.width * UIStyles.BUTTON_LABEL_SIDE_PAD_FRAC;
            tmp.margin = new Vector4(pad, 0f, pad, 0f);
            // One outline around the whole word, so the tighter tracking doesn't pile letters up.
            MergedOutlineText.Apply(tmp, UIStyles.HUD_TEXT_STROKE, UIStyles.HUD_TEXT_BORDER_WIDTH);
            return tmp;
        }

        /// <summary>The font size whose CAP HEIGHT fills <paramref name="faceHeight"/> minus the
        /// vertical pad (<see cref="UIStyles.BUTTON_LABEL_VERTICAL_PAD_FRAC"/> each side) — the
        /// natural ceiling for an ALL-CAPS word on a face. Reads the default font's cap line.</summary>
        public static float CapFitFontSize(float faceHeight)
        {
            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            float capEm = font != null && font.faceInfo.pointSize > 0f
                ? font.faceInfo.capLine / font.faceInfo.pointSize
                : 0.7f;
            return faceHeight * (1f - 2f * UIStyles.BUTTON_LABEL_VERTICAL_PAD_FRAC) / capEm;
        }

        // --- sizing + fitting helpers ---

        /// <summary>Size for an authored sprite shown at <paramref name="width"/>, height following its native aspect.</summary>
        public static Vector2 SizeByWidth(Sprite sprite, float width)
        {
            float aspect = sprite != null ? sprite.rect.width / sprite.rect.height : 1f;
            return new Vector2(width, width / aspect);
        }

        /// <summary>Size for an authored sprite shown at <paramref name="height"/>, width following its native aspect.</summary>
        public static Vector2 SizeByHeight(Sprite sprite, float height)
        {
            float aspect = sprite != null ? sprite.rect.width / sprite.rect.height : 1f;
            return new Vector2(height * aspect, height);
        }

        /// <summary>Shrink-to-fit for a SINGLE-LINE label: the rect stays fixed and the text scales
        /// down (to <paramref name="min"/>) to stay inside it. ⚠️ This forces NoWrap — that is the
        /// point for a label, but it silently destroys a PARAGRAPH (it then grows sideways as one
        /// endless line instead of wrapping). Paragraphs want <see cref="AutoFitWrapped"/>.</summary>
        public static void AutoFit(TextMeshProUGUI tmp, float min, float max)
        {
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = min;
            tmp.fontSizeMax = max;
        }

        /// <summary>Shrink-to-fit for a WRAPPING paragraph: same fixed rect and size floor, but the
        /// text keeps flowing onto new lines. Use this wherever a translated paragraph has to stay
        /// inside fixed art (tutorial tips) rather than a one-line label.</summary>
        public static void AutoFitWrapped(TextMeshProUGUI tmp, float min, float max)
        {
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = min;
            tmp.fontSizeMax = max;
        }

        // --- fill + border styling ---

        // One material per (font material, border color, width) style, shared by every text using it —
        // keeps draws batched and avoids per-component material instances (mutating tmp.outlineWidth /
        // fontMaterial right after AddComponent is the path that never rendered reliably).
        private static readonly Dictionary<(Material font, Color32 border, float width, Color32 shadow, bool hasShadow, float dilate), Material> _outlineMaterials = new();

        /// <summary>
        /// Styles a text with the standard HUD palette: cream fill + dark-brown border
        /// (see UIStyles.HUD_TEXT_*). Used on the big card numbers and all red-box labels.
        /// </summary>
        public static void StyleHudText(TMP_Text tmp) =>
            StyleFillAndBorder(tmp, UIStyles.HUD_TEXT_FILL, UIStyles.HUD_TEXT_STROKE, UIStyles.HUD_TEXT_BORDER_WIDTH,
                UIStyles.HUD_TEXT_BORDER);

        /// <summary>
        /// Gives a text the game's "sticker" lettering (the artist's Photoshop layer-style recipe —
        /// see Look Reference/Font info.png): solid fill + rendered border + a hard drop shadow in
        /// the border color (the TMP SDF Underlay pass; offsets in UIStyles.TEXT_SHADOW_*). Assigns
        /// a cached shared material (keywords enabled, mesh padding updated) instead of the
        /// per-component setters, which don't reliably render on runtime-created TMP components.
        /// </summary>
        /// <param name="faceDilate">Overrides the glyph weight (default UIStyles.TEXT_FACE_DILATE) —
        /// the fill-only twin of a MergedOutlineText thins by the stroke width to match the stroked
        /// text's visible fill exactly.</param>
        public static void StyleFillAndBorder(TMP_Text tmp, Color fill, Color32 border, float width,
            Color32? shadowColor = null, bool shadow = true, float? faceDilate = null)
        {
            tmp.color = fill;

            Color32 underlay = shadowColor ?? border;
            Material fontMat = tmp.font.material;
            float dilate = faceDilate ?? UIStyles.TEXT_FACE_DILATE;
            var key = (fontMat, border, width, underlay, shadow, dilate);
            if (!_outlineMaterials.TryGetValue(key, out Material mat))
            {
                mat = new Material(fontMat);
                mat.SetFloat(ShaderUtilities.ID_FaceDilate, dilate);
                mat.EnableKeyword(ShaderUtilities.Keyword_Outline);
                mat.SetColor(ShaderUtilities.ID_OutlineColor, (Color)border);
                mat.SetFloat(ShaderUtilities.ID_OutlineWidth, width);
                if (shadow)
                {
                    // The drop shadow: darker than the stroke, dilated and pushed down, hard-edged —
                    // reads as the second (outer) outline of the baked lettering. Small labels pass
                    // shadow: false and keep the plain single outline (DesiredFontSingleOutline.png).
                    mat.EnableKeyword(ShaderUtilities.Keyword_Underlay);
                    mat.SetColor(ShaderUtilities.ID_UnderlayColor, (Color)underlay);
                    mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, UIStyles.TEXT_SHADOW_OFFSET_X);
                    mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, UIStyles.TEXT_SHADOW_OFFSET_Y);
                    mat.SetFloat(ShaderUtilities.ID_UnderlayDilate, UIStyles.TEXT_SHADOW_DILATE);
                    mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, UIStyles.TEXT_SHADOW_SOFTNESS);
                }
                _outlineMaterials[key] = mat;
            }

            tmp.fontSharedMaterial = mat;
            tmp.UpdateMeshPadding();
        }
    }
}
