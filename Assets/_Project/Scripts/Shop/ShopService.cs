using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Purchase and equip rules for the Shop — UI-free, so the screen stays layout-only.
    /// All Try* methods are atomic: they either complete the whole transaction (spend + grant)
    /// or leave everything untouched and return false.
    /// </summary>
    public static class ShopService
    {
        /// <summary>Default skins are implicitly owned; bought skins are recorded by id.</summary>
        public static bool OwnsSkin(Skin skin) =>
            skin != null && (skin.IsDefault ||
                (SaveDataManager.Instance != null && SaveDataManager.Instance.OwnsSkin(skin.Id)));

        /// <summary>Buys a skin with its unlock currency and auto-equips it (buy = wear).</summary>
        public static bool TryBuySkin(Skin skin)
        {
            SaveDataManager save = SaveDataManager.Instance;
            if (skin == null || save == null || OwnsSkin(skin)) return false;

            bool paid = skin.Unlock == UnlockMethod.Gems
                ? save.SpendGems(skin.GemCost)
                : save.SpendStars(skin.StarCost);
            if (!paid) return false;

            save.GrantSkin(skin.Id);
            Theme.Equip(skin);
            return true;
        }

        /// <summary>Equips an owned (or default) skin. No-op on skins the player doesn't own.</summary>
        public static bool TryEquip(Skin skin)
        {
            if (!OwnsSkin(skin)) return false;
            Theme.Equip(skin);
            return true;
        }

        public static bool TryBuyConsumable(ConsumableType type, ConsumablePack pack)
        {
            SaveDataManager save = SaveDataManager.Instance;
            if (save == null || !save.SpendStars(pack.StarCost)) return false;

            save.AddConsumables(type, pack.Quantity);
            return true;
        }

        /// <summary>The Pro Cook Pack: one star spend, then the pack's quantity of every type.</summary>
        public static bool TryBuyProCookPack()
        {
            SaveDataManager save = SaveDataManager.Instance;
            ConsumablePack pack = MonetizationConfig.PRO_COOK_PACK;
            if (save == null || !save.SpendStars(pack.StarCost)) return false;

            save.AddConsumables(ConsumableType.Ketchup, pack.Quantity);
            save.AddConsumables(ConsumableType.Mustard, pack.Quantity);
            save.AddConsumables(ConsumableType.Skewer, pack.Quantity);
            return true;
        }

        public static bool TryBuyStarPack(StarProduct product)
        {
            SaveDataManager save = SaveDataManager.Instance;
            if (save == null || !save.SpendGems(product.GemCost)) return false;

            save.AddStars(product.Amount);
            return true;
        }

        /// <summary>IAP stub — grants immediately; the real store flow lands with the IAP SDK.</summary>
        public static void BuyGemPack(GemProduct product)
        {
            Debug.Log($"[Shop] IAP stub - 'bought' {product.Amount} gems for {product.PriceLabel}");
            SaveDataManager.Instance?.AddGems(product.Amount);
        }

        /// <summary>IAP stub — remove ads plus the bundled gem sweetener. One-time.</summary>
        public static void BuyRemoveAds()
        {
            SaveDataManager save = SaveDataManager.Instance;
            if (save == null || save.AdsRemoved) return;

            Debug.Log($"[Shop] IAP stub - 'bought' Remove Ads for {MonetizationConfig.REMOVE_ADS_PRICE_LABEL}");
            save.SetAdsRemoved();
            save.AddGems(MonetizationConfig.REMOVE_ADS_BONUS_GEMS);
        }
    }
}
