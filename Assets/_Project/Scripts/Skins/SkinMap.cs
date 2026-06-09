using System;

namespace DogtorBurguer
{
    /// <summary>
    /// Explicit mapping from gameplay enums to <see cref="SkinSlot"/>. Kept as a deliberate
    /// hand-written switch (rather than coupling the enums by cast) so the two bun ingredient
    /// types can collapse onto the single <see cref="SkinSlot.BunSkin"/> slot.
    /// </summary>
    public static class SkinMap
    {
        public static SkinSlot SlotFor(IngredientType type) => type switch
        {
            IngredientType.Meat => SkinSlot.MeatSkin,
            IngredientType.Cheese => SkinSlot.CheeseSkin,
            IngredientType.Tomato => SkinSlot.TomatoSkin,
            IngredientType.Onion => SkinSlot.OnionSkin,
            IngredientType.Pickle => SkinSlot.PickleSkin,
            IngredientType.Lettuce => SkinSlot.LettuceSkin,
            IngredientType.Egg => SkinSlot.EggSkin,
            IngredientType.BunBottom => SkinSlot.BunSkin,
            IngredientType.BunTop => SkinSlot.BunSkin,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "No SkinSlot mapped for this IngredientType.")
        };
    }
}
