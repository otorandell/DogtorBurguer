using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Central scoring math (mirrors the Rng pattern: one static utility per cross-cutting
    /// concern). The single source of truth for burger tier boundaries and point values.
    /// </summary>
    public static class Scoring
    {
        /// <summary>Tier of a burger by its ingredient count. The one place the boundaries live.</summary>
        public static BurgerTier GetBurgerTier(int ingredientCount)
        {
            if (ingredientCount == 0) return BurgerTier.Poor;
            if (ingredientCount <= 2) return BurgerTier.Small;
            if (ingredientCount <= 4) return BurgerTier.Medium;
            if (ingredientCount <= 6) return BurgerTier.Large;
            if (ingredientCount <= 8) return BurgerTier.Mega;
            return BurgerTier.Max;
        }

        /// <summary>Points awarded for a completed burger: per-ingredient base + a tier bonus.</summary>
        public static int CalculateBurgerPoints(int ingredientCount)
        {
            int basePoints = ingredientCount * GameplayConfig.POINTS_PER_INGREDIENT;
            int bonus = GetBurgerTier(ingredientCount) switch
            {
                BurgerTier.Poor => GameplayConfig.BONUS_POOR_BURGER,
                BurgerTier.Small => GameplayConfig.BONUS_SMALL_BURGER,
                BurgerTier.Medium => GameplayConfig.BONUS_MEDIUM_BURGER,
                BurgerTier.Large => GameplayConfig.BONUS_LARGE_BURGER,
                BurgerTier.Mega => GameplayConfig.BONUS_MEGA_BURGER,
                _ => GameplayConfig.BONUS_MAX_BURGER,
            };
            return basePoints + bonus;
        }

        /// <summary>Fast-drop bonus: points scale with the distance the player skipped.</summary>
        public static int FastDropPoints(float distanceToLand)
        {
            return Mathf.RoundToInt(distanceToLand * GameplayConfig.FAST_DROP_POINTS_PER_UNIT);
        }
    }
}
