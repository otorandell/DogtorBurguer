namespace DogtorBurguer
{
    /// <summary>
    /// What a fairy carries. Gems/stars award currency on collect; a consumable goes into the
    /// inventory. Each payload has its own full-body fairy sprite (see <see cref="RewardArt"/>);
    /// only the collect logic diverges.
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
        public static FairyPayload Stars() => new FairyPayload(FairyPayloadKind.Stars, default);
        public static FairyPayload Of(ConsumableType type) => new FairyPayload(FairyPayloadKind.Consumable, type);
    }
}
