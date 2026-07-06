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

        public override void Apply(Column column)
        {
            Ingredient top = column.GetTopIngredient();
            if (top == null) return;
            GridManager.Instance?.ConsumableSweepType(top.Type, column);
        }

        public override void PlayVfx(Column column) => ConsumableVfx.MustardSweep(column);
    }
}
