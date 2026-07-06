namespace DogtorBurguer
{
    /// <summary>
    /// A consumable shop rung (quantity + star price), type-agnostic — the same ladder applies
    /// to every <see cref="ConsumableType"/>. Quantity and cost live together so a button label
    /// and its grant can't drift.
    /// </summary>
    public readonly struct ConsumablePack
    {
        public readonly int Quantity;
        public readonly int StarCost;

        public ConsumablePack(int quantity, int starCost)
        {
            Quantity = quantity;
            StarCost = starCost;
        }
    }
}
