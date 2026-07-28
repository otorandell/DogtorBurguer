using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// Composes the Shop page, top to bottom: remove-ads offer, skins (Dogtor, then ingredients),
    /// power-ups, star packs (gems → stars, confirm-gated), gem packs (IAP + free ad). Pure
    /// composition on top of ShopWidgets; every transaction routes through ShopService.
    /// </summary>
    public static class ShopSections
    {
        private static readonly ConsumableType[] ConsumableTypes =
            { ConsumableType.Ketchup, ConsumableType.Mustard, ConsumableType.Skewer };

        public static void BuildAll(RectTransform content, ShopScreen screen)
        {
            BuildRemoveAds(content, screen);
            BuildSkinRow(content, screen, "DOGTOR SKINS", ShopCatalog.ChefSkins());
            BuildIngredientSkins(content, screen);
            BuildConsumables(content, screen);
            BuildStarPacks(content, screen);
            BuildGemPacks(content, screen);
        }

        // The one "special offer": remove ads bundled with a gem sweetener. Banner position (top)
        // and the bundle framing follow standard shop merchandising; it vanishes once bought.
        private static void BuildRemoveAds(RectTransform content, ShopScreen screen)
        {
            if (SaveDataManager.Instance != null && SaveDataManager.Instance.AdsRemoved) return;

            RectTransform bar = ShopWidgets.CreateBar(content, UIStyles.SHOP_REMOVE_ADS_BAR_H,
                UIStyles.SHOP_CARD_BG_HIGHLIGHT);
            ShopWidgets.CreateBarTexts(bar, "ui_gem", "REMOVE ADS",
                $"No more forced ads, {MonetizationConfig.REMOVE_ADS_BONUS_GEMS} bonus Gems!\nReward ads stay available.", "");
            ShopWidgets.CreatePriceButton(bar, new Vector2(1f, 0.5f), UIStyles.SHOP_PRICE_BTN_POS,
                UIStyles.SHOP_PRICE_BTN_SIZE, UIStyles.BTN_SHOP_BUY, null,
                MonetizationConfig.REMOVE_ADS_PRICE_LABEL,
                () => { ShopService.BuyRemoveAds(); screen.NotifyChanged(); });

            screen.RegisterRefresh(() => bar.gameObject.SetActive(
                SaveDataManager.Instance == null || !SaveDataManager.Instance.AdsRemoved));
        }

        private static void BuildSkinRow(RectTransform content, ShopScreen screen, string title, List<Skin> skins)
        {
            if (skins.Count == 0) return;

            ShopWidgets.CreateSectionTitle(content, title);
            RectTransform row = ShopWidgets.CreateHorizontalRow(content, UIStyles.SHOP_SKIN_ROW_H);
            foreach (Skin skin in skins)
                ShopSkinCell.Create(row, skin, screen);
        }

        // Ingredient skins: one "INGREDIENT SKINS" header, then a labelled row per ingredient type
        // (Patty, Cheese, …, Buns) so each type's variants scroll independently.
        private static void BuildIngredientSkins(RectTransform content, ShopScreen screen)
        {
            List<(string Label, List<Skin> Skins)> rows = ShopCatalog.IngredientSkinRows();
            if (rows.Count == 0) return;

            ShopWidgets.CreateSectionTitle(content, "INGREDIENT SKINS");
            foreach ((string label, List<Skin> skins) in rows)
            {
                ShopWidgets.CreateSubTitle(content, label);
                RectTransform row = ShopWidgets.CreateHorizontalRow(content, UIStyles.SHOP_SKIN_ROW_H);
                foreach (Skin skin in skins)
                    ShopSkinCell.Create(row, skin, screen);
            }
        }

        private static void BuildConsumables(RectTransform content, ShopScreen screen)
        {
            ShopWidgets.CreateSectionTitle(content, "POWER-UPS");
            RectTransform row = ShopWidgets.CreateHorizontalRow(content, UIStyles.SHOP_CONSUMABLE_ROW_H);
            foreach (ConsumableType type in ConsumableTypes)
                BuildConsumableCard(row, type, screen);
        }

        // One card per consumable: icon, name, owned count, and the shared pack ladder as
        // stacked star-priced buy buttons. Star spends are instant (no confirm — soft currency).
        private static void BuildConsumableCard(RectTransform row, ConsumableType type, ShopScreen screen)
        {
            GameObject cardObj = new GameObject("Card_" + type);
            cardObj.transform.SetParent(row, false);
            RectTransform card = cardObj.AddComponent<RectTransform>();
            cardObj.AddComponent<Image>().color = UIStyles.SHOP_CARD_BG;
            LayoutElement layout = cardObj.AddComponent<LayoutElement>();
            layout.preferredWidth = UIStyles.SHOP_CONSUMABLE_CARD_SIZE.x;
            layout.preferredHeight = UIStyles.SHOP_CONSUMABLE_CARD_SIZE.y;

            Sprite icon = UiArt.Load("ui_consumable_" + type.ToString().ToLowerInvariant());
            float aspect = icon != null ? icon.rect.width / icon.rect.height : 1f;
            UIFactory.CreateImage(card, "Icon", icon, new Vector2(0.5f, 1f),
                new Vector2(0f, UIStyles.SHOP_CONSUMABLE_ICON_Y),
                new Vector2(UIStyles.SHOP_CONSUMABLE_ICON_H * aspect, UIStyles.SHOP_CONSUMABLE_ICON_H));

            TextMeshProUGUI name = UIFactory.CreateText(card, type.ToString(), Vector2.zero,
                new Vector2(UIStyles.SHOP_CONSUMABLE_CARD_SIZE.x, 24f), UIStyles.SHOP_SKIN_NAME_SIZE,
                FontStyles.Bold, UIStyles.TEXT_UI);
            AnchorTop(name.rectTransform, UIStyles.SHOP_CONSUMABLE_NAME_Y);

            TextMeshProUGUI owned = UIFactory.CreateText(card, "", Vector2.zero,
                new Vector2(UIStyles.SHOP_CONSUMABLE_CARD_SIZE.x, 20f), UIStyles.SHOP_OFFER_SUB_SIZE,
                FontStyles.Normal, UIStyles.SHOP_SUBTEXT_COLOR);
            AnchorTop(owned.rectTransform, UIStyles.SHOP_CONSUMABLE_OWNED_Y);

            void RefreshOwned() => owned.text = "Owned: " + (ConsumableInventory.Instance != null
                ? ConsumableInventory.Instance.CountOf(type)
                : SaveDataManager.Instance != null ? SaveDataManager.Instance.ConsumableCount(type) : 0);
            RefreshOwned();
            screen.RegisterRefresh(RefreshOwned);

            ConsumablePack[] packs = MonetizationConfig.CONSUMABLE_PACKS;
            for (int i = 0; i < packs.Length; i++)
            {
                ConsumablePack pack = packs[i];
                float y = UIStyles.SHOP_CONSUMABLE_BTN_Y - i * UIStyles.SHOP_CONSUMABLE_BTN_SPACING;
                Button button = null;
                button = ShopWidgets.CreatePriceButton(card, new Vector2(0.5f, 1f),
                    new Vector2(0f, y), UIStyles.SHOP_CONSUMABLE_BTN_SIZE, UIStyles.BTN_SHOP_BUY,
                    "ui_star", $"x{pack.Quantity} - {pack.StarCost}", () =>
                    {
                        if (ShopService.TryBuyConsumable(type, pack)) screen.NotifyChanged();
                        else ShopScreen.Deny(button.transform);
                    });
            }
        }

        // Stars are bought with gems (hard → soft, one-directional). Gem spends get a confirm
        // dialog — the one place the shop asks "are you sure".
        private static void BuildStarPacks(RectTransform content, ShopScreen screen)
        {
            ShopWidgets.CreateSectionTitle(content, "GET STARS");
            foreach (StarProduct product in MonetizationConfig.STAR_PRODUCTS)
            {
                RectTransform bar = ShopWidgets.CreateBar(content, UIStyles.SHOP_OFFER_BAR_H, UIStyles.SHOP_CARD_BG);
                ShopWidgets.CreateBarTexts(bar, "ui_star", $"{product.Amount} Stars", "", product.Badge);
                StarProduct captured = product;
                ShopWidgets.CreatePriceButton(bar, new Vector2(1f, 0.5f), UIStyles.SHOP_PRICE_BTN_POS,
                    UIStyles.SHOP_PRICE_BTN_SIZE, UIStyles.BTN_SHOP_BUY, "ui_gem",
                    product.GemCost.ToString(),
                    () => screen.ShowConfirm($"Buy {captured.Amount} Stars\nfor {captured.GemCost} Gems?", () =>
                    {
                        if (ShopService.TryBuyStarPack(captured)) screen.NotifyChanged();
                        else ShopScreen.Deny(screen.GemPill);
                    }));
            }
        }

        // Gem packs are real-money products (mock IAP for now) — the OS confirms those, we don't.
        // The free rewarded-ad grant leads the section so the "get gems" path always has a $0 rung.
        private static void BuildGemPacks(RectTransform content, ShopScreen screen)
        {
            ShopWidgets.CreateSectionTitle(content, "GET GEMS");

            RectTransform adBar = ShopWidgets.CreateBar(content, UIStyles.SHOP_OFFER_BAR_H, UIStyles.SHOP_CARD_BG);
            ShopWidgets.CreateBarTexts(adBar, "ui_gem", $"Free {MonetizationConfig.GEM_REWARD_AD} Gems",
                "Watch an ad", "");
            Button adButton = ShopWidgets.CreatePriceButton(adBar, new Vector2(1f, 0.5f), UIStyles.SHOP_PRICE_BTN_POS,
                UIStyles.SHOP_PRICE_BTN_SIZE, UIStyles.BTN_SHOP_AD, null, "FREE", () =>
                {
                    if (AdManager.Instance == null) return;
                    AdManager.Instance.ShowRewarded(success =>
                    {
                        if (success && SaveDataManager.Instance != null)
                            SaveDataManager.Instance.AddGems(MonetizationConfig.GEM_REWARD_AD);
                    });
                });

            // Track live rewarded availability — an ad may finish loading while the shop is open.
            TextMeshProUGUI adLabel = adButton.GetComponentInChildren<TextMeshProUGUI>();
            screen.RegisterPerFrame(() =>
            {
                bool available = AdManager.Instance != null && AdManager.Instance.IsRewardedAvailable;
                if (adButton.interactable == available) return;
                adButton.interactable = available;
                adLabel.text = available ? "FREE" : "LOADING...";
            });

            foreach (GemProduct product in MonetizationConfig.GEM_PRODUCTS)
            {
                RectTransform bar = ShopWidgets.CreateBar(content, UIStyles.SHOP_OFFER_BAR_H, UIStyles.SHOP_CARD_BG);
                ShopWidgets.CreateBarTexts(bar, "ui_gem", $"{product.Amount} Gems", "", product.Badge);
                GemProduct captured = product;
                ShopWidgets.CreatePriceButton(bar, new Vector2(1f, 0.5f), UIStyles.SHOP_PRICE_BTN_POS,
                    UIStyles.SHOP_PRICE_BTN_SIZE, UIStyles.BTN_SHOP_BUY, null, product.PriceLabel,
                    () => { ShopService.BuyGemPack(captured); screen.NotifyChanged(); });
            }
        }

        private static void AnchorTop(RectTransform rect, float y)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, y);
        }
    }
}
