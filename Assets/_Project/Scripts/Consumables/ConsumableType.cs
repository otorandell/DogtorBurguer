namespace DogtorBurguer
{
    /// <summary>
    /// The three consumables a Burger Fairy can carry. Int order is load-bearing: it indexes
    /// GameplayConfig.CONSUMABLE_SPAWN_WEIGHTS, and sprites are loaded as the lowercased name
    /// (Resources/Rewards/{name} for the fairy/ghost/faller badge, Resources/UI/ui_consumable_{name}
    /// for the inventory slot icon).
    /// </summary>
    public enum ConsumableType
    {
        Ketchup = 0,
        Mustard = 1,
        Skewer = 2
    }
}
