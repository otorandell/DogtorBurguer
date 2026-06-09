namespace DogtorBurguer
{
    /// <summary>
    /// Sub-state of the wave spawner while it is active (gated by IngredientSpawner._active).
    /// </summary>
    public enum SpawnerState
    {
        Delaying,      // counting down before spawning the first wave
        WaveFalling    // a wave is falling; the preview queue refills continuously until it lands
    }
}
