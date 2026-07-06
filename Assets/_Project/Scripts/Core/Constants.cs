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

        // Camera framing (see CameraFit). The game is designed for WIDTH: the camera is sized so this
        // world width always fills the screen (the 4 columns span 5.6; +tiny margin). DESIGN_ORTHO_SIZE
        // is the floor for wide screens. At the reference 9:16 these reproduce orthographic size 5.
        // Raise PLAY_AREA_WIDTH for more side margin around the columns.
        public const float PLAY_AREA_WIDTH = 5.625f;
        public const float DESIGN_ORTHO_SIZE = 5f;

        // Chef positions (between columns) — always one fewer than the columns
        public const int CHEF_POSITION_COUNT = COLUMN_COUNT - 1;
        public const int CHEF_START_POSITION = 1; // default middle position
        // The chef's feet sit this far below the grid origin. Position anchors the feet (not the
        // sprite centre), so resizing the chef sprite keeps it planted on the bottom border.
        public const float CHEF_BOTTOM_OFFSET = 1.66f;

        // One decorative plate sits under each column; the bottom ingredient rests on it.
        // Offset below the row-0 ingredient centre (world units).
        public const float PLATE_Y_OFFSET = 0.3f;

        // Sorting layers (sprite/text render order; higher = nearer the camera).
        // Structural — these define z-ordering between systems, not gameplay feel.
        public const int SORT_BACKGROUND = -100;
        public const int SORT_RESTAURANT = -90; // diner scene, above the base fill, below the play mat
        public const int SORT_GAME_PANEL = -50; // blue grid-cell play mat
        // Plate at the back; chef renders over the ingredients (which sort by row, falling = MAX_ROWS+1)
        // but below the challenge/preview/UI layers (60+).
        public const int SORT_PLATE = -2;
        public const int SORT_CHEF = 50;
        public const int SORT_CHALLENGE_BASE = 60;
        public const int SORT_WAVE_PREVIEW = 90;
        public const int SORT_GEM_PACK = 100;        // fairy (full-body per-payload illustration)
        public const int SORT_CONSUMABLE_SLOT = 90;  // inventory icons in the score panel
        public const int SORT_CONSUMABLE_GHOST = 92; // translucent column preview while dragging
        public const int SORT_CONSUMABLE_FX_STREAM = 93; // ketchup stream (under its nozzle)
        public const int SORT_CONSUMABLE_FX_NOZZLE = 94; // use-effect nozzles (under the faller)
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

        // Gem-pack fly-across spawn geometry (off-screen X, upper-area Y band).
        public const float GEM_SPAWN_EDGE_X = 5f;
        public const float GEM_SPAWN_Y_MIN = 0f;
        public const float GEM_SPAWN_Y_MAX = 3f;
    }
}
