using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DogtorBurguer
{
    public class ShopPanel : MonoBehaviour
    {
        private GameObject _panel;
        private Canvas _canvas;

        public void Show()
        {
            if (_panel != null)
            {
                _panel.SetActive(true);
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
            // Find existing canvas or create one
            _canvas = FindAnyObjectByType<Canvas>();

            // Overlay container
            _panel = new GameObject("ShopPanel");
            _panel.transform.SetParent(_canvas.transform, false);

            RectTransform panelRect = _panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.sizeDelta = Vector2.zero;

            // Dark background
            Image bgImg = _panel.AddComponent<Image>();
            bgImg.color = UIStyles.OVERLAY_DARK;

            // Inner panel
            GameObject inner = new GameObject("Inner");
            inner.transform.SetParent(_panel.transform, false);
            RectTransform innerRect = inner.AddComponent<RectTransform>();
            innerRect.anchorMin = new Vector2(0.5f, 0.5f);
            innerRect.anchorMax = new Vector2(0.5f, 0.5f);
            innerRect.sizeDelta = UIStyles.SHOP_PANEL_SIZE;
            Image innerImg = inner.AddComponent<Image>();
            innerImg.color = UIStyles.INNER_PANEL_BG;

            // Title
            CreateText(inner, "Shop", 0, 160, UIStyles.PANEL_TITLE_SIZE, FontStyles.Bold, UIStyles.GOLD);

            // Gem balance
            int gems = SaveDataManager.Instance != null ? SaveDataManager.Instance.Gems : 0;
            CreateText(inner, $"Your gems: {gems}", 0, 110, UIStyles.SETTINGS_BUTTON_TEXT_SIZE, FontStyles.Normal, UIStyles.TEXT_UI);

            // Watch Ad button
            CreateShopButton(inner, "Watch Ad (+25 gems)", 0, 40,
                UIStyles.BTN_SHOP_AD, OnWatchAdClicked);

            // Buy 100 gems
            CreateShopButton(inner, "Buy 100 gems - $0.99", 0, -40,
                UIStyles.BTN_SHOP_BUY, OnBuy100Clicked);

            // Buy 500 gems
            CreateShopButton(inner, "Buy 500 gems - $3.99", 0, -120,
                UIStyles.BTN_SHOP_BUY, OnBuy500Clicked);

            // Close button
            CreateShopButton(inner, "Close", 0, -190,
                UIStyles.BTN_CLOSE, Hide);
        }

        private void CreateShopButton(GameObject parent, string label, float x, float y,
            Color color, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject(label);
            btnObj.transform.SetParent(parent.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = new Vector2(x, y);
            btnRect.sizeDelta = UIStyles.SHOP_BUTTON_SIZE;

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
            tmp.fontSize = UIStyles.PANEL_BUTTON_TEXT_SIZE;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = UIStyles.TEXT_UI;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.outlineWidth = UIStyles.OUTLINE_WIDTH_UI;
            tmp.outlineColor = UIStyles.OUTLINE_COLOR;
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
            rect.sizeDelta = new Vector2(350, 50);

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

        private void OnWatchAdClicked()
        {
            if (AdManager.Instance == null) return;

            AdManager.Instance.ShowRewarded((success) =>
            {
                if (success && SaveDataManager.Instance != null)
                {
                    SaveDataManager.Instance.AddGems(Constants.GEM_REWARD_AD);
                    Debug.Log($"[Shop] Rewarded +{Constants.GEM_REWARD_AD} gems");
                }
            });
        }

        private void OnBuy100Clicked()
        {
            // Placeholder for IAP
            Debug.Log("[Shop] IAP not implemented - would buy 100 gems for $0.99");
            // For testing, grant the gems anyway
            SaveDataManager.Instance?.AddGems(100);
        }

        private void OnBuy500Clicked()
        {
            // Placeholder for IAP
            Debug.Log("[Shop] IAP not implemented - would buy 500 gems for $3.99");
            // For testing, grant the gems anyway
            SaveDataManager.Instance?.AddGems(500);
        }
    }
}
