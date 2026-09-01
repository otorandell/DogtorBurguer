using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// Composes the Shop page to the mock, top to bottom: the support banner (remove-ads offer,
    /// or THANK YOU once bought), skins (Dogtor, then ingredients), the power-up grid + Pro Cook
    /// Pack, star packs (gems → stars, confirm-gated), gem packs (free ad rung + IAP). Pure
    /// composition on top of ShopWidgets; every transaction routes through ShopService.
    /// </summary>
    public static class ShopSections
    {
        private static readonly Vector2 Center = new(0.5f, 0.5f);
        private static readonly ConsumableType[] ConsumableTypes =
            { ConsumableType.Ketchup, ConsumableType.Mustard, ConsumableType.Skewer };

        public static void BuildAll(RectTransform content, ShopScreen screen)
        {
            BuildSupportBanner(content, screen);
            BuildSkinRow(content, screen, "DOGTOR SKINS", ShopCatalog.ChefSkins());
            BuildIngredientSkins(content, screen);
            BuildPowerUps(content, screen);
            BuildStarPacks(content, screen);
            BuildGemPacks(content, screen);
        }

        // Top of the page: the one "special offer" (remove ads + a gem sweetener) while unbought;
        // afterwards the mock's THANK YOU FOR SUPPORTING US! banner takes its slot.
        private static void BuildSupportBanner(RectTransform content, ShopScreen screen)
        {
            RectTransform offer = CreateBanner(content, "RemoveAds");
            TextMeshProUGUI offerText = UIFactory.CreateText(offer,
                $"REMOVE ADS\n{MonetizationConfig.REMOVE_ADS_BONUS_GEMS} bonus Gems!", Vector2.zero,
                new Vector2(UIStyles.SHOP_BANNER_OFFER_TEXT_W, UIStyles.SHOP_BANNER_H - UIStyles.SHOP_BANNER_TEXT_INSET.y),
                UIStyles.SHOP_BANNER_TEXT_SIZE, FontStyles.Bold);
            ShopWidgets.StyleAccent(offerText);
            UIFactory.AutoFit(offerText, UIStyles.SHOP_BANNER_TEXT_MIN, UIStyles.SHOP_BANNER_TEXT_SIZE);
            ShopWidgets.AnchorLeft(offerText.rectTransform, new Vector2(UIStyles.SHOP_BANNER_TEXT_INSET.x, 0f));
            Button offerPill = ShopWidgets.CreatePill(offer, "Pill", "ui_btn_green_wide", new Vector2(1f, 0.5f),
                new Vector2(UIStyles.SHOP_BANNER_PILL_X, 0f), UIStyles.SHOP_BANNER_PILL_W,
                () => { ShopService.BuyRemoveAds(); screen.NotifyChanged(); });
            ShopWidgets.SetPillLabel(offerPill, ShopWidgets.MoneyLabel(MonetizationConfig.REMOVE_ADS_PRICE_LABEL), null);

            RectTransform thanks = CreateBanner(content, "ThankYou");
            TextMeshProUGUI thanksText = UIFactory.CreateText(thanks, "THANK YOU FOR\nSUPPORTING US!", Vector2.zero,
                thanks.sizeDelta - UIStyles.SHOP_BANNER_TEXT_INSET, UIStyles.SHOP_BANNER_TEXT_SIZE, FontStyles.Bold);
            ShopWidgets.StyleAccent(thanksText);

            void Refresh()
            {
                bool removed = SaveDataManager.Instance != null && SaveDataManager.Instance.AdsRemoved;
                offer.gameObject.SetActive(!removed);
                thanks.gameObject.SetActive(removed);
            }
            Refresh();
            screen.RegisterRefresh(Refresh);
        }

        // A full-width cream box row on the page.
        private static RectTransform CreateBanner(RectTransform content, string name)
        {
            Image box = ShopWidgets.CreateBox(content, name, Center, Vector2.zero, Vector2.zero);
            box.gameObject.AddComponent<LayoutElement>().preferredHeight = UIStyles.SHOP_BANNER_H;
            return box.rectTransform;
        }

        private static void BuildSkinRow(RectTransform content, ShopScreen screen, string title, List<Skin> skins)
        {
            if (skins.Count == 0) return;

            ShopWidgets.CreateSectionTitle(content, title);
            RectTransform row = ShopWidgets.CreateHorizontalRow(content, ShopWidgets.CellHeight(withLabel: true));
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
                RectTransform row = ShopWidgets.CreateHorizontalRow(content, ShopWidgets.CellHeight(withLabel: true));
                foreach (Skin skin in skins)
                    ShopSkinCell.Create(row, skin, screen);
            }
        }

        // The power-up grid: one row per pack rung (x1, x3, …), one column per consumable — each
        // cell shows the owned count badge, the icon and the quantity; then the Pro Cook Pack row.
        // Star spends are instant (no confirm — soft currency).
        private static void BuildPowerUps(RectTransform content, ShopScreen screen)
        {
            ShopWidgets.CreateSectionTitle(content, "POWER-UPS");

            RectTransform grid = ShopWidgets.CreateGrid(content, ShopWidgets.CellHeight(withLabel: false));
            foreach (ConsumablePack pack in MonetizationConfig.CONSUMABLE_PACKS)
                foreach (ConsumableType type in ConsumableTypes)
                    BuildPowerUpCell(grid, type, pack, screen);

            BuildProCookPack(content, screen);
        }

        private static void BuildPowerUpCell(RectTransform grid, ConsumableType type, ConsumablePack pack, ShopScreen screen)
        {
            ShopCell cell = null;
            cell = ShopWidgets.CreateCell(grid, $"{type}_x{pack.Quantity}", null, () =>
            {
                if (ShopService.TryBuyConsumable(type, pack)) screen.NotifyChanged();
                else ShopScreen.Deny(cell.Root);
            });

            Sprite icon = ConsumableIcon(type);
            UIFactory.CreateImage(cell.Box, "Icon", icon, Center, Vector2.zero, UIFactory.SizeByHeight(icon, UIStyles.SHOP_ITEM_ICON_H));

            TextMeshProUGUI qty = UIFactory.CreateText(cell.Box, $"x{pack.Quantity}", UIStyles.SHOP_QTY_POS,
                UIStyles.SHOP_QTY_RECT, UIStyles.SHOP_QTY_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(qty);

            // Owned-count badge (the red num box from the consumable kit) on the box's corner.
            Sprite badgeArt = UiArt.Load("ui_consumable_num");
            Image badge = UIFactory.CreateImage(cell.Box, "Owned", badgeArt, Center, UIStyles.SHOP_COUNT_BADGE_POS,
                UIFactory.SizeByHeight(badgeArt, UIStyles.SHOP_COUNT_BADGE_H));
            TextMeshProUGUI owned = UIFactory.CreateText(badge.transform, "0", Vector2.zero, badge.rectTransform.sizeDelta,
                UIStyles.SHOP_COUNT_BADGE_TEXT, FontStyles.Bold);
            UIFactory.StyleHudText(owned);

            void RefreshOwned() => owned.text = OwnedCount(type).ToString();
            RefreshOwned();
            screen.RegisterRefresh(RefreshOwned);

            cell.SetPill(pack.StarCost.ToString(), "ui_star");
        }

        // The bundle row: the three icons + quantity, the pack name, and one big star pill.
        private static void BuildProCookPack(RectTransform content, ShopScreen screen)
        {
            ConsumablePack pack = MonetizationConfig.PRO_COOK_PACK;
            Image box = ShopWidgets.CreateBox(content, "ProCookPack", Center, Vector2.zero, Vector2.zero);
            box.gameObject.AddComponent<LayoutElement>().preferredHeight = UIStyles.SHOP_BUNDLE_H;
            RectTransform row = box.rectTransform;

            for (int i = 0; i < ConsumableTypes.Length; i++)
            {
                Sprite icon = ConsumableIcon(ConsumableTypes[i]);
                UIFactory.CreateImage(row, "Icon" + i, icon, new Vector2(0f, 0.5f),
                    new Vector2(UIStyles.SHOP_BUNDLE_ICON_X0 + i * UIStyles.SHOP_BUNDLE_ICON_SPACING, UIStyles.SHOP_BUNDLE_ICON_Y),
                    UIFactory.SizeByHeight(icon, UIStyles.SHOP_BUNDLE_ICON_H));
            }
            TextMeshProUGUI qty = UIFactory.CreateText(row, $"x{pack.Quantity}", Vector2.zero,
                UIStyles.SHOP_QTY_RECT, UIStyles.SHOP_QTY_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(qty);
            ShopWidgets.AnchorLeft(qty.rectTransform, UIStyles.SHOP_BUNDLE_QTY_POS);

            TextMeshProUGUI name = UIFactory.CreateText(row, "PRO COOK PACK", Vector2.zero,
                UIStyles.SHOP_BUNDLE_NAME_RECT, UIStyles.SHOP_BUNDLE_NAME_SIZE, FontStyles.Bold);
            ShopWidgets.StyleAccent(name);
            RectTransform nameRect = name.rectTransform;
            nameRect.anchorMin = nameRect.anchorMax = new Vector2(0f, 0.5f);
            nameRect.anchoredPosition = UIStyles.SHOP_BUNDLE_NAME_POS;

            Button pill = null;
            pill = ShopWidgets.CreatePill(row, "Pill", "ui_btn_green_wide", new Vector2(1f, 0.5f),
                new Vector2(UIStyles.SHOP_BUNDLE_PILL_X, 0f), UIStyles.SHOP_BUNDLE_PILL_W, () =>
                {
                    if (ShopService.TryBuyProCookPack()) screen.NotifyChanged();
                    else ShopScreen.Deny(pill.transform);
                });
            ShopWidgets.SetPillLabel(pill, pack.StarCost.ToString(), "ui_star");
        }

        // Stars are bought with gems (hard → soft, one-directional). Gem spends get a confirm
        // dialog — the one place the shop asks "are you sure". (Gift-box art pending: ui_star stands in.)
        private static void BuildStarPacks(RectTransform content, ShopScreen screen)
        {
            ShopWidgets.CreateSectionTitle(content, "STARS");
            RectTransform grid = ShopWidgets.CreateGrid(content, ShopWidgets.CellHeight(withLabel: true));
            foreach (StarProduct product in MonetizationConfig.STAR_PRODUCTS)
            {
                StarProduct captured = product;
                ShopCell cell = ShopWidgets.CreateCell(grid, "Stars_" + product.Amount, product.Amount.ToString(),
                    () => screen.ShowConfirm(captured.Amount, "ui_star", captured.GemCost, "ui_gem", () =>
                    {
                        if (ShopService.TryBuyStarPack(captured)) screen.NotifyChanged();
                        else ShopScreen.Deny(screen.GemPill);
                    }));
                AddPackContents(cell, "ui_star", product.Badge);
                cell.SetPill(product.GemCost.ToString(), "ui_gem");
            }
        }

        // Gem packs are real-money products (mock IAP for now) — the OS confirms those, we don't.
        // The free rewarded-ad rung leads the grid so the "get gems" path always has a $0 cell.
        private static void BuildGemPacks(RectTransform content, ShopScreen screen)
        {
            ShopWidgets.CreateSectionTitle(content, "GEMS");
            RectTransform grid = ShopWidgets.CreateGrid(content, ShopWidgets.CellHeight(withLabel: true));

            ShopCell adCell = ShopWidgets.CreateCell(grid, "Gems_Ad", MonetizationConfig.GEM_REWARD_AD.ToString(), () =>
            {
                if (AdManager.Instance == null) return;
                AdManager.Instance.ShowRewarded(success =>
                {
                    if (success && SaveDataManager.Instance != null)
                        SaveDataManager.Instance.AddGems(MonetizationConfig.GEM_REWARD_AD);
                });
            });
            AddPackContents(adCell, "ui_gem", "");
            // The watch pill replaces the green one: the blue blank with its baked TV icon + a label
            // that tracks live rewarded availability (an ad may finish loading while the shop is open).
            adCell.Pill.image.sprite = UiArt.Load("ui_btn_blue_watch");
            TextMeshProUGUI watchLabel = UIFactory.CreateText(adCell.Pill.transform, "WATCH", UIStyles.SHOP_WATCH_LABEL_POS,
                UIStyles.SHOP_WATCH_LABEL_RECT, UIStyles.SHOP_PILL_TEXT_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(watchLabel);
            UIFactory.AutoFit(watchLabel, UIStyles.SHOP_WATCH_LABEL_MIN, UIStyles.SHOP_PILL_TEXT_SIZE);
            screen.RegisterPerFrame(() =>
            {
                bool available = AdManager.Instance != null && AdManager.Instance.IsRewardedAvailable;
                if (adCell.Button.interactable == available) return;
                adCell.Button.interactable = available;
                adCell.Pill.interactable = available;
                watchLabel.text = available ? "WATCH" : "LOADING...";
            });

            foreach (GemProduct product in MonetizationConfig.GEM_PRODUCTS)
            {
                GemProduct captured = product;
                ShopCell cell = ShopWidgets.CreateCell(grid, "Gems_" + product.Amount, product.Amount.ToString(),
                    () => { ShopService.BuyGemPack(captured); screen.NotifyChanged(); });
                AddPackContents(cell, "ui_gem", product.Badge);
                cell.SetPill(ShopWidgets.MoneyLabel(product.PriceLabel));
            }
        }

        // A currency pack cell's box: the (stand-in) pack icon, plus a gold merchandising badge
        // on the label line when the product carries one.
        private static void AddPackContents(ShopCell cell, string iconArt, string badge)
        {
            Sprite icon = UiArt.Load(iconArt);
            UIFactory.CreateImage(cell.Box, "Icon", icon, Center, Vector2.zero, UIFactory.SizeByHeight(icon, UIStyles.SHOP_ITEM_ICON_H));

            cell.Label.alignment = TextAlignmentOptions.Left;
            if (string.IsNullOrEmpty(badge)) return;

            TextMeshProUGUI badgeText = UIFactory.CreateText(cell.Label.transform, badge, Vector2.zero,
                cell.Label.rectTransform.sizeDelta, UIStyles.SHOP_BADGE_SIZE, FontStyles.Bold,
                UIStyles.SHOP_BADGE_COLOR, TextAlignmentOptions.Right);
            badgeText.rectTransform.anchorMin = Vector2.zero;
            badgeText.rectTransform.anchorMax = Vector2.one;
            badgeText.rectTransform.sizeDelta = Vector2.zero;
        }

        private static Sprite ConsumableIcon(ConsumableType type) =>
            UiArt.Load("ui_consumable_" + type.ToString().ToLowerInvariant());

        private static int OwnedCount(ConsumableType type) =>
            ConsumableInventory.Instance != null ? ConsumableInventory.Instance.CountOf(type)
            : SaveDataManager.Instance != null ? SaveDataManager.Instance.ConsumableCount(type) : 0;
    }
}
