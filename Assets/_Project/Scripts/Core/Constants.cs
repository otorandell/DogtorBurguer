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
    }
}
