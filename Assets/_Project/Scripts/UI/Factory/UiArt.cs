using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Loads and caches authored UI sprites from Resources/UI by name. UI chrome lives outside the
    /// Theme/Skins pipeline (it isn't per-element re-skinnable), so it's loaded directly here.
    /// </summary>
    public static class UiArt
    {
        private const string UiResourcePath = "UI/";
        private static readonly Dictionary<string, Sprite> _cache = new();

        public static Sprite Load(string name)
        {
            if (_cache.TryGetValue(name, out Sprite cached))
                return cached;

            Sprite sprite = Resources.Load<Sprite>(UiResourcePath + name);
            _cache[name] = sprite;
            return sprite;
        }
    }
}
