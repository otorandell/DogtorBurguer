using System.Collections.Generic;

namespace DogtorBurguer
{
    /// <summary>
    /// A detected, ready-to-resolve burger: the parts, their column span, and the
    /// pre-computed points/name. Passed from GridManager to BurgerAnimator.
    /// </summary>
    public struct BurgerData
    {
        public Column Column;
        public List<Ingredient> Parts;
        public int BunBottomIndex;
        public int BunTopIndex;
        public int IngredientCount;
        public List<IngredientType> IngredientTypes;
        public int Points;
        public string Name;
    }
}
