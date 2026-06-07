namespace DogtorBurguer
{
    /// <summary>
    /// Monetization tuning — gems, ads, IAP, drop rates. Owned by business/PM.
    /// Split out of Constants.cs per the 6-config-file architecture.
    /// </summary>
    public static class MonetizationConfig
    {
        #region Gems
        public const int CONTINUE_GEM_COST = 50;
        public const int GEM_REWARD_AD = 25;
        public const int GEM_PACK_VALUE = 5;
        #endregion

        #region Ads
        public const int INTERSTITIAL_EVERY_N_GAMES = 3;
        #endregion

        #region Gem Pack Drops
        public const float GEM_PACK_SPAWN_CHANCE = 0.08f;
        public const float GEM_PACK_SPAWN_INTERVAL = 10f;
        #endregion

        #region IAP Products
        // Store products offered in the Shop. Amount + displayed price live in one
        // place; ShopPanel derives both the button label and the grant from these.
        public static readonly GemProduct[] GEM_PRODUCTS =
        {
            new GemProduct(100, "$0.99"),
            new GemProduct(500, "$3.99"),
        };
        #endregion
    }
}
