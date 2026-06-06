using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DogtorBurguer
{
    public class ShopPanel : MonoBehaviour
    {
        private GameObject _panel;

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
            Canvas canvas = FindAnyObjectByType<Canvas>();

            // Overlay container
            _panel = UIFactory.CreateOverlay(canvas.transform, UIStyles.OVERLAY_DARK);

            // Inner panel
            GameObject inner = UIFactory.CreatePanel(_panel.transform, UIStyles.SHOP_PANEL_SIZE, UIStyles.INNER_PANEL_BG);

            // Title
            UIFactory.CreateText(inner.transform, "Shop", UIStyles.SHOP_TITLE_POS, UIStyles.SHOP_TEXT_RECT,
                UIStyles.PANEL_TITLE_SIZE, FontStyles.Bold, UIStyles.GOLD);

            // Gem balance
            int gems = SaveDataManager.Instance != null ? SaveDataManager.Instance.Gems : 0;
            UIFactory.CreateText(inner.transform, $"Your gems: {gems}", UIStyles.SHOP_BALANCE_POS, UIStyles.SHOP_TEXT_RECT,
                UIStyles.SETTINGS_BUTTON_TEXT_SIZE);

            // Watch Ad button
            UIFactory.CreateButton(inner.transform, "Watch Ad (+25 gems)", new Vector2(0, UIStyles.SHOP_BTN_START_Y),
                UIStyles.SHOP_BUTTON_SIZE, UIStyles.BTN_SHOP_AD, UIStyles.PANEL_BUTTON_TEXT_SIZE, OnWatchAdClicked);

            // Buy 100 gems
            UIFactory.CreateButton(inner.transform, "Buy 100 gems - $0.99", new Vector2(0, UIStyles.SHOP_BTN_START_Y + UIStyles.SHOP_BTN_SPACING),
                UIStyles.SHOP_BUTTON_SIZE, UIStyles.BTN_SHOP_BUY, UIStyles.PANEL_BUTTON_TEXT_SIZE, OnBuy100Clicked);

            // Buy 500 gems
            UIFactory.CreateButton(inner.transform, "Buy 500 gems - $3.99", new Vector2(0, UIStyles.SHOP_BTN_START_Y + UIStyles.SHOP_BTN_SPACING * 2),
                UIStyles.SHOP_BUTTON_SIZE, UIStyles.BTN_SHOP_BUY, UIStyles.PANEL_BUTTON_TEXT_SIZE, OnBuy500Clicked);

            // Close button
            UIFactory.CreateButton(inner.transform, "Close", UIStyles.SHOP_CLOSE_POS,
                UIStyles.SHOP_BUTTON_SIZE, UIStyles.BTN_CLOSE, UIStyles.PANEL_BUTTON_TEXT_SIZE, Hide);
        }

        private void OnWatchAdClicked()
        {
            if (AdManager.Instance == null) return;

            AdManager.Instance.ShowRewarded((success) =>
            {
                if (success && SaveDataManager.Instance != null)
                {
                    SaveDataManager.Instance.AddGems(MonetizationConfig.GEM_REWARD_AD);
                    Debug.Log($"[Shop] Rewarded +{MonetizationConfig.GEM_REWARD_AD} gems");
                }
            });
        }

        private void OnBuy100Clicked()
        {
            Debug.Log("[Shop] IAP not implemented - would buy 100 gems for $0.99");
            SaveDataManager.Instance?.AddGems(100);
        }

        private void OnBuy500Clicked()
        {
            Debug.Log("[Shop] IAP not implemented - would buy 500 gems for $3.99");
            SaveDataManager.Instance?.AddGems(500);
        }
    }
}
