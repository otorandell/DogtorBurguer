namespace DogtorBurguer
{
    /// <summary>
    /// Run ruleset, chosen in the MENU Settings (persisted; applies to the next run).
    /// Relax: identical speed/type curve, but every level threshold is stretched by
    /// GameplayConfig.RELAX_LENGTH_SCALE (runs ~3x longer), all star income is scaled by
    /// MonetizationConfig.RELAX_STAR_SCALE, and the high score is never written.
    /// </summary>
    public enum GameMode
    {
        Classic = 0,
        Relax = 1,
    }
}
