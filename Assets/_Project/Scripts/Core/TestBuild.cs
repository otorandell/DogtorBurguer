using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// The tester switch: everything unlocked, no ads, free store. Enabled by the menu scene's
    /// MainMenuUI "Test Build" checkbox (the first scene on device, so the flag is set before any
    /// manager reads it). What it does:
    ///   - AdManager creates no ad provider — interstitials never show, rewarded ads "reward"
    ///     instantly (continue, the free-gems rung).
    ///   - IapManager uses the mock store — gem packs and Remove Ads grant for free.
    ///   - ShopService treats every skin as owned (EQUIP everywhere).
    ///   - The currency and consumable stock are topped up to the stash below on each launch.
    ///   - The menu carries a red TEST BUILD label so a build with this on can't be mistaken
    ///     for a release.
    /// Stash values are testing conveniences, not balance — they live here, not in a config.
    /// </summary>
    public static class TestBuild
    {
        private const int STASH_STARS = 99999;
        private const int STASH_GEMS = 9999;
        private const int STASH_CONSUMABLES = 99;

        public static bool IsEnabled { get; private set; }

        /// <summary>Call BEFORE the core managers are created — AdManager/IapManager pick their
        /// provider in Awake.</summary>
        public static void Enable()
        {
            IsEnabled = true;
            Debug.LogWarning("[TestBuild] TEST BUILD — everything unlocked, ads bypassed, store mocked. Do not ship.");
        }

        /// <summary>Tops the persistent stock up to the stash (never takes anything away).
        /// Call once the SaveDataManager exists.</summary>
        public static void TopUpStash(SaveDataManager save)
        {
            if (!IsEnabled || save == null) return;

            if (save.Stars < STASH_STARS) save.AddStars(STASH_STARS - save.Stars);
            if (save.Gems < STASH_GEMS) save.AddGems(STASH_GEMS - save.Gems);
            foreach (ConsumableType type in System.Enum.GetValues(typeof(ConsumableType)))
            {
                int have = save.ConsumableCount(type);
                if (have < STASH_CONSUMABLES) save.AddConsumables(type, STASH_CONSUMABLES - have);
            }
        }
    }
}
