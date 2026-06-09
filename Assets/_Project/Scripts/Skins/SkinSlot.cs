namespace DogtorBurguer
{
    /// <summary>
    /// Every cosmetically-skinnable graphic slot. One <see cref="Skin"/> targets exactly one slot.
    /// The <see cref="BunSkin"/> slot carries two sprites (top + bottom); all other slots carry one.
    /// Names are suffixed with "Skin" so they never read ambiguously against <see cref="IngredientType"/>.
    /// </summary>
    public enum SkinSlot
    {
        MeatSkin,
        CheeseSkin,
        TomatoSkin,
        OnionSkin,
        PickleSkin,
        LettuceSkin,
        EggSkin,
        BunSkin,
        ChefSkin,
        GameBackgroundSkin,
        MenuBackgroundSkin
    }
}
