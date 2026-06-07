namespace DogtorBurguer
{
    /// <summary>
    /// Sub-state of the wave spawner while it is active (gated by IngredientSpawner._active).
    /// </summary>
    public enum SpawnerState
    {
        Delaying,        // counting down before spawning the next wave
        WaveFalling,     // current wave is falling; waiting for it to clear the top cell
        WaitingForLand   // previews shown; waiting for the current wave to finish landing
    }
}
