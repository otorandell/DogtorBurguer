using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// Composes the Shop page to the mock, top to bottom: the support banner (the authored
    /// Remove-Ads offer, or THANK YOU once bought), skins (Dogtor, then ingredients), the power-up
    /// grid + Pro Cook Pack, star packs (gems → stars, confirm-gated), gem packs (free ad rung +
    /// IAP). Pure composition on top of ShopWidgets; every transaction routes through ShopService.
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

        // Top of the page: the authored Remove-Ads banner (one tappable image — price, gem bonus and
        // the ONE TIME BUY tag are baked in) while unbought; afterwards the mock's THANK YOU FOR
        // SUPPORTING US! box takes its slot. The grant arrives through IapManager (store callback).
        private static void BuildSupportBanner(RectTransform content, ShopScreen screen)
        {
            Sprite offerArt = UiArt.Load("ui_shop_remove_ads");
            Button offer = null;
            offer = UIFactory.CreateSpriteButton(content, "RemoveAds", offerArt, Center, Vector2.zero,
                UIFactory.SizeByWidth(offerArt, UIStyles.SHOP_CONTENT_W),
                () => StorePurchase(MonetizationConfig.REMOVE_ADS_STORE_ID, offer.transform));
            offer.image.preserveAspect = true; // the page layout stretches widths; keep the art's shape
            offer.gameObject.AddComponent<LayoutElement>().preferredHeight =
                UIFactory.SizeByWidth(offerArt, UIStyles.SHOP_CONTENT_W).y;

            Image thanksBox = ShopWidgets.CreateBox(content, "ThankYou", UIStyles.SHOP_BANNER_H);
            TextMeshProUGUI thanksText = UIFactory.CreateText(thanksBox.transform, "THANK YOU FOR\nSUPPORTING US!", Vector2.zero,
                new Vector2(UIStyles.SHOP_CONTENT_W, UIStyles.SHOP_BANNER_H) - UIStyles.SHOP_BANNER_TEXT_INSET,
                UIStyles.SHOP_BANNER_TEXT_SIZE, FontStyles.Bold);
            ShopWidgets.StyleAccent(thanksText);

            void Refresh()
            {
                bool removed = SaveDataManager.Instance != null && SaveDataManager.Instance.AdsRemoved;
                offer.gameObject.SetActive(!removed);
                thanksBox.gameObject.SetActive(removed);
            }
            Refresh();
            screen.RegisterRefresh(Refresh);
        }

        private static void BuildSkinRow(RectTransform content, ShopScreen screen, string title, List<Skin> skins)
        {
            if (skins.Count == 0) return;

            ShopWidgets.CreateSectionTitle(content, title);
            RectTransform row = ShopWidgets.CreateHorizontalRow(content, ShopWidgets.CellHeight(true, ShopWidgets.SkinBoxArt));
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
                RectTransform row = ShopWidgets.CreateHorizontalRow(content, ShopWidgets.CellHeight(true, ShopWidgets.SkinBoxArt));
                foreach (Skin skin in skins)
                    ShopSkinCell.Create(row, skin, screen);
            }
        }

        // The power-up grid: one row per pack rung (x1, x3, …), one column per consumable — each
        // cell shows the owned count badge, the icon (the trio art from x3 up) and the quantity;
        // then the Pro Cook Pack row. Star spends are instant (no confirm — soft currency).
        private static void BuildPowerUps(RectTransform content, ShopScreen screen)
        {
            ShopWidgets.CreateSectionTitle(content, "POWER-UPS");

            RectTransform grid = ShopWidgets.CreateGrid(content, ShopWidgets.CellHeight(false, ShopWidgets.ItemBoxArt));
            foreach (ConsumablePack pack in MonetizationConfig.CONSUMABLE_PACKS)
                foreach (ConsumableType type in ConsumableTypes)
                    BuildPowerUpCell(grid, type, pack, screen);

            BuildProCookPack(content, screen);
        }

        private static void BuildPowerUpCell(RectTransform grid, ConsumableType type, ConsumablePack pack, ShopScreen screen)
        {
            ShopCell cell = null;
            cell = ShopWidgets.CreateCell(grid, $"{type}_x{pack.Quantity}", null, ShopWidgets.ItemBoxArt, () =>
            {
                if (ShopService.TryBuyConsumable(type, pack)) screen.NotifyChanged();
                else ShopScreen.Deny(cell.Root);
            });

            Sprite icon = ConsumableIcon(type, pack.Quantity);
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

        // The bundle row: the condiment tray + quantity, the pack name, and the big green pill.
        private static void BuildProCookPack(RectTransform content, ShopScreen screen)
        {
            ConsumablePack pack = MonetizationConfig.PRO_COOK_PACK;
            Image box = ShopWidgets.CreateBox(content, "ProCookPack", UIStyles.SHOP_BUNDLE_H);
            RectTransform row = box.rectTransform;

            Sprite tray = UiArt.Load("ui_shop_condiment_pack");
            UIFactory.CreateImage(row, "Tray", tray, new Vector2(0f, 0.5f),
                new Vector2(UIStyles.SHOP_BUNDLE_ICON_X, UIStyles.SHOP_BUNDLE_ICON_Y),
                UIFactory.SizeByHeight(tray, UIStyles.SHOP_BUNDLE_ICON_H));
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
            pill = ShopWidgets.CreatePill(row, "Pill", "ui_btn_green_big", new Vector2(1f, 0.5f),
                new Vector2(UIStyles.SHOP_BUNDLE_PILL_X, 0f), UIStyles.SHOP_BUNDLE_PILL_W, () =>
                {
                    if (ShopService.TryBuyProCookPack()) screen.NotifyChanged();
                    else ShopScreen.Deny(pill.transform);
                });
            ShopWidgets.CreateIconLine(pill.transform, "Face", UIStyles.SHOP_PILL_LABEL_NUDGE,
                pill.GetComponent<RectTransform>().sizeDelta, pack.StarCost.ToString(),
                UIStyles.SHOP_BUNDLE_PILL_TEXT_SIZE, "ui_star", UIStyles.SHOP_BUNDLE_PILL_ICON_H);
        }

        // Stars are bought with gems (hard → soft, one-directional). Gem spends get a confirm
        // dialog — the one place the shop asks "are you sure". Pack art by ladder position.
        private static void BuildStarPacks(RectTransform content, ShopScreen screen)
        {
            ShopWidgets.CreateSectionTitle(content, "STARS");
            RectTransform grid = ShopWidgets.CreateGrid(content, ShopWidgets.CellHeight(true, ShopWidgets.ItemBoxArt));
            StarProduct[] products = MonetizationConfig.STAR_PRODUCTS;
            for (int i = 0; i < products.Length; i++)
            {
                StarProduct captured = products[i];
                ShopCell cell = ShopWidgets.CreateCell(grid, "Stars_" + captured.Amount, captured.Amount.ToString(),
                    ShopWidgets.ItemBoxArt,
                    () => screen.ShowConfirm(captured.Amount, "ui_star", captured.GemCost, "ui_gem", () =>
                    {
                        if (ShopService.TryBuyStarPack(captured)) screen.NotifyChanged();
                        else ShopScreen.Deny(screen.GemPill);
                    }));
                AddPackContents(cell, "ui_pack_stars_" + (i + 1), captured.Badge);
                cell.SetPill(captured.GemCost.ToString(), "ui_gem");
            }
        }

        // Gem packs are real-money products — the store's own dialog confirms those, we don't. The
        // free rewarded-ad rung leads the grid so the "get gems" path always has a $0 cell; the
        // App Store's mandatory Restore Purchases sits under the grid.
        private static void BuildGemPacks(RectTransform content, ShopScreen screen)
        {
            ShopWidgets.CreateSectionTitle(content, "GEMS");
            RectTransform grid = ShopWidgets.CreateGrid(content, ShopWidgets.CellHeight(true, ShopWidgets.ItemBoxArt));

            ShopCell adCell = ShopWidgets.CreateCell(grid, "Gems_Ad", MonetizationConfig.GEM_REWARD_AD.ToString(),
                ShopWidgets.ItemBoxArt, () =>
                {
                    if (AdManager.Instance == null) return;
                    AdManager.Instance.ShowRewarded(success =>
                    {
                        if (success && SaveDataManager.Instance != null)
                            SaveDataManager.Instance.AddGems(MonetizationConfig.GEM_REWARD_AD);
                    });
                });
            AddPackContents(adCell, "ui_gem", "");
            // The watch pill replaces the green one: the authored blank with its baked TV icon (sized by
            // height — it's a wider shape) + a label that tracks live rewarded availability (an ad may
            // finish loading while the shop is open).
            Sprite watchArt = UiArt.Load("ui_shop_watch");
            adCell.Pill.image.sprite = watchArt;
            adCell.Pill.GetComponent<RectTransform>().sizeDelta = UIFactory.SizeByHeight(watchArt, UIStyles.SHOP_WATCH_PILL_H);
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

            GemProduct[] products = MonetizationConfig.GEM_PRODUCTS;
            for (int i = 0; i < products.Length; i++)
            {
                GemProduct captured = products[i];
                ShopCell cell = null;
                cell = ShopWidgets.CreateCell(grid, "Gems_" + captured.Amount, captured.Amount.ToString(),
                    ShopWidgets.ItemBoxArt, () => StorePurchase(captured.StoreId, cell.Root));
                AddPackContents(cell, "ui_pack_gems_" + (i + 1), captured.Badge);
                cell.SetPill(StorePrice(captured.StoreId, captured.PriceLabel));
            }

#if UNITY_IOS
            // The App Store requires a user-facing restore button; everywhere else (Google Play,
            // the editor) the store restores automatically at init and IAP 5 rejects the call
            // ("not a supported platform for the restore button"), so the row is iOS-only.
            TextMeshProUGUI restore = UIFactory.CreateText(content, "Restore Purchases", Vector2.zero, Vector2.zero,
                UIStyles.SHOP_SUBTITLE_SIZE, FontStyles.Bold, UIStyles.TOPBAR_NUMBER_COLOR);
            restore.gameObject.AddComponent<LayoutElement>().preferredHeight = UIStyles.SHOP_RESTORE_H;
            restore.raycastTarget = true;
            restore.gameObject.AddComponent<Button>().onClick.AddListener(() =>
                IapManager.Instance?.RestorePurchases(ok => { if (!ok) ShopScreen.Deny(restore.transform); }));
#endif
        }

        // A real-money purchase: the store dialog handles confirmation; the grant lands via
        // IapManager.OnGranted (the screen re-renders on it). Only a failure shakes the cell —
        // a cancel is the player's choice.
        private static void StorePurchase(string storeId, Transform denyTarget)
        {
            if (IapManager.Instance == null) return;
            IapManager.Instance.Purchase(storeId, result =>
            {
                if (result != IapResult.Success && result != IapResult.Cancelled && denyTarget != null)
                    ShopScreen.Deny(denyTarget);
            });
        }

        // The store's localized price when it has one (shown verbatim — it's what the store dialog
        // will say), else the config placeholder with the "$" dropped for the trial font.
        private static string StorePrice(string storeId, string fallbackLabel)
        {
            string fallback = ShopWidgets.MoneyLabel(fallbackLabel);
            return IapManager.Instance != null ? IapManager.Instance.PriceLabel(storeId, fallback) : fallback;
        }

        // A currency pack cell's box: the pack icon, plus a gold merchandising badge on the label
        // line when the product carries one.
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

        // Single icon for the x1 rung, the trio art from x3 up.
        private static Sprite ConsumableIcon(ConsumableType type, int quantity)
        {
            string name = type.ToString().ToLowerInvariant();
            return UiArt.Load(quantity >= 3 ? "ui_shop_trio_" + name : "ui_consumable_" + name);
        }

        private static int OwnedCount(ConsumableType type) =>
            ConsumableInventory.Instance != null ? ConsumableInventory.Instance.CountOf(type)
            : SaveDataManager.Instance != null ? SaveDataManager.Instance.ConsumableCount(type) : 0;
    }
}
