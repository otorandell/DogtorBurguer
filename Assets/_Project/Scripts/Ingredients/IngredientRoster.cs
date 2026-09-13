using System.Collections.Generic;
using UnityEngine;

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

        /// <summary>Rebuilds a roster from a saved order (resuming a run). Falls back to a fresh
        /// random roster if the saved one doesn't cover every regular ingredient — a short order
        /// would throw the moment a level unlocked a position it lacks.</summary>
        public IngredientRoster(IngredientType[] savedOrder)
        {
            if (savedOrder != null && savedOrder.Length == GameplayConfig.REGULAR_INGREDIENTS.Length)
            {
                _order.AddRange(savedOrder);
                return;
            }

            Debug.LogWarning("[IngredientRoster] Saved order unusable — rolling a fresh one.");
            Randomize();
        }

        public IngredientRoster()
        {
            Randomize();
        }

        /// <summary>This run's order, for the resume snapshot.</summary>
        public IngredientType[] ToArray() => _order.ToArray();

        private void Randomize()
        {
            _order.Clear();
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
