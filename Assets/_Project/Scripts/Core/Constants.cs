namespace DogtorBurguer
{
    public static class Constants
    {
        // Grid dimensions
        public const int COLUMN_COUNT = 4;
        public const int MAX_ROWS = 13;

        // Cell size in world units
        public const float CELL_WIDTH = 1.4f;
        public const float CELL_VISUAL_HEIGHT = 0.40f; // 60% overlap between rows

        // Grid positioning (bottom-left of grid)
        public const float GRID_ORIGIN_X = -2.1f;
        public const float GRID_ORIGIN_Y = -4.2f;

        // Chef positions (between columns) — always one fewer than the columns
        public const int CHEF_POSITION_COUNT = COLUMN_COUNT - 1;
        public const int CHEF_START_POSITION = 1; // default middle position

        // Sorting layers (sprite/text render order; higher = nearer the camera).
        // Structural — these define z-ordering between systems, not gameplay feel.
        public const int SORT_BACKGROUND = -100;
        public const int SORT_BACKGROUND_FILTER = -99;
        public const int SORT_GAME_PANEL = -50;
        public const int SORT_CHEF_BUBBLE = -1;
        public const int SORT_CHALLENGE_BASE = 60;
        public const int SORT_WAVE_PREVIEW = 90;
        public const int SORT_GEM_PACK = 100;        // fairy body
        public const int SORT_FAIRY_BADGE = 101;     // payload badge, just above the fairy body
        public const int SORT_CONSUMABLE_SLOT = 90;  // inventory icons in the score panel
        public const int SORT_CONSUMABLE_GHOST = 92; // translucent column preview while dragging
        public const int SORT_CONSUMABLE_FALLER = 95;// the consumable dropping into a column
        public const int SORT_CONSUMABLE_CARRY = 150;// the icon held under the finger (on top of all)
        public const int SORT_FLOATING_TEXT = 100;
        public const int SORT_SCORE_POPUP = 100;
        public const int SORT_FEEDBACK_TEXT = 100;
        public const int SORT_BURGER_POPUP = 110;
        public const int SORT_SCREEN_FLASH = 200;

        // World-space z depth for layered 2D frames (sprite draw order is governed
        // by the SORT_* sorting orders above; this positions the panel in world z).
        public const float Z_GAME_PANEL = 5f;
        public const float Z_BACKGROUND = 10f;
        public const float Z_BACKGROUND_FILTER = 9.9f;

        // Gem-pack fly-across spawn geometry (off-screen X, upper-area Y band).
        public const float GEM_SPAWN_EDGE_X = 5f;
        public const float GEM_SPAWN_Y_MIN = 0f;
        public const float GEM_SPAWN_Y_MAX = 3f;
    }
}
