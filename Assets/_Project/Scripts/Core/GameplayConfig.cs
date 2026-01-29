namespace DogtorBurguer
{
    /// <summary>
    /// Gameplay tuning values. Change these to affect how the game plays.
    /// Structural constants (grid size, max rows) remain in Constants.cs.
    /// </summary>
    public static class GameplayConfig
    {
        #region Wave Spawning
        public const float INITIAL_SPAWN_DELAY = 1.5f;
        public const float FORCED_BUN_MULTIPLIER = 1.5f;
        public const int TRIPLE_WAVE_START_LEVEL = 8;
        public const float TRIPLE_WAVE_MAX_CHANCE = 0.35f;
        #endregion

        #region Bun Selection
        public const float BUN_TOP_BASE_CHANCE = 0.5f;
        public const float BUN_TOP_CHANCE_PER_BOTTOM = 0.08f;
        public const float BUN_TOP_CHANCE_CAP = 0.8f;
        #endregion

        #region Tap Interaction
        public const float PREVIEW_TAP_RADIUS_MULT = 0.7f;
        public const float FALLING_TAP_RADIUS_MULT = 0.6f;
        public const float FAST_DROP_POINTS_PER_UNIT = 2f;
        #endregion

        #region Burger Challenge
        public const int MAX_CHALLENGE_INGREDIENTS = 5;
        public const int CHALLENGE_MATCH_MULTIPLIER = 3;
        public const int CHALLENGE_GLOBAL_MULT_PER_LEVEL = 5;
        public const int CHALLENGE_COMBO_MAX_ATTEMPTS = 200;
        #endregion

        #region Column Swap
        public const float SWAP_WAVE_DELAY_PER_ROW = 0.04f;
        public const float SWAP_THRESHOLD_BUFFER_MULT = 0.2f;
        public const float SWAP_POST_ANIM_DELAY = 0.3f;
        #endregion

        #region Difficulty Thresholds
        /// <summary>
        /// Ingredients placed required to reach each level (index 0 = level 1).
        /// </summary>
        public static readonly int[] LEVEL_THRESHOLDS = {
            0, 3, 7, 12, 18, 25, 33, 42, 52, 64,
            77, 91, 106, 122, 139, 157, 176, 196, 217, 239
        };
        #endregion
    }
}
