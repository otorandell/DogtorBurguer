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

            // Watch Ad button (reward amount derived from config)
            UIFactory.CreateButton(inner.transform, $"Watch Ad (+{MonetizationConfig.GEM_REWARD_AD} gems)",
                new Vector2(0, UIStyles.SHOP_BTN_START_Y),
                UIStyles.SHOP_BUTTON_SIZE, UIStyles.BTN_SHOP_AD, UIStyles.PANEL_BUTTON_TEXT_SIZE, OnWatchAdClicked);

            // Buy buttons — label and grant both derive from the product table (ad sits at index 0)
            GemProduct[] products = MonetizationConfig.GEM_PRODUCTS;
            for (int i = 0; i < products.Length; i++)
            {
                GemProduct product = products[i];
                float y = UIStyles.SHOP_BTN_START_Y + UIStyles.SHOP_BTN_SPACING * (i + 1);
                UIFactory.CreateButton(inner.transform, $"Buy {product.Amount} gems - {product.PriceLabel}",
                    new Vector2(0, y), UIStyles.SHOP_BUTTON_SIZE, UIStyles.BTN_SHOP_BUY,
                    UIStyles.PANEL_BUTTON_TEXT_SIZE, () => OnBuyClicked(product));
            }

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

        private void OnBuyClicked(GemProduct product)
        {
            // IAP stub — grants the gems directly until real IAP lands (see pre-launch checklist).
            Debug.Log($"[Shop] IAP not implemented - would buy {product.Amount} gems for {product.PriceLabel}");
            SaveDataManager.Instance?.AddGems(product.Amount);
        }
    }
}
