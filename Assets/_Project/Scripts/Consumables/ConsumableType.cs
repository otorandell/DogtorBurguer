namespace DogtorBurguer
{
    /// <summary>
    /// The three consumables a Burger Fairy can carry. Int order is load-bearing: it indexes
    /// GameplayConfig.CONSUMABLE_SPAWN_WEIGHTS, and the sprite is loaded as the lowercased name
    /// (Resources/Rewards/{name}).
    /// </summary>
    public enum ConsumableType
    {
        Ketchup = 0,
        Mustard = 1,
        Skewer = 2
    }
}
