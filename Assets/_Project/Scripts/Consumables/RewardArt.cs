using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Loads and caches the consumable-system sprites from Resources (Fairy/ + Rewards/ +
    /// Effects/), the same load-by-convention approach used for Music/Skins. One reward badge per
    /// payload doubles as the inventory icon, the column ghost (alpha-tinted by the consumer),
    /// and the faller; the Effects/ sprites are the use-effect art (see ConsumableVfx).
    /// </summary>
    public static class RewardArt
    {
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        public static Sprite Badge(ConsumableType type) => Load("Rewards/" + type.ToString().ToLowerInvariant());

        public static Sprite KetchupNozzle => Load("Effects/fx_ketchup_nozzle");
        public static Sprite KetchupStream => Load("Effects/fx_ketchup_stream");
        public static Sprite MustardNozzle => Load("Effects/fx_mustard_nozzle");
        public static Sprite MustardDrop => Load("Effects/fx_mustard_drop");
        public static Sprite SkewerFalling => Load("Effects/fx_skewer_falling");
        public static Sprite SkewerHead => Load("Effects/fx_skewer_head");
        public static Sprite SkewerTip => Load("Effects/fx_skewer_tip");

        /// <summary>The fairy itself — one empty-handed (blue) body for every payload since
        /// 2026-09-26; the cargo is a separate sprite in its hand (<see cref="FairyCargo"/>).</summary>
        public static Sprite FairyBody => Load("Fairy/fairy_body");

        /// <summary>What a fairy carries: the top-bar gem/star icons for currency, the in-game
        /// inventory slot icon for a consumable (the same art the player grabs).</summary>
        public static Sprite FairyCargo(FairyPayload payload)
        {
            switch (payload.Kind)
            {
                case FairyPayloadKind.Gems: return Load("UI/ui_gem");
                case FairyPayloadKind.Stars: return Load("UI/ui_star");
                default: return Load("UI/ui_consumable_" + payload.Consumable.ToString().ToLowerInvariant());
            }
        }

        // The VISIBLE art inside each cargo sprite (alpha bbox, measured 2026-09-26): x = visible
        // height / sprite height, (y, z) = visible centre offset from the sprite centre, as a
        // fraction of the sprite height (right, up). The slot icons carry uneven transparent
        // margins (skewer 85% tall, mustard 97%), so cargo is sized and centred on what shows.
        private static readonly Dictionary<string, Vector3> CargoArt = new()
        {
            ["ui_gem"] = new Vector3(0.967f, -0.012f, 0.014f),
            ["ui_star"] = new Vector3(0.975f, -0.010f, 0.010f),
            ["ui_consumable_ketchup"] = new Vector3(0.904f, 0.007f, -0.025f),
            ["ui_consumable_mustard"] = new Vector3(0.970f, 0.020f, -0.003f),
            ["ui_consumable_skewer"] = new Vector3(0.853f, 0.029f, 0.010f),
        };

        /// <summary>Visible-art metrics for a cargo sprite (whole sprite if unmeasured).</summary>
        public static Vector3 CargoVisibleArt(Sprite sprite) =>
            sprite != null && CargoArt.TryGetValue(sprite.name, out Vector3 v) ? v : new Vector3(1f, 0f, 0f);

        private static Sprite Load(string path)
        {
            if (_cache.TryGetValue(path, out Sprite cached) && cached != null)
                return cached;

            Sprite sprite = Resources.Load<Sprite>(path);
            if (sprite == null)
                Debug.LogError($"[RewardArt] Missing sprite at Resources/{path}");
            _cache[path] = sprite;
            return sprite;
        }
    }
}
