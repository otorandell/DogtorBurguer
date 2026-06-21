namespace DogtorBurguer
{
    public enum IngredientType
    {
        // Regular ingredients. NOTE: these int values are NOT load-bearing for spawn
        // progression — that order is defined by GameplayConfig.REGULAR_INGREDIENTS.
        // Values are kept stable (append-only) so existing serialized references survive.
        Meat = 0,
        Cheese = 1,
        Tomato = 2,
        Onion = 3,
        Pickle = 4,
        Lettuce = 5,
        Egg = 6,        // Special - appears in advanced phases
        Bacon = 7,

        // Buns
        BunBottom = 10,
        BunTop = 11
    }
}
