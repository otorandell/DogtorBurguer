namespace DogtorBurguer
{
    /// <summary>Generates the flavor name shown when a burger completes, scaled by size.</summary>
    public static class BurgerNamer
    {
        // "Lil" without the apostrophe — the trial font renders ' as a placeholder sliver.
        private static readonly string[] SmallPrefixes = { "The", "Lil", "Mini", "Baby" };
        private static readonly string[] MediumPrefixes = { "Super", "Big", "Double", "Triple" };
        private static readonly string[] LargePrefixes = { "Mega", "Ultra", "Giga", "Hyper" };
        private static readonly string[] MegaPrefixes = { "ULTRA", "LEGENDARY", "EPIC", "GODLIKE" };

        private static readonly string[] Adjectives =
        {
            "Explosive", "Deluxe", "Supreme", "Wild", "Savage",
            "Brutal", "Infernal", "Cosmic", "Atomic", "Turbo",
            "Divine", "Furious", "Volcanic", "Radical", "Blazing"
        };

        private static readonly string[] Nouns =
        {
            "Tower", "Monster", "Beast", "Titan", "Colossus",
            "Skyscraper", "Tsunami", "Quake", "Volcano", "Hurricane",
            "Avalanche", "Tornado", "Meteor", "Dragon", "Kraken"
        };

        public static string Generate(int ingredientCount)
        {
            if (ingredientCount == 0)
                return "Just Bread...";
            if (ingredientCount >= 9)
                return "¡DOKTOR BURGUER!";

            string[] prefixes;
            if (ingredientCount <= 2) prefixes = SmallPrefixes;
            else if (ingredientCount <= 4) prefixes = MediumPrefixes;
            else if (ingredientCount <= 6) prefixes = LargePrefixes;
            else prefixes = MegaPrefixes;

            string prefix = prefixes[Rng.Range(0, prefixes.Length)];
            string adj = Adjectives[Rng.Range(0, Adjectives.Length)];
            string noun = Nouns[Rng.Range(0, Nouns.Length)];

            return $"{prefix} {noun} {adj}";
        }
    }
}
