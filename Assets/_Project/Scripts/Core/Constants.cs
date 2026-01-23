namespace DogtorBurguer
{
    public static class Constants
    {
        // Grid dimensions
        public const int COLUMN_COUNT = 4;
        public const int MAX_ROWS = 15;

        // Cell size in world units
        public const float CELL_WIDTH = 1.2f;
        public const float CELL_HEIGHT = 1.0f;
        public const float CELL_VISUAL_HEIGHT = 0.40f; // 60% overlap between rows

        // Grid positioning (bottom-left of grid)
        public const float GRID_ORIGIN_X = -1.8f;
        public const float GRID_ORIGIN_Y = -4.0f;

        // Timing (easy start → hard max)
        public const float INITIAL_FALL_STEP_DURATION = 0.3f;
        public const float MIN_FALL_STEP_DURATION = 0.1f;
        public const float SPAWN_INTERVAL_INITIAL = 3.0f;
        public const float SPAWN_INTERVAL_MIN = 1.2f;

        // Difficulty levels
        public const int MAX_LEVEL = 10;
        public const int STARTING_INGREDIENT_COUNT = 3;
        public const int MAX_INGREDIENT_COUNT = 7;

        // Scoring
        public const int POINTS_MATCH = 10;
        public const int POINTS_PER_INGREDIENT = 10;
        public const int BONUS_SMALL_BURGER = 20;    // 1-2 ingredients
        public const int BONUS_MEDIUM_BURGER = 50;   // 3-4 ingredients
        public const int BONUS_LARGE_BURGER = 100;   // 5-6 ingredients
        public const int BONUS_MEGA_BURGER = 200;    // 7+ ingredients

        // Chef positions (between columns)
        public const int CHEF_POSITION_COUNT = 3;

        // Monetization
        public const int CONTINUE_GEM_COST = 50;
        public const int GEM_REWARD_AD = 25;
        public const int GEM_PACK_VALUE = 5;
        public const int INTERSTITIAL_EVERY_N_GAMES = 3;
        public const float GEM_PACK_SPAWN_CHANCE = 0.08f;
        public const float GEM_PACK_SPAWN_INTERVAL = 10f;
    }
}
