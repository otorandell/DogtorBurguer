using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Skewer — always drops to the floor; if the column has a bottom bun it relocates one to row 0
    /// and destroys the rest (regulars collapse on top). Fizzles if there is no bun.
    /// </summary>
    public sealed class SkewerEffect : ConsumableEffect
    {
        public override ConsumableType Type => ConsumableType.Skewer;
        public override int ImpactRow(Column column) => 0;
        // The full skewer falls point-first; lifted so its tip (not its middle) meets the floor.
        public override Sprite FallerSprite => RewardArt.SkewerFalling;
        public override float FallerHeight => UIStyles.FX_SKEWER_FALLING_HEIGHT;
        public override float FallerImpactLift => UIStyles.FX_SKEWER_IMPACT_LIFT;

        public override bool CanApply(Column column)
        {
            if (column == null) return false;
            List<Ingredient> all = column.GetAllIngredients();
            for (int i = 0; i < all.Count; i++)
                if (all[i].Type == IngredientType.BunBottom) return true;
            return false;
        }

        public override void Apply(Column column) => GridManager.Instance?.ConsumableSkewer(column);
        public override void PlayVfx(Column column) => ConsumableVfx.SkewerPin(column);
    }
}
