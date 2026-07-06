namespace DogtorBurguer
{
    /// <summary>
    /// A purchasable star pack, priced in gems (the standard one-directional hard→soft
    /// exchange). Amount and cost live together so a button label and its grant can't drift.
    /// </summary>
    public readonly struct StarProduct
    {
        public readonly int Amount;
        public readonly int GemCost;
        /// <summary>Merchandising tag shown on the card; "" for none.</summary>
        public readonly string Badge;

        public StarProduct(int amount, int gemCost, string badge = "")
        {
            Amount = amount;
            GemCost = gemCost;
            Badge = badge;
        }
    }
}
