using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DogtorBurguer
{
    public class SettingsPanel : MonoBehaviour
    {
        private GameObject _panel;
        private Canvas _canvas;
        private TextMeshProUGUI _soundLabel;
        private TextMeshProUGUI _controlLabel;

        public void Show()
        {
            if (_panel != null)
            {
                _panel.SetActive(true);
                UpdateSoundLabel();
                UpdateControlLabel();
                return;
            }

            CreatePanel();
        }

        public void Hide()
        {
            if (_panel != null)
                _panel.SetActive(false);
        }

        private void CreatePanel()
        {
            _canvas = FindAnyObjectByType<Canvas>();

            _panel = new GameObject("SettingsPanel");
            _panel.transform.SetParent(_canvas.transform, false);

            RectTransform panelRect = _panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            Image bgImg = _panel.AddComponent<Image>();
            bgImg.color = UIStyles.OVERLAY_DARK;

            // Inner panel
            GameObject inner = new GameObject("Inner");
            inner.transform.SetParent(_panel.transform, false);
            RectTransform innerRect = inner.AddComponent<RectTransform>();
            innerRect.anchorMin = new Vector2(0.5f, 0.5f);
            innerRect.anchorMax = new Vector2(0.5f, 0.5f);
            innerRect.sizeDelta = UIStyles.SETTINGS_PANEL_SIZE;
            Image innerImg = inner.AddComponent<Image>();
            innerImg.color = UIStyles.INNER_PANEL_BG;

            // Title
            CreateText(inner, "Settings", 0, 130, UIStyles.PANEL_TITLE_SIZE, FontStyles.Bold, UIStyles.TEXT_UI);

            // Sound toggle button
            CreateSettingsButton(inner, 0, 50, OnSoundToggleClicked, out _soundLabel);
            UpdateSoundLabel();

            // Control mode toggle button
            CreateSettingsButton(inner, 0, -20, OnControlToggleClicked, out _controlLabel);
            UpdateControlLabel();

            // Close button
            GameObject closeObj = new GameObject("CloseBtn");
            closeObj.transform.SetParent(inner.transform, false);
            RectTransform closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.5f, 0.5f);
            closeRect.anchorMax = new Vector2(0.5f, 0.5f);
            closeRect.anchoredPosition = new Vector2(0, -110);
            closeRect.sizeDelta = UIStyles.CLOSE_BUTTON_SIZE;

            Image closeImg = closeObj.AddComponent<Image>();
            closeImg.color = UIStyles.BTN_CLOSE;

            Button closeBtn = closeObj.AddComponent<Button>();
            closeBtn.targetGraphic = closeImg;
            closeBtn.onClick.AddListener(Hide);

            GameObject closeTextObj = new GameObject("Text");
            closeTextObj.transform.SetParent(closeObj.transform, false);
            RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI closeTmp = closeTextObj.AddComponent<TextMeshProUGUI>();
            closeTmp.text = "Close";
            closeTmp.fontSize = UIStyles.SETTINGS_BUTTON_TEXT_SIZE;
            closeTmp.fontStyle = FontStyles.Bold;
            closeTmp.color = UIStyles.TEXT_UI;
            closeTmp.alignment = TextAlignmentOptions.Center;
            closeTmp.outlineWidth = UIStyles.OUTLINE_WIDTH_UI;
            closeTmp.outlineColor = UIStyles.OUTLINE_COLOR;
        }

        private void CreateSettingsButton(GameObject parent, float x, float y,
            UnityEngine.Events.UnityAction onClick, out TextMeshProUGUI label)
        {
            GameObject btnObj = new GameObject("SettingsBtn");
            btnObj.transform.SetParent(parent.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = new Vector2(x, y);
            btnRect.sizeDelta = UIStyles.SETTINGS_BUTTON_SIZE;

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = UIStyles.BTN_SETTINGS_TOGGLE;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(onClick);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            label = textObj.AddComponent<TextMeshProUGUI>();
            label.fontSize = UIStyles.SETTINGS_BUTTON_TEXT_SIZE;
            label.fontStyle = FontStyles.Bold;
            label.color = UIStyles.TEXT_UI;
            label.alignment = TextAlignmentOptions.Center;
            label.outlineWidth = UIStyles.OUTLINE_WIDTH_UI;
            label.outlineColor = UIStyles.OUTLINE_COLOR;
        }

        private TextMeshProUGUI CreateText(GameObject parent, string text, float x, float y,
            float size, FontStyles style, Color color)
        {
            GameObject textObj = new GameObject(text);
            textObj.transform.SetParent(parent.transform, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(300, 50);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.outlineWidth = UIStyles.OUTLINE_WIDTH_UI;
            tmp.outlineColor = UIStyles.OUTLINE_COLOR;

            return tmp;
        }

        private void OnSoundToggleClicked()
        {
            if (SaveDataManager.Instance == null) return;

            bool newState = !SaveDataManager.Instance.SoundOn;
            SaveDataManager.Instance.SetSoundOn(newState);
            AudioListener.volume = newState ? 1f : 0f;
            MusicManager.Instance?.ApplySoundSetting();
            UpdateSoundLabel();
        }

        private void UpdateSoundLabel()
        {
            if (_soundLabel == null) return;
            bool soundOn = SaveDataManager.Instance != null ? SaveDataManager.Instance.SoundOn : true;
            _soundLabel.text = soundOn ? "Sound: ON" : "Sound: OFF";
        }

        private void OnControlToggleClicked()
        {
            if (SaveDataManager.Instance == null) return;

            ControlMode current = SaveDataManager.Instance.ControlMode;
            ControlMode next = current == ControlMode.Drag ? ControlMode.Tap : ControlMode.Drag;
            SaveDataManager.Instance.SetControlMode(next);
            UpdateControlLabel();
        }

        private void UpdateControlLabel()
        {
            if (_controlLabel == null) return;
            ControlMode mode = SaveDataManager.Instance != null
                ? SaveDataManager.Instance.ControlMode
                : ControlMode.Drag;
            _controlLabel.text = mode == ControlMode.Drag ? "Controls: Drag" : "Controls: Tap";
        }
    }
}
