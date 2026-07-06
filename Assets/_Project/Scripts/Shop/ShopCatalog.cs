using System.Collections.Generic;

namespace DogtorBurguer
{
    /// <summary>
    /// Shop-facing view of the skin catalog. Skins are authored assets (Resources/Skins) served
    /// by <see cref="Theme"/>; this groups them into the shop's sections. A slot only appears
    /// once it has at least one purchasable (non-default) skin — a lone "equipped" default cell
    /// is noise, not a shop. Adding a skin to the shop = dropping a new Skin asset, no code.
    /// </summary>
    public static class ShopCatalog
    {
        private static readonly SkinSlot[] ChefSlots = { SkinSlot.ChefSkin };

        private static readonly SkinSlot[] IngredientSlots =
        {
            SkinSlot.MeatSkin, SkinSlot.CheeseSkin, SkinSlot.TomatoSkin, SkinSlot.OnionSkin,
            SkinSlot.PickleSkin, SkinSlot.LettuceSkin, SkinSlot.EggSkin, SkinSlot.BaconSkin,
            SkinSlot.BunSkin,
        };

        public static List<Skin> ChefSkins() => SkinsFor(ChefSlots);
        public static List<Skin> IngredientSkins() => SkinsFor(IngredientSlots);

        private static List<Skin> SkinsFor(SkinSlot[] slots)
        {
            List<Skin> result = new();
            foreach (SkinSlot slot in slots)
            {
                List<Skin> ofSlot = new();
                foreach (Skin skin in Theme.AllSkins())
                    if (skin.Slot == slot)
                        ofSlot.Add(skin);

                if (ofSlot.Count < 2) continue; // default only — nothing to sell for this slot

                // Default first (it's the "classic" cell), then cheapest to priciest.
                ofSlot.Sort((a, b) => a.IsDefault != b.IsDefault
                    ? (a.IsDefault ? -1 : 1)
                    : a.StarCost.CompareTo(b.StarCost));
                result.AddRange(ofSlot);
            }
            return result;
        }
    }
}
