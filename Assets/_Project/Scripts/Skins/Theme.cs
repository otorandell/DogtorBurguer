using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Central access point for all gameplay sprites. Loads every <see cref="Skin"/> from
    /// Resources/Skins and resolves the active skin per <see cref="SkinSlot"/>. Phase 1 always
    /// serves the default skin for each slot; per-slot selection and persistence arrive later,
    /// at which point <see cref="_active"/> diverges from <see cref="_defaults"/>.
    /// </summary>
    public static class Theme
    {
        private const string SkinsResourcePath = "Skins";

        private static Dictionary<SkinSlot, Skin> _defaults;
        private static Dictionary<SkinSlot, Skin> _active;

        private static void EnsureLoaded()
        {
            if (_defaults != null) return;

            _defaults = new Dictionary<SkinSlot, Skin>();
            _active = new Dictionary<SkinSlot, Skin>();

            Skin[] all = Resources.LoadAll<Skin>(SkinsResourcePath);
            foreach (Skin skin in all)
            {
                // An explicit default wins; otherwise the first skin seen for a slot stands in.
                if (skin.IsDefault || !_defaults.ContainsKey(skin.Slot))
                    _defaults[skin.Slot] = skin;
            }

            foreach (KeyValuePair<SkinSlot, Skin> entry in _defaults)
                _active[entry.Key] = entry.Value;
        }

        /// <summary>The skin currently active for a slot, or null if none is authored.</summary>
        public static Skin Active(SkinSlot slot)
        {
            EnsureLoaded();
            return _active.TryGetValue(slot, out Skin skin) ? skin : null;
        }

        /// <summary>Primary sprite for a slot. Use <see cref="Ingredient"/> for bun-aware lookups.</summary>
        public static Sprite Sprite(SkinSlot slot)
        {
            Skin skin = Active(slot);
            return skin != null ? skin.Sprite : null;
        }

        /// <summary>Sprite for an ingredient, routing the two bun types to the single bun skin.</summary>
        public static Sprite Ingredient(IngredientType type)
        {
            Skin skin = Active(SkinMap.SlotFor(type));
            if (skin == null) return null;
            return type == IngredientType.BunBottom ? skin.SecondarySprite : skin.Sprite;
        }

        public static Sprite Chef => Sprite(SkinSlot.ChefSkin);

        public static Sprite Background(BackgroundType type) =>
            Sprite(type == BackgroundType.Menu ? SkinSlot.MenuBackgroundSkin : SkinSlot.GameBackgroundSkin);
    }
}
