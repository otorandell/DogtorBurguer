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

        public void Show()
        {
            if (_panel != null)
            {
                _panel.SetActive(true);
                UpdateSoundLabel();
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
            bgImg.color = new Color(0, 0, 0, 0.85f);

            // Inner panel
            GameObject inner = new GameObject("Inner");
            inner.transform.SetParent(_panel.transform, false);
            RectTransform innerRect = inner.AddComponent<RectTransform>();
            innerRect.anchorMin = new Vector2(0.5f, 0.5f);
            innerRect.anchorMax = new Vector2(0.5f, 0.5f);
            innerRect.sizeDelta = new Vector2(350, 280);
            Image innerImg = inner.AddComponent<Image>();
            innerImg.color = new Color(0.18f, 0.18f, 0.25f, 1f);

            // Title
            CreateText(inner, "Settings", 0, 100, 36, FontStyles.Bold, Color.white);

            // Sound toggle button
            bool soundOn = SaveDataManager.Instance != null ? SaveDataManager.Instance.SoundOn : true;
            CreateSettingsButton(inner, 0, 20, OnSoundToggleClicked, out _soundLabel);
            UpdateSoundLabel();

            // Close button
            GameObject closeObj = new GameObject("CloseBtn");
            closeObj.transform.SetParent(inner.transform, false);
            RectTransform closeRect = closeObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.5f, 0.5f);
            closeRect.anchorMax = new Vector2(0.5f, 0.5f);
            closeRect.anchoredPosition = new Vector2(0, -80);
            closeRect.sizeDelta = new Vector2(200, 50);

            Image closeImg = closeObj.AddComponent<Image>();
            closeImg.color = new Color(0.5f, 0.5f, 0.5f);

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
            closeTmp.fontSize = 22;
            closeTmp.fontStyle = FontStyles.Bold;
            closeTmp.color = Color.white;
            closeTmp.alignment = TextAlignmentOptions.Center;
        }

        private void CreateSettingsButton(GameObject parent, float x, float y,
            UnityEngine.Events.UnityAction onClick, out TextMeshProUGUI label)
        {
            GameObject btnObj = new GameObject("SoundBtn");
            btnObj.transform.SetParent(parent.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = new Vector2(x, y);
            btnRect.sizeDelta = new Vector2(280, 55);

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.3f, 0.5f, 0.7f);

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
            label.fontSize = 22;
            label.fontStyle = FontStyles.Bold;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
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

            return tmp;
        }

        private void OnSoundToggleClicked()
        {
            if (SaveDataManager.Instance == null) return;

            bool newState = !SaveDataManager.Instance.SoundOn;
            SaveDataManager.Instance.SetSoundOn(newState);
            UpdateSoundLabel();
        }

        private void UpdateSoundLabel()
        {
            if (_soundLabel == null) return;
            bool soundOn = SaveDataManager.Instance != null ? SaveDataManager.Instance.SoundOn : true;
            _soundLabel.text = soundOn ? "Sound: ON" : "Sound: OFF";
        }
    }
}
