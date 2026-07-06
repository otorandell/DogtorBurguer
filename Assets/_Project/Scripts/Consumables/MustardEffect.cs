using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Mustard — reads the type of the targeted column's top ingredient, then removes all of that
    /// type across the whole board.
    /// </summary>
    public sealed class MustardEffect : ConsumableEffect
    {
        public override ConsumableType Type => ConsumableType.Mustard;
        public override int ImpactRow(Column column) => column.StackHeight;
        public override bool CanApply(Column column) => column != null && !column.IsEmpty;
        public override Sprite FallerSprite => RewardArt.MustardDrop;
        public override float FallerHeight => UIStyles.FX_MUSTARD_DROP_HEIGHT;
        public override Sprite GhostSprite => RewardArt.MustardNozzle;
        public override bool GhostLingers => true; // the ghost nozzle "drops" the falling blob

        public override void Apply(Column column)
        {
            Ingredient top = column.GetTopIngredient();
            if (top == null) return;
            GridManager.Instance?.ConsumableSweepType(top.Type, column);
        }
    }
}
