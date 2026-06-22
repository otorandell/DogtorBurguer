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
    /// across GameHUD, GameOverPanel, MainMenuUI, SettingsPanel, and ShopPanel.
    /// </summary>
    public static class UIFactory
    {
        /// <summary>
        /// Creates a screen-space overlay canvas with standard scaler settings.
        /// </summary>
        public static Canvas CreateCanvas(Transform parent, string name, int sortingOrder)
        {
            GameObject canvasObj = new GameObject(name);
            canvasObj.transform.SetParent(parent, false);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
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
        /// Creates a centered TextMeshProUGUI element with standard outline styling.
        /// </summary>
        public static TextMeshProUGUI CreateText(
            Transform parent, string text, Vector2 position, Vector2 size,
            float fontSize, FontStyles style = FontStyles.Normal,
            Color? color = null, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            GameObject textObj = new GameObject(text);
            textObj.transform.SetParent(parent, false);
            SetCenteredRect(textObj, position, size);
            return AddStyledText(textObj, text, fontSize, style, color ?? UIStyles.TEXT_UI, alignment);
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
            GameObject obj, string text, float fontSize, FontStyles style, Color color, TextAlignmentOptions alignment)
        {
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.outlineWidth = UIStyles.OUTLINE_WIDTH_UI;
            tmp.outlineColor = UIStyles.OUTLINE_COLOR;
            return tmp;
        }
    }
}
