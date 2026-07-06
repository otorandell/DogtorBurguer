using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Central access point for all gameplay sprites. Loads every <see cref="Skin"/> from
    /// Resources/Skins and resolves the active skin per <see cref="SkinSlot"/>: the persisted
    /// equipped skin when one is set (see <see cref="Equip"/> / the Shop), otherwise the slot's
    /// default. Consumers read sprites at spawn/build time, so an equip applies to everything
    /// created afterwards — already-spawned sprites keep the art they were born with.
    /// </summary>
    public static class Theme
    {
        private const string SkinsResourcePath = "Skins";

        private static Dictionary<SkinSlot, Skin> _defaults;
        private static Dictionary<SkinSlot, Skin> _active;
        private static Dictionary<string, Skin> _byId;
        private static bool _persistedApplied;

        private static void EnsureLoaded()
        {
            if (_defaults == null)
            {
                _defaults = new Dictionary<SkinSlot, Skin>();
                _active = new Dictionary<SkinSlot, Skin>();
                _byId = new Dictionary<string, Skin>();

                Skin[] all = Resources.LoadAll<Skin>(SkinsResourcePath);
                foreach (Skin skin in all)
                {
                    // An explicit default wins; otherwise the first skin seen for a slot stands in.
                    if (skin.IsDefault || !_defaults.ContainsKey(skin.Slot))
                        _defaults[skin.Slot] = skin;
                    if (!string.IsNullOrEmpty(skin.Id))
                        _byId[skin.Id] = skin;
                }

                foreach (KeyValuePair<SkinSlot, Skin> entry in _defaults)
                    _active[entry.Key] = entry.Value;
            }

            // Persisted equips apply lazily: the first Theme access can beat SaveDataManager.Awake
            // (scene-object Awake order), so keep retrying until the save layer is alive.
            if (!_persistedApplied && SaveDataManager.Instance != null)
            {
                _persistedApplied = true;
                foreach (KeyValuePair<SkinSlot, Skin> entry in _defaults)
                {
                    string id = SaveDataManager.Instance.GetEquippedSkinId(entry.Key);
                    if (id.Length > 0 && _byId.TryGetValue(id, out Skin skin) && skin.Slot == entry.Key)
                        _active[entry.Key] = skin;
                }
            }
        }

        /// <summary>All authored skins, for the shop catalog. Do not mutate.</summary>
        public static IEnumerable<Skin> AllSkins()
        {
            EnsureLoaded();
            return _byId.Values;
        }

        /// <summary>
        /// Makes a skin the active one for its slot and persists the choice. Ownership is the
        /// caller's concern (see ShopService) — this only switches and saves.
        /// </summary>
        public static void Equip(Skin skin)
        {
            if (skin == null) return;
            EnsureLoaded();

            _active[skin.Slot] = skin;
            SaveDataManager.Instance?.SetEquippedSkinId(skin.Slot, skin.IsDefault ? "" : skin.Id);
        }

        public static bool IsEquipped(Skin skin) =>
            skin != null && Active(skin.Slot) == skin;

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

        /// <summary>The chef's flipped-facing sprite (secondary), shown after a plate flip.</summary>
        public static Sprite ChefFlipped
        {
            get
            {
                Skin skin = Active(SkinSlot.ChefSkin);
                return skin != null ? skin.SecondarySprite : null;
            }
        }

        public static Sprite Plate => Sprite(SkinSlot.PlateSkin);

        /// <summary>The diner-scene strip behind the top of the playfield (game background only).</summary>
        public static Sprite Restaurant => Sprite(SkinSlot.RestaurantSkin);

        /// <summary>The blue play-mat cells under the falling ingredients (game background only).</summary>
        public static Sprite GridCells => Sprite(SkinSlot.GridCellsSkin);

        public static Sprite Background(BackgroundType type) =>
            Sprite(type == BackgroundType.Menu ? SkinSlot.MenuBackgroundSkin : SkinSlot.GameBackgroundSkin);
    }
}
