using System.Collections.Generic;

namespace DogtorBurguer
{
    /// <summary>
    /// Shuffle-bag distributor for regular (non-bun) ingredients: even spread with no
    /// droughts or streaks. Each bag holds one of every active type plus
    /// <see cref="GameplayConfig.BAG_RANDOM_EXTRAS"/> random extras, shuffled; pieces are
    /// drawn without replacement and the bag refills (rebuilt from the current active count)
    /// when it empties or when the active-type count changes. No weights, no grid awareness.
    /// </summary>
    public class IngredientBag
    {
        private readonly List<IngredientType> _bag = new();
        private int _builtForCount = -1;

        /// <summary>Draws the next regular ingredient, refilling/rebuilding as needed. The
        /// roster maps unlock positions to this run's random types (2026-09-08).</summary>
        public IngredientType Next(IngredientRoster roster, int activeIngredientCount)
        {
            if (_bag.Count == 0 || _builtForCount != activeIngredientCount)
                Refill(roster, activeIngredientCount);

            // Draw from the tail so removal is O(1); the bag is already shuffled.
            int last = _bag.Count - 1;
            IngredientType type = _bag[last];
            _bag.RemoveAt(last);
            return type;
        }

        private void Refill(IngredientRoster roster, int activeIngredientCount)
        {
            _bag.Clear();
            _builtForCount = activeIngredientCount;

            // One of each active type guarantees no type droughts...
            for (int i = 0; i < activeIngredientCount; i++)
                _bag.Add(roster.At(i));

            // ...the random extras inject controlled, non-countable variance.
            for (int i = 0; i < GameplayConfig.BAG_RANDOM_EXTRAS; i++)
                _bag.Add(roster.At(Rng.Range(0, activeIngredientCount)));

            Shuffle(_bag);
        }

        private static void Shuffle(List<IngredientType> list)
        {
            // Fisher-Yates via the shared Rng (never UnityEngine.Random).
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Rng.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
