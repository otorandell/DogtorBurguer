namespace DogtorBurguer
{
    /// <summary>How a <see cref="Skin"/> is obtained by the player.</summary>
    public enum UnlockMethod
    {
        Free,
        Gems,
        Iap,
        AdUnlock,
        // Appended (value 4). UnlockMethod ints are serialized in every Skin .asset (_unlock:),
        // so this enum is append-only — never reorder.
        Stars
    }
}
