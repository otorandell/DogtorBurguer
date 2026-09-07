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
        public const float TOP_BUN_BASE_CHANCE = 0.10f;               // top chance at exactly one open bottom (0.08 pre-2026-09-05 — closing felt slow)
        public const float TOP_BUN_CHANCE_PER_EXTRA_BOTTOM = 0.05f;   // added per additional open bottom (was 0.04)
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
        // 2026-09-05 redesign (Oscar): order difficulty scales with the CHALLENGE (multiplier)
        // level, never the game level. Every order is an exact-count recipe (named + free slots)
        // from the ladder tables (the Special Order Ladder region below). The global multiplier
        // is gentle and applies to ALL gameplay score.
        public const float CHALLENGE_MULT_STEP = 0.25f;               // global mult = 1 + step·(level−1): 1, 1.25, 1.5 …
        public const int CHALLENGE_MATCH_MULTIPLIER = 3;              // extra ×3 on the matched burger itself
        public const int CHALLENGE_ORDERS_TO_LEVEL_CAP = 3;           // orders per mult level: (level+3)/2 capped here (2, 2, 3, 3, 3 …)
        public const int ORDER_MAX_SIZE = Constants.MAX_ROWS - 2;     // biggest physically possible burger (11): a full column minus its two buns
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

        // Highest level selectable from the Settings START level row (a player feature since
        // 2026-09-07 — it replaced the mode toggle). TESTING: set to KILLER_LEVEL so the kill
        // screen can be entered directly; drop to MAX_LEVEL (or lower) for release.
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

        #region Difficulty Thresholds (ingredients placed to reach each level, index 0 = level 1)
        // TIME-BUDGETED (2026-09-07, Oscar): a placement count is a bad clock — pieces fall ~4x
        // faster at L20 than at L1, so equal counts meant late levels flashed by (L1 ~68 s, L19
        // ~24 s on the old 2x table). Each level is now sized for ~42 s of play (60 s felt long —
        // trimmed 30%, Oscar 2026-09-07) from the speed curve: placements = 42 / secPerPlacement,
        // secPerPlacement ≈ (9 rows × fallStep + WAVE_MOVE_DURATION) / (2 + tripleChance).
        // Re-derive when FALL_STEP_BY_LEVEL or TRIPLE_CHANCE_BY_LEVEL change
        // (scratchpad/level_time.py). ~14 min to the kill screen.
        // One ruleset since 2026-09-07 (the mode toggle went; speed players pick a higher START
        // level in Settings). MAX_LEVEL long (asserted).
        public static readonly int[] LEVEL_THRESHOLDS = {
            0, 20, 44, 70, 98, 129, 162, 198, 237, 278,
            323, 372, 424, 480, 540, 604, 673, 748, 829, 917
        };
        public const int KILLER_LEVEL_THRESHOLD = 1012; // ingredients placed to enter the kill screen
        #endregion

        #region Special Order Ladder (indexed by challenge level − 1, clamped at the last entry)
        // An order = an exact TOTAL size with NAMED ingredients among it (the rest are free
        // mystery slots): one named ingredient while the free slots grow (L1 1, L2 1+1, L3 1+2),
        // a second named only from level 4, a third from 7, then one more named every ~4 levels
        // while the total grows every 2 (L6 5-2, L7 5-3, L8 6-3, L9 6-3 … — Oscar, 2026-09-06).
        public static readonly int[] ORDER_SIZE_BY_LEVEL = {
            1, 2, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9, 10, 10, 11, 11, 11, 11
        };
        public static readonly int[] ORDER_NAMED_BY_LEVEL = {
            1, 1, 1, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 6, 6, 7
        };
        #endregion

        #region Consumables
        // When a fairy spawns, this is the chance it carries a consumable (vs gems). 60/40 keeps
        // gem income ≈ unchanged while adding ~2 consumables/game (see MonetizationConfig).
        public const float FAIRY_CONSUMABLE_CHANCE = 0.60f;

        // Relative weights for which consumable a consumable-fairy carries. Index by ConsumableType
        // (Ketchup, Mustard, Skewer). Even thirds to start — retune here if playtest needs it
        // (Ketchup = rescue, Mustard = board thinning + cascades, Skewer = mega-burger setup).
        public static readonly float[] CONSUMABLE_SPAWN_WEIGHTS = { 1f, 1f, 1f };

        // Direct removals from a consumable score this per NON-BUN ingredient (flat, no multiplier —
        // match-like, not burger-like). Buns destroyed by consumables score nothing.
        public const int POINTS_CONSUMABLE_PER_INGREDIENT = 10;

        // Mustard sweeps this many distinct REGULAR types read from the targeted column, top down
        // (buns skipped — sweeping open bottoms would kill every burger in progress for 0 points).
        // The pair rule keeps adjacent pieces distinct, so 2 always yields two types from a
        // 2+ regular stack. 1 = the original single-type mustard, which scaled DOWN with level
        // (the shuffle bag spreads types evenly, so a type has ~board/activeTypes copies).
        public const int MUSTARD_SWEEP_TYPES = 2;

        // Mustard's score escalates with the sweep size: the i-th popped piece (0-based) is worth
        // POINTS_CONSUMABLE_PER_INGREDIENT + i * this. 5 pops = 100, 10 pops = 325 (see
        // Scoring.MustardSweepPoints). Rewards the big board-wide clears its identity is about.
        public const int MUSTARD_POINTS_STEP = 5;

        // The consumable faller drops much faster than ingredients and resolves on impact with its
        // target (top-of-stack for Ketchup/Mustard, first bun for Skewer). Seconds per cell.
        public const float CONSUMABLE_FALL_STEP_DURATION = 0.03f;
        #endregion
    }
}
