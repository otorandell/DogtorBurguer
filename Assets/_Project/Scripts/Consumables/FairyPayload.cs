namespace DogtorBurguer
{
    /// <summary>
    /// What a fairy carries. Gems award currency on collect; a consumable goes into the per-run
    /// inventory. The badge sprite is uniform either way (see <see cref="RewardArt"/>); only the
    /// collect logic diverges.
    /// </summary>
    public readonly struct FairyPayload
    {
        public readonly FairyPayloadKind Kind;
        public readonly ConsumableType Consumable; // meaningful only when Kind == Consumable

        private FairyPayload(FairyPayloadKind kind, ConsumableType consumable)
        {
            Kind = kind;
            Consumable = consumable;
        }

        public static FairyPayload Gems() => new FairyPayload(FairyPayloadKind.Gems, default);
        public static FairyPayload Of(ConsumableType type) => new FairyPayload(FairyPayloadKind.Consumable, type);
    }
}
