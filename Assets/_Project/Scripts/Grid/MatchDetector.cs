using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Static utility for match detection: finds and resolves the top-of-column match,
    /// leaving GridManager to fire the events. Burger detection is inline in GridManager —
    /// a burger only ever forms when a BunTop lands (F-31).
    /// </summary>
    public static class MatchDetector
    {
        /// <summary>
        /// Tries to find and process a single match at the top of the column.
        /// Removes and destroys matched ingredients. Call in a loop to process cascading matches.
        /// </summary>
        public static bool TryProcessMatch(Column column, out MatchResult result)
        {
            result = default;

            if (!column.CheckForMatch(out Ingredient top, out Ingredient second))
                return false;

            result.EffectPosition = (top.transform.position + second.transform.position) / 2f;
            result.IsBunMatch = top.Type == IngredientType.BunBottom;

            column.RemoveIngredient(top);
            column.RemoveIngredient(second);

            top.DestroyWithFlash();
            second.DestroyWithFlash();

            return true;
        }
    }
}
