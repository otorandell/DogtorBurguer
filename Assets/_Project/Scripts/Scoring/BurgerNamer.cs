namespace DogtorBurguer
{
    /// <summary>The flavor name shown when a burger completes: one fixed name per size — a
    /// generic praise ladder ending in the house special (simplified 2026-09-08; the old
    /// random prefix/noun/adjective combos read like word salad).</summary>
    public static class BurgerNamer
    {
        // Indexed by ingredient count - 1, clamped at the top. Trial-font-safe characters
        // only; the excitement marks are baked into the strings (! -> !! -> !!! at the top).
        private static readonly string[] NameBySize =
        {
            "Good Burger!",       // 1
            "Tasty Burger!",      // 2
            "Great Burger!",      // 3
            "Delicious Burger!",  // 4
            "Awesome Burger!!",   // 5
            "Incredible Burger!!",// 6
            "Exquisite Burger!!", // 7
            "Gourmet Burger!!",   // 8
        };

        public static string Generate(int ingredientCount)
        {
            if (ingredientCount <= 0)
                return "Just Bread...";
            if (ingredientCount > NameBySize.Length)
                return "DOGTOR BURGER!!!";
            return NameBySize[ingredientCount - 1];
        }
    }
}
