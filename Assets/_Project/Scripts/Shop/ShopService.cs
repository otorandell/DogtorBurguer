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
        /// <summary>Default skins are implicitly owned; bought skins are recorded by id. A test
        /// build owns everything (nothing is persisted — flip the flag off and ownership is real again).</summary>
        public static bool OwnsSkin(Skin skin) =>
            skin != null && (skin.IsDefault || TestBuild.IsEnabled ||
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

        /// <summary>Grant half of a gem-pack purchase — called by IapManager once the store has
        /// completed (or replayed) the transaction. Never call from UI; go through IapManager.Purchase.</summary>
        public static void GrantGemPack(GemProduct product)
        {
            SaveDataManager.Instance?.AddGems(product.Amount);
        }

        /// <summary>Grant half of Remove Ads plus its bundled gem sweetener. Idempotent — the store
        /// replays owned non-consumables on every init / restore.</summary>
        public static void GrantRemoveAds()
        {
            SaveDataManager save = SaveDataManager.Instance;
            if (save == null || save.AdsRemoved) return;

            save.SetAdsRemoved();
            save.AddGems(MonetizationConfig.REMOVE_ADS_BONUS_GEMS);
        }
    }
}
