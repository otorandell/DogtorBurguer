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
            canvasObj.transform.SetParent(parent);

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

            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color ?? UIStyles.TEXT_UI;
            tmp.alignment = alignment;
            tmp.outlineWidth = UIStyles.OUTLINE_WIDTH_UI;
            tmp.outlineColor = UIStyles.OUTLINE_COLOR;

            return tmp;
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

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = position;
            btnRect.sizeDelta = size;

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = color;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(onClick);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = UIStyles.TEXT_UI;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.outlineWidth = UIStyles.OUTLINE_WIDTH_UI;
            tmp.outlineColor = UIStyles.OUTLINE_COLOR;

            return (btnObj, btn, tmp);
        }

        /// <summary>
        /// Creates a full-screen overlay with the given color.
        /// </summary>
        public static GameObject CreateOverlay(Transform parent, Color color)
        {
            GameObject overlay = new GameObject("Overlay");
            overlay.transform.SetParent(parent, false);

            RectTransform rect = overlay.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

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

            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;

            Image img = panel.AddComponent<Image>();
            img.color = color;

            return panel;
        }
    }
}
