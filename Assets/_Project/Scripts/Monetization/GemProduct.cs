namespace DogtorBurguer
{
    /// <summary>
    /// A purchasable gem pack (IAP store product). The granted amount and the
    /// displayed price live together so a button label and its grant can't drift.
    /// Distinct from the in-game <see cref="BurgerFairy"/> collectible.
    /// </summary>
    public readonly struct GemProduct
    {
        public readonly int Amount;
        public readonly string PriceLabel;
        /// <summary>Merchandising tag shown on the card ("MOST POPULAR", "BEST VALUE"); "" for none.</summary>
        public readonly string Badge;

        public GemProduct(int amount, string priceLabel, string badge = "")
        {
            Amount = amount;
            PriceLabel = priceLabel;
            Badge = badge;
        }
    }
}
