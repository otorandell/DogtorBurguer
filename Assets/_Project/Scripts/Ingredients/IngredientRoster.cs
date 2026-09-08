using System.Collections.Generic;

namespace DogtorBurguer
{
    /// <summary>
    /// The per-run ingredient unlock order (2026-09-08): Meat, Cheese and Bacon always start;
    /// every other type joins in a RANDOM order as INGREDIENT_COUNT_BY_LEVEL grows — with skins
    /// equippable, a fixed roster made every run read identical. Index i = the type that is
    /// active once the count exceeds i (so index 3 is the run's random fourth starter).
    /// Built once per run by IngredientSpawner; the bag and the Special Orders read through it.
    /// </summary>
    public class IngredientRoster
    {
        private static readonly IngredientType[] Starters =
        {
            IngredientType.Meat, IngredientType.Cheese, IngredientType.Bacon,
        };

        private readonly List<IngredientType> _order = new();

        public IngredientRoster()
        {
            _order.AddRange(Starters);

            List<IngredientType> rest = new();
            foreach (IngredientType type in GameplayConfig.REGULAR_INGREDIENTS)
                if (!_order.Contains(type)) rest.Add(type);

            while (rest.Count > 0)
            {
                int i = Rng.Range(0, rest.Count);
                _order.Add(rest[i]);
                rest.RemoveAt(i);
            }
        }

        /// <summary>The type at unlock position <paramref name="index"/> (0-based).</summary>
        public IngredientType At(int index) => _order[index];
    }
}
