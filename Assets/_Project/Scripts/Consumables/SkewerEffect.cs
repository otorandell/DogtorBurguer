using System.Collections.Generic;

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

        public override bool CanApply(Column column)
        {
            if (column == null) return false;
            List<Ingredient> all = column.GetAllIngredients();
            for (int i = 0; i < all.Count; i++)
                if (all[i].Type == IngredientType.BunBottom) return true;
            return false;
        }

        public override void Apply(Column column) => GridManager.Instance?.ConsumableSkewer(column);
    }
}
