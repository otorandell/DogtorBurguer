using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Static utility for match detection and burger pattern recognition.
    /// Handles the logic of finding matches and burgers in columns,
    /// while GridManager handles the event firing and orchestration.
    /// </summary>
    public static class MatchDetector
    {
        public struct MatchResult
        {
            public Vector3 EffectPosition;
            public bool IsBunMatch;
        }

        public struct BurgerDetection
        {
            public bool Found;
            public List<Ingredient> Parts;
            public int BunBottomIndex;
            public int BunTopIndex;
            public int IngredientCount;
            public List<IngredientType> IngredientTypes;
        }

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

        /// <summary>
        /// Checks if there is a BunBottom below the given ingredient in the column.
        /// </summary>
        public static bool HasBunBelow(Column column, Ingredient topBun)
        {
            var ingredients = column.GetAllIngredients();
            int topBunIndex = ingredients.IndexOf(topBun);

            for (int i = topBunIndex - 1; i >= 0; i--)
            {
                if (ingredients[i].Type == IngredientType.BunBottom)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Detects a complete burger in the column (BunTop + ingredients + BunBottom).
        /// Returns detection result without modifying the column.
        /// </summary>
        public static BurgerDetection DetectBurger(Column column)
        {
            var result = new BurgerDetection { Found = false };
            var ingredients = column.GetAllIngredients();

            if (ingredients.Count < 2) return result;

            // Search from top for BunTop
            int bunTopIndex = -1;
            for (int i = ingredients.Count - 1; i >= 0; i--)
            {
                if (ingredients[i].Type == IngredientType.BunTop)
                {
                    bunTopIndex = i;
                    break;
                }
            }

            if (bunTopIndex < 0) return result;

            // Search downward for BunBottom
            int bunBottomIndex = -1;
            for (int i = bunTopIndex - 1; i >= 0; i--)
            {
                if (ingredients[i].Type == IngredientType.BunBottom)
                {
                    bunBottomIndex = i;
                    break;
                }
                if (ingredients[i].Type == IngredientType.BunTop)
                    break;
            }

            if (bunBottomIndex < 0) return result;

            // Collect burger parts (top to bottom)
            var parts = new List<Ingredient>();
            for (int i = bunTopIndex; i >= bunBottomIndex; i--)
                parts.Add(ingredients[i]);

            // Collect ingredient types (excluding buns)
            var ingredientTypes = new List<IngredientType>();
            for (int i = bunBottomIndex + 1; i < bunTopIndex; i++)
                ingredientTypes.Add(ingredients[i].Type);

            result.Found = true;
            result.Parts = parts;
            result.BunBottomIndex = bunBottomIndex;
            result.BunTopIndex = bunTopIndex;
            result.IngredientCount = bunTopIndex - bunBottomIndex - 1;
            result.IngredientTypes = ingredientTypes;

            return result;
        }
    }
}
