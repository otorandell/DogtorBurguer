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

        // Ordered ingredient slots, each shown as its own labelled shop row. Bun top+bottom collapse
        // onto the single BunSkin slot, so buns are one row / one purchasable (both sprites per skin).
        // Row order (Oscar, 2026-09-05): buns first, then the level-1 ingredients (the first
        // INGREDIENT_COUNT_BY_LEVEL[0] entries of GameplayConfig.REGULAR_INGREDIENTS), then the
        // rest in order of appearance — keep in step with that array.
        private static readonly SkinSlot[] IngredientSlots =
        {
            SkinSlot.BunSkin,
            SkinSlot.MeatSkin, SkinSlot.CheeseSkin, SkinSlot.TomatoSkin, SkinSlot.BaconSkin,
            SkinSlot.OnionSkin, SkinSlot.PickleSkin, SkinSlot.LettuceSkin, SkinSlot.EggSkin,
        };

        public static List<Skin> ChefSkins() => SkinsFor(ChefSlots);

        /// <summary>Ingredient skins grouped into one labelled row per slot (in <see cref="IngredientSlots"/>
        /// order). A slot appears only once it owns a purchasable skin beyond its classic default.</summary>
        public static List<(string Label, List<Skin> Skins)> IngredientSkinRows()
        {
            List<(string, List<Skin>)> rows = new();
            foreach (SkinSlot slot in IngredientSlots)
            {
                List<Skin> ofSlot = SkinsForSlot(slot);
                if (ofSlot.Count < 2) continue; // default only — nothing to sell for this slot
                rows.Add((SlotLabel(slot), ofSlot));
            }
            return rows;
        }

        /// <summary>Row subtitle for an ingredient slot (BunSkin reads "Buns" — top+bottom together).</summary>
        public static string SlotLabel(SkinSlot slot) => slot switch
        {
            SkinSlot.MeatSkin => "Patty",
            SkinSlot.CheeseSkin => "Cheese",
            SkinSlot.TomatoSkin => "Tomato",
            SkinSlot.OnionSkin => "Onion",
            SkinSlot.PickleSkin => "Pickles",
            SkinSlot.LettuceSkin => "Lettuce",
            SkinSlot.EggSkin => "Egg",
            SkinSlot.BaconSkin => "Bacon",
            SkinSlot.BunSkin => "Buns",
            _ => slot.ToString()
        };

        // All skins for one slot. Order (Oscar, 2026-09-09): the classic default anchors the
        // row, then the cheap star skin, then gem skins cheapest-first, and the slot's ULTIMATE
        // skin — its priciest star skin (the golden set, the Royale king) — closes the row as
        // the thing to scroll toward. (The original cheap-to-pricey sort keyed on StarCost
        // alone, floating gem skins, star cost 0, ahead of every star skin.)
        private static List<Skin> SkinsForSlot(SkinSlot slot)
        {
            List<Skin> ofSlot = new();
            foreach (Skin skin in Theme.AllSkins())
                if (skin.Slot == slot)
                    ofSlot.Add(skin);

            int maxStar = 0;
            foreach (Skin skin in ofSlot)
                if (skin.Unlock == UnlockMethod.Stars && skin.StarCost > maxStar)
                    maxStar = skin.StarCost;

            int Rank(Skin s) => s.IsDefault ? 0
                : s.Unlock == UnlockMethod.Stars && s.StarCost == maxStar ? 3
                : s.Unlock == UnlockMethod.Stars ? 1 : 2;

            ofSlot.Sort((a, b) => Rank(a) != Rank(b)
                ? Rank(a).CompareTo(Rank(b))
                : a.Unlock == UnlockMethod.Stars
                    ? a.StarCost.CompareTo(b.StarCost)
                    : a.GemCost.CompareTo(b.GemCost));
            return ofSlot;
        }

        private static List<Skin> SkinsFor(SkinSlot[] slots)
        {
            List<Skin> result = new();
            foreach (SkinSlot slot in slots)
            {
                List<Skin> ofSlot = SkinsForSlot(slot);
                if (ofSlot.Count < 2) continue; // default only — nothing to sell for this slot
                result.AddRange(ofSlot);
            }
            return result;
        }
    }
}
