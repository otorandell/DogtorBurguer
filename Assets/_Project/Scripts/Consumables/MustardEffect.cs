using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Mustard — reads the targeted column's top MUSTARD_SWEEP_TYPES distinct regular types
    /// (top down, buns skipped), then removes every piece of those types across the whole board.
    /// Its score escalates with the sweep size (Scoring.MustardSweepPoints).
    /// </summary>
    public sealed class MustardEffect : ConsumableEffect
    {
        public override ConsumableType Type => ConsumableType.Mustard;
        public override int ImpactRow(Column column) => column.StackHeight;
        public override bool CanApply(Column column) => SweepTypes(column).Count > 0;
        public override Sprite FallerSprite => RewardArt.MustardDrop;
        public override float FallerHeight => UIStyles.FX_MUSTARD_DROP_HEIGHT;
        public override Sprite GhostSprite => RewardArt.MustardNozzle;
        public override bool GhostLingers => true; // the ghost nozzle "drops" the falling blob

        /// <summary>The distinct regular types the sweep targets, read from the column top down
        /// (buns skipped). Empty when the column holds no regular ingredient → the drop fizzles.
        /// Shared by the applicability check and the impact.</summary>
        public static List<IngredientType> SweepTypes(Column column)
        {
            List<IngredientType> types = new List<IngredientType>(GameplayConfig.MUSTARD_SWEEP_TYPES);
            if (column == null) return types;

            List<Ingredient> all = column.GetAllIngredients();
            for (int i = all.Count - 1; i >= 0 && types.Count < GameplayConfig.MUSTARD_SWEEP_TYPES; i--)
            {
                IngredientType type = all[i].Type;
                if (type == IngredientType.BunBottom || type == IngredientType.BunTop) continue;
                if (!types.Contains(type)) types.Add(type);
            }
            return types;
        }

        public override void Apply(Column column)
        {
            List<IngredientType> types = SweepTypes(column);
            if (types.Count == 0) return;
            GridManager.Instance?.ConsumableSweepTypes(types, column);
        }
    }
}
