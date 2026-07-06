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
        #endregion

        #region Ingredient Bag
        // Regular ingredients are drawn from a shuffle-bag of (one of each active type +
        // this many random extras) — even spread, no droughts, but non-countable.
        public const int BAG_RANDOM_EXTRAS = 3;
        #endregion

        #region Ingredient Pool
        // Regular (non-bun) ingredients in spawn-progression order (Egg appears in
        // advanced phases). Spawning indexes this list, so IngredientType's int values
        // are no longer load-bearing — adding one is a single append here (F-39).
        public static readonly IngredientType[] REGULAR_INGREDIENTS =
        {
            IngredientType.Meat,
            IngredientType.Cheese,
            IngredientType.Tomato,
            IngredientType.Bacon,
            IngredientType.Onion,
            IngredientType.Pickle,
            IngredientType.Lettuce,
            IngredientType.Egg
        };
        #endregion

        #region Bun Economy
        // Buns are decoupled from the ingredient bag and from level/type-count. Bottom is a flat
        // chance (starts a burger); top only spawns when there's an open bottom to close and scales
        // with the backlog, so open bottoms self-balance near where top chance crosses bottom chance.
        public const float BOTTOM_BUN_CHANCE = 0.12f;                  // flat per-slot chance of a bottom bun
        public const float TOP_BUN_BASE_CHANCE = 0.08f;               // top chance at exactly one open bottom
        public const float TOP_BUN_CHANCE_PER_EXTRA_BOTTOM = 0.04f;   // added per additional open bottom
        public const float TOP_BUN_CHANCE_CAP = 0.40f;                // safety cap (rarely reached)
        public const int BUN_DROUGHT_LIMIT = 15;                      // pieces with no bun → force a bottom
        #endregion

        #region Tap Interaction
        // 0.5 × CELL_WIDTH = adjacent columns' tap circles touch exactly: no dead gap between
        // columns and no cross-column overlap (an edge tap can't grab the neighbor's piece).
        public const float PREVIEW_TAP_RADIUS_MULT = 0.5f;
        public const float FALLING_TAP_RADIUS_MULT = 0.5f;
        public const float CHEF_TAP_RADIUS_MULT = 2f; // tap within this × bubble radius of the chef = swap
        public const float CHEF_MOVE_ZONE_TOP_OFFSET = 0.6f; // Tap-mode move zone extends this far above the grid floor
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
        // Spawner's pre-difficulty default and the column-swap animation duration.
        // (No longer the curve's level-1 speed — that comes from FALL_STEP_BY_LEVEL.)
        public const float INITIAL_FALL_STEP_DURATION = 0.5f;
        // Absolute fastest fall allowed — the SetFallSpeed floor, and the kill-screen speed.
        public const float MIN_FALL_STEP_DURATION = 0.06f;
        public const int MAX_LEVEL = 20;
        public const int KILLER_LEVEL = 21; // Tetris-style kill screen, above the normal curve.
        public const int STARTING_INGREDIENT_COUNT = 4;
        public const int MAX_INGREDIENT_COUNT = 8;

        // Highest level selectable from Settings. TESTING: set to KILLER_LEVEL so the kill
        // screen can be entered directly from the stepper; drop to MAX_LEVEL for release.
        public const int SETTINGS_LEVEL_CAP = KILLER_LEVEL;

        // Per-level curves: index 0 = level 1 … index 19 = level 20. Length MUST equal MAX_LEVEL.
        // The killer level (21) is NOT in these tables — it applies MIN_FALL_STEP_DURATION,
        // MAX_INGREDIENT_COUNT, and always-triple waves directly (see DifficultyManager).
        public static readonly float[] FALL_STEP_BY_LEVEL = {
            0.45f, 0.37f, 0.33f, 0.31f, 0.28f, 0.27f, 0.25f, 0.23f, 0.22f, 0.205f,
            0.19f, 0.18f, 0.17f, 0.16f, 0.15f, 0.14f, 0.13f, 0.12f, 0.11f, 0.10f
        };
        public static readonly int[] INGREDIENT_COUNT_BY_LEVEL = {
            4, 4, 4, 5, 5, 5, 5, 6, 6, 6,
            6, 7, 7, 7, 7, 8, 8, 8, 8, 8
        };
        public static readonly float[] TRIPLE_CHANCE_BY_LEVEL = {
            0f,    0f,    0f,    0f,    0f,    0.05f, 0.08f, 0.11f, 0.15f, 0.18f,
            0.22f, 0.26f, 0.30f, 0.34f, 0.38f, 0.41f, 0.44f, 0.47f, 0.49f, 0.50f
        };
        public const int KILLER_LEVEL_THRESHOLD = 434; // ingredients placed to enter the kill screen
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
            0, 16, 33, 50, 68, 86, 105, 124, 144, 164,
            185, 206, 228, 250, 273, 296, 320, 344, 369, 394
        };
        #endregion

        #region Consumables
        // When a fairy spawns, this is the chance it carries a consumable (vs gems). 60/40 keeps
        // gem income ≈ unchanged while adding ~2 consumables/game (see MonetizationConfig).
        public const float FAIRY_CONSUMABLE_CHANCE = 0.60f;

        // Relative weights for which consumable a consumable-fairy carries. Index by ConsumableType
        // (Ketchup, Mustard, Skewer). Even thirds to start — down-weight Mustard (strongest, board-
        // wide) or up-weight Skewer (situational) here if playtest needs it.
        public static readonly float[] CONSUMABLE_SPAWN_WEIGHTS = { 1f, 1f, 1f };

        // Direct removals from a consumable score this per NON-BUN ingredient (flat, no multiplier —
        // match-like, not burger-like). Buns destroyed by consumables score nothing.
        public const int POINTS_CONSUMABLE_PER_INGREDIENT = 10;

        // The consumable faller drops much faster than ingredients and resolves on impact with its
        // target (top-of-stack for Ketchup/Mustard, first bun for Skewer). Seconds per cell.
        public const float CONSUMABLE_FALL_STEP_DURATION = 0.03f;
        #endregion
    }
}
