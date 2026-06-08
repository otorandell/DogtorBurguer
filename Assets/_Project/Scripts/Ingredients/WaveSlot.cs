namespace DogtorBurguer
{
    /// <summary>One ingredient queued in a wave, with the column it spawns into.</summary>
    public readonly struct WaveSlot
    {
        public readonly IngredientType Type;
        public readonly int ColumnIndex;

        public WaveSlot(IngredientType type, int columnIndex)
        {
            Type = type;
            ColumnIndex = columnIndex;
        }
    }
}
