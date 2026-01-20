namespace DogtorBurguer
{
    public enum IngredientType
    {
        // Regular ingredients (order matters for spawn progression)
        Meat = 0,
        Cheese = 1,
        Tomato = 2,
        Onion = 3,
        Pickle = 4,
        Lettuce = 5,
        Egg = 6,        // Special - appears in advanced phases

        // Buns
        BunBottom = 10,
        BunTop = 11
    }

    public static class IngredientTypeExtensions
    {
        public static bool IsBun(this IngredientType type)
        {
            return type == IngredientType.BunBottom || type == IngredientType.BunTop;
        }

        public static bool IsRegularIngredient(this IngredientType type)
        {
            return (int)type >= 0 && (int)type <= 6;
        }
    }
}
