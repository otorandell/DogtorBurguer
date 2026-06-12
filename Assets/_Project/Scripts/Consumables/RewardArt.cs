using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Loads and caches the consumable-system sprites from Resources (Fairy/ + Rewards/), the same
    /// load-by-convention approach used for Music/Skins. One reward badge per payload doubles as the
    /// inventory icon, the column ghost (alpha-tinted by the consumer), and the faller.
    /// </summary>
    public static class RewardArt
    {
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        public static Sprite Fairy => Load("Fairy/fairy");
        public static Sprite Gem => Load("Rewards/gem");
        public static Sprite Badge(ConsumableType type) => Load("Rewards/" + type.ToString().ToLowerInvariant());

        /// <summary>Badge sprite for a payload — the gem or the consumable icon.</summary>
        public static Sprite Badge(FairyPayload payload) =>
            payload.Kind == FairyPayloadKind.Gems ? Gem : Badge(payload.Consumable);

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
