namespace DogtorBurguer
{
    /// <summary>
    /// A purchasable gem pack (IAP store product). The granted amount and the
    /// displayed price live together so a button label and its grant can't drift.
    /// Distinct from the in-game <see cref="GemPack"/> collectible.
    /// </summary>
    public readonly struct GemProduct
    {
        public readonly int Amount;
        public readonly string PriceLabel;

        public GemProduct(int amount, string priceLabel)
        {
            Amount = amount;
            PriceLabel = priceLabel;
        }
    }
}
