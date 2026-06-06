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
        public const int CHALLENGE_MIN_SIZE = 2;
        public const int CHALLENGE_MAX_SIZE = 7;
        public const int CHALLENGE_MAX_CONTAINS = 3;
        public const int CHALLENGE_MATCH_MULTIPLIER = 3;
        public const int CHALLENGE_GLOBAL_MULT_PER_LEVEL = 5;
        #endregion

        #region Column Swap
        public const float SWAP_WAVE_DELAY_PER_ROW = 0.04f;
        public const float SWAP_THRESHOLD_BUFFER_MULT = 0.2f;
        public const float SWAP_POST_ANIM_DELAY = 0.3f;
        #endregion

        #region Difficulty Curve
        public const float INITIAL_FALL_STEP_DURATION = 0.5f;
        public const float MIN_FALL_STEP_DURATION = 0.1f;
        public const int MAX_LEVEL = 20;
        public const int STARTING_INGREDIENT_COUNT = 3;
        public const int MAX_INGREDIENT_COUNT = 7;
        #endregion

        #region Scoring
        public const int POINTS_MATCH = 10;
        public const int POINTS_PER_INGREDIENT = 10;
        public const int BONUS_POOR_BURGER = 5;      // 0 ingredients
        public const int BONUS_SMALL_BURGER = 20;    // 1-2 ingredients
        public const int BONUS_MEDIUM_BURGER = 50;   // 3-4 ingredients
        public const int BONUS_LARGE_BURGER = 100;   // 5-6 ingredients
        public const int BONUS_MEGA_BURGER = 200;    // 7-8 ingredients
        public const int BONUS_MAX_BURGER = 500;     // 9+ ingredients
        #endregion

        #region Difficulty Thresholds
        /// <summary>
        /// Ingredients placed required to reach each level (index 0 = level 1).
        /// </summary>
        public static readonly int[] LEVEL_THRESHOLDS = {
            0, 10, 22, 36, 52, 70, 90, 112, 136, 162,
            190, 220, 252, 286, 322, 360, 400, 442, 486, 532
        };
        #endregion
    }
}
