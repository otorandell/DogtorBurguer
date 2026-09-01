namespace DogtorBurguer
{
    /// <summary>
    /// Monetization tuning — gems, stars, ads, IAP, drop rates, shop prices. Owned by business/PM.
    /// Split out of Constants.cs per the 6-config-file architecture.
    /// </summary>
    public static class MonetizationConfig
    {
        #region Gems
        public const int CONTINUE_GEM_COST = 50;
        public const int GEM_REWARD_AD = 25;
        public const int GEM_PACK_VALUE = 5;
        // Star fairies award ~the gem pack's worth (5 stars/gem shop rate → 5 gems ≈ 25 stars).
        public const int STAR_PACK_VALUE = 25;
        #endregion

        #region Ads
        public const int INTERSTITIAL_EVERY_N_GAMES = 3;
        #endregion

        #region Ads — LevelPlay credentials (from the Unity Dashboard: LevelPlay → app → ad units)
        // Empty = not configured: AdManager falls back to MockAdProvider (with a loud warning on
        // device builds). Paste the App Key + one Interstitial and one Rewarded ad-unit ID per
        // platform once the dashboard app exists. Keys are per-PLATFORM, not per-build-type.
#if UNITY_IOS
        public const string LEVELPLAY_APP_KEY = "";
        public const string LEVELPLAY_INTERSTITIAL_ID = "";
        public const string LEVELPLAY_REWARDED_ID = "";
#else
        public const string LEVELPLAY_APP_KEY = "27e01a54d";
        public const string LEVELPLAY_INTERSTITIAL_ID = "g2u3waor50a9audu";
        public const string LEVELPLAY_REWARDED_ID = "d2m12nt12g87alee";
#endif
        /// <summary>True when every LevelPlay credential for this platform is filled in.</summary>
        public static bool LevelPlayConfigured =>
            LEVELPLAY_APP_KEY.Length > 0 && LEVELPLAY_INTERSTITIAL_ID.Length > 0 && LEVELPLAY_REWARDED_ID.Length > 0;
        // Launches LevelPlay's on-device test suite after init — flip on for the first
        // device-integration pass, ship OFF.
        public const bool LEVELPLAY_TEST_SUITE = false;
        // Ad personalization policy (decision 2026-09-01): NO consent prompt for v1 — every user is
        // treated as having declined, so LevelPlay serves non-personalized ads everywhere (lower
        // EU eCPMs, but nothing to show and nothing to get wrong) and iOS never asks for
        // tracking (no ATT call → no IDFA). Flipping this to true is NOT enough on its own: a
        // consent prompt (EEA/UK) + the ATT prompt must gate it — see Docs/pre-launch-checklist.md.
        public const bool ADS_PERSONALIZED = false;
        // Not directed at children (matches the privacy policy / Play target audience).
        public const bool ADS_CHILD_DIRECTED = false;
        #endregion

        #region Fairy Drops
        // Burger Fairies carry a consumable (GameplayConfig.FAIRY_CONSUMABLE_CHANCE, 60%) or
        // currency (the rest), which splits gems vs stars by FAIRY_STAR_SHARE. This is the
        // per-interval chance one appears; raising it scales ALL payload kinds equally.
        public const float FAIRY_SPAWN_CHANCE = 0.20f;
        public const float FAIRY_SPAWN_INTERVAL = 10f;
        // Share of currency fairies that carry stars instead of gems (0.5 → 20% stars / 20% gems
        // of all fairies at the 60% consumable rate).
        public const float FAIRY_STAR_SHARE = 0.5f;
        #endregion

        #region IAP Products — gem packs (real money via Unity IAP; the mock store in the editor)
        // Store price-point ladder ($0.99/$4.99/$9.99/$19.99) with monotonically improving
        // gems-per-dollar (+~10/20/30% over the baseline) so every step up is a better deal.
        // Store ids are the Play Console / App Store Connect product ids (see
        // Docs/play-store-listing.md); the price labels are placeholders until the store prices them.
        public static readonly GemProduct[] GEM_PRODUCTS =
        {
            new GemProduct("gems_100", 100, "$0.99"),
            new GemProduct("gems_550", 550, "$4.99", "MOST POPULAR"),
            new GemProduct("gems_1200", 1200, "$9.99"),
            new GemProduct("gems_2600", 2600, "$19.99", "BEST VALUE"),
        };

        /// <summary>The gem pack behind a store id, or null for an unknown id.</summary>
        public static GemProduct? FindGemProduct(string storeId)
        {
            foreach (GemProduct product in GEM_PRODUCTS)
                if (product.StoreId == storeId)
                    return product;
            return null;
        }
        #endregion

        #region Shop — star packs (bought with gems; hard→soft, one-directional)
        // ~5 stars/gem baseline, improving with tier — mirrors the gem ladder's shape.
        public static readonly StarProduct[] STAR_PRODUCTS =
        {
            new StarProduct(200, 40),
            new StarProduct(550, 100),
            new StarProduct(1200, 200, "BEST VALUE"),
        };
        #endregion

        #region Shop — consumables (priced in stars; the soft-currency sink)
        // One ladder for all three types: single at base price, triple at ~11% off.
        public static readonly ConsumablePack[] CONSUMABLE_PACKS =
        {
            new ConsumablePack(1, 150),
            new ConsumablePack(3, 400),
        };
        // The "Pro Cook Pack" bundle: Quantity of EACH consumable type for one star price
        // (9 items for 1000 vs 3 × 400 = 1200 buying the triples separately — ~17% off).
        public static readonly ConsumablePack PRO_COOK_PACK = new ConsumablePack(3, 1000);
        #endregion

        #region Stars — earning (the soft-currency faucet; sinks live in the Shop sections above)
        // Per completed Special Order: BASE + PER_LEVEL·(challengeLevel−1), awarded live.
        // A good run reaching challenge level 4 nets ~50 order stars; casual runs ~10-20.
        public const int STARS_PER_ORDER_BASE = 3;
        public const int STARS_PER_ORDER_PER_LEVEL = 2;
        // End-of-run payout: 1 star per this much score (only score not yet paid out —
        // a continue extends the run without double-paying earlier score).
        public const int STAR_SCORE_DIVISOR = 500;
        #endregion

        #region Shop — remove ads (one-time IAP; kills interstitials, keeps rewarded ads)
        public const string REMOVE_ADS_STORE_ID = "remove_ads";                // non-consumable store product id
        public const string REMOVE_ADS_PRICE_LABEL = "$2.99";                   // placeholder until the store prices it; also baked into ui_shop_remove_ads
        // Bundled gem sweetener — "Remove Ads + gems" converts far better than the bare toggle.
        public const int REMOVE_ADS_BONUS_GEMS = 100;
        #endregion

        #region Debug
        public const int DEBUG_STAR_GRANT = 500; // editor hotkey grant (see TouchInputHandler)
        #endregion
    }
}
