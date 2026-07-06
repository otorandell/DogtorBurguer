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
        // The stick falls onto the BUN it will keep (not the floor); the head VFX takes over
        // there and rides the bun down (see ConsumableVfx.SkewerPin).
        public override int ImpactRow(Column column) => Mathf.Max(0, TopBottomBunRow(column));
        // Falls point-first; lifted so its tip meets the bun's top edge, not its middle.
        public override Sprite FallerSprite => RewardArt.SkewerFalling;
        public override float FallerHeight => UIStyles.FX_SKEWER_FALLING_HEIGHT;
        public override float FallerImpactLift => UIStyles.FX_SKEWER_IMPACT_LIFT;
        public override Sprite GhostSprite => RewardArt.SkewerTip;
        public override bool FallerVanishesOnImpact => true; // the head pin continues the motion

        public override bool CanApply(Column column) =>
            column != null && TopBottomBunRow(column) >= 0;

        /// <summary>Row of the topmost bottom bun (the one the skewer keeps and pins), or -1.
        /// Shared by the impact target, the applicability check, and the head VFX.</summary>
        public static int TopBottomBunRow(Column column)
        {
            if (column == null) return -1;
            List<Ingredient> all = column.GetAllIngredients();
            for (int i = all.Count - 1; i >= 0; i--)
                if (all[i].Type == IngredientType.BunBottom) return i;
            return -1;
        }

        public override void Apply(Column column) => GridManager.Instance?.ConsumableSkewer(column);
        public override void PlayVfx(Column column) => ConsumableVfx.SkewerPin(column);
    }
}
