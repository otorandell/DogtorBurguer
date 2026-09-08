namespace DogtorBurguer
{
    /// <summary>The flavor name shown when a burger completes: one fixed name per size — a
    /// praise ladder ending in the house special (simplified 2026-09-08; localized the same
    /// day — one LocKey per rung).</summary>
    public static class BurgerNamer
    {
        // Indexed by ingredient count - 1, clamped at the top.
        private static readonly LocKey[] NameBySize =
        {
            LocKey.NamerGood,       // 1
            LocKey.NamerTasty,      // 2
            LocKey.NamerGreat,      // 3
            LocKey.NamerDelicious,  // 4
            LocKey.NamerAwesome,    // 5
            LocKey.NamerIncredible, // 6
            LocKey.NamerExquisite,  // 7
            LocKey.NamerGourmet,    // 8
        };

        public static string Generate(int ingredientCount)
        {
            if (ingredientCount <= 0)
                return Loc.Get(LocKey.NamerJustBread);
            if (ingredientCount > NameBySize.Length)
                return Loc.Get(LocKey.NamerDogtorBurger);
            return Loc.Get(NameBySize[ingredientCount - 1]);
        }
    }
}
