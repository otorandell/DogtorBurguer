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
            "Good!",       // 1
            "Tasty!",      // 2
            "Great!",      // 3
            "Delicious!",  // 4
            "Awesome!!",   // 5
            "Incredible!!",// 6
            "Exquisite!!", // 7
            "Gourmet!!",   // 8
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
