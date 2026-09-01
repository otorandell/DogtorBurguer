namespace DogtorBurguer
{
    /// <summary>
    /// A purchasable gem pack (IAP store product). The granted amount and the
    /// displayed price live together so a button label and its grant can't drift.
    /// Distinct from the in-game <see cref="BurgerFairy"/> collectible.
    /// </summary>
    public readonly struct GemProduct
    {
        /// <summary>The store product id — must match the Play Console / App Store Connect product exactly.</summary>
        public readonly string StoreId;
        public readonly int Amount;
        /// <summary>Placeholder price shown until the store supplies its localized string.</summary>
        public readonly string PriceLabel;
        /// <summary>Merchandising tag shown on the card ("MOST POPULAR", "BEST VALUE"); "" for none.</summary>
        public readonly string Badge;

        public GemProduct(string storeId, int amount, string priceLabel, string badge = "")
        {
            StoreId = storeId;
            Amount = amount;
            PriceLabel = priceLabel;
            Badge = badge;
        }
    }
}
