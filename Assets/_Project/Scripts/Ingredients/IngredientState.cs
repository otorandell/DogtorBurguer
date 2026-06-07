namespace DogtorBurguer
{
    /// <summary>Lifecycle of a spawned ingredient: awaiting fall start, falling, or landed.</summary>
    public enum IngredientState
    {
        Spawned,
        Falling,
        Landed
    }
}
