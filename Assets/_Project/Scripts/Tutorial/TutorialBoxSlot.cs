namespace DogtorBurguer
{
    /// <summary>Where the tutorial callout sits (2026-09-26): always in the HUD band ABOVE the
    /// board, never over it. <see cref="Top"/> spans the band; <see cref="Left"/> / <see cref="Right"/>
    /// take one half so the HUD element the step is about (the Special Order card on the right,
    /// the consumable row on the left) stays visible.</summary>
    public enum TutorialBoxSlot
    {
        Top,
        Left,
        Right,
    }
}
