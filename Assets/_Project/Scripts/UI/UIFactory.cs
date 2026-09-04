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

            return canvas;
        }

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
            btn.onClick.AddListener(onClick);

            // Label stretches to fill the button.
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            SetStretchRect(textObj);
            TextMeshProUGUI tmp = AddStyledText(textObj, label, fontSize, FontStyles.Bold, UIStyles.TEXT_UI, TextAlignmentOptions.Center);

            return (btnObj, btn, tmp);
        }

        /// <summary>
        /// Creates a full-screen overlay with the given color.
        /// </summary>
        public static GameObject CreateOverlay(Transform parent, Color color)
        {
            GameObject overlay = new GameObject("Overlay");
            overlay.transform.SetParent(parent, false);
            SetStretchRect(overlay);

            Image img = overlay.AddComponent<Image>();
            img.color = color;

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
                btn.onClick.AddListener(onClick);

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
            return tmp;
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

        /// <summary>Shrink-to-fit: the rect stays fixed and the text scales down (to <paramref name="min"/>) to stay inside it.</summary>
        public static void AutoFit(TextMeshProUGUI tmp, float min, float max)
        {
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = min;
            tmp.fontSizeMax = max;
        }

        // --- fill + border styling ---

        // One material per (font material, border color, width) style, shared by every text using it —
        // keeps draws batched and avoids per-component material instances (mutating tmp.outlineWidth /
        // fontMaterial right after AddComponent is the path that never rendered reliably).
        private static readonly Dictionary<(Material font, Color32 border, float width, Color32 shadow, bool hasShadow), Material> _outlineMaterials = new();

        /// <summary>
        /// Styles a text with the standard HUD palette: cream fill + dark-brown border
        /// (see UIStyles.HUD_TEXT_*). Used on the big card numbers and all red-box labels.
        /// </summary>
        public static void StyleHudText(TextMeshProUGUI tmp) =>
            StyleFillAndBorder(tmp, UIStyles.HUD_TEXT_FILL, UIStyles.HUD_TEXT_STROKE, UIStyles.HUD_TEXT_BORDER_WIDTH,
                UIStyles.HUD_TEXT_BORDER);

        /// <summary>
        /// Gives a text the game's "sticker" lettering (the artist's Photoshop layer-style recipe —
        /// see Look Reference/Font info.png): solid fill + rendered border + a hard drop shadow in
        /// the border color (the TMP SDF Underlay pass; offsets in UIStyles.TEXT_SHADOW_*). Assigns
        /// a cached shared material (keywords enabled, mesh padding updated) instead of the
        /// per-component setters, which don't reliably render on runtime-created TMP components.
        /// </summary>
        public static void StyleFillAndBorder(TextMeshProUGUI tmp, Color fill, Color32 border, float width,
            Color32? shadowColor = null, bool shadow = true)
        {
            tmp.color = fill;

            Color32 underlay = shadowColor ?? border;
            Material fontMat = tmp.font.material;
            var key = (fontMat, border, width, underlay, shadow);
            if (!_outlineMaterials.TryGetValue(key, out Material mat))
            {
                mat = new Material(fontMat);
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
