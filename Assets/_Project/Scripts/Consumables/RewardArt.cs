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

        /// <summary>The full-body fairy illustration for a payload (each carries its cargo in-art).</summary>
        public static Sprite Fairy(FairyPayload payload)
        {
            switch (payload.Kind)
            {
                case FairyPayloadKind.Gems: return Load("Fairy/fairy_gems");
                case FairyPayloadKind.Stars: return Load("Fairy/fairy_stars");
                default: return Load("Fairy/fairy_" + payload.Consumable.ToString().ToLowerInvariant());
            }
        }

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
