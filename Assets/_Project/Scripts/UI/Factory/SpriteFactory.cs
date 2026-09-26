using System;
using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Procedural sprite generation with caching. Each distinct sprite is built once
    /// and reused for the app's lifetime (the "cache generated assets" rule), so scene
    /// reloads no longer leak a fresh Texture2D/Sprite per object. Callers tint via
    /// SpriteRenderer.color and size via localScale / 9-slice, so the cached sprites
    /// stay shareable.
    ///
    /// Sibling of UIFactory (screen-space UGUI) and WorldTextFactory (world-space TMP).
    /// </summary>
    public static class SpriteFactory
    {
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        /// <summary>
        /// A 1×1 white sprite (PPU 1 → 1 world unit). Tint with SpriteRenderer.color and
        /// scale with localScale. Reused for solid fills, filters, and meters.
        /// </summary>
        public static Sprite White()
        {
            return GetOrCreate("white", () =>
            {
                Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            });
        }

        /// <summary>Vertical gradient (bottom → top), 2×256 @ PPU 100. Cached per color pair.</summary>
        public static Sprite VerticalGradient(Color bottom, Color top)
        {
            return GetOrCreate($"grad:{bottom}:{top}", () =>
            {
                const int width = 2;
                const int height = 256;
                Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
                for (int y = 0; y < height; y++)
                {
                    Color c = Color.Lerp(bottom, top, (float)y / (height - 1));
                    for (int x = 0; x < width; x++)
                        tex.SetPixel(x, y, c);
                }
                tex.Apply();
                return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
            });
        }

        /// <summary>Corner radius, in texture px, of <see cref="RoundedRect"/>. A UGUI Image shows it
        /// at any radius R via <c>pixelsPerUnitMultiplier = ROUNDED_RECT_RADIUS / R</c>.</summary>
        public const int ROUNDED_RECT_RADIUS = 32;

        /// <summary>A white, anti-aliased rounded rectangle with 9-slice borders on the corners —
        /// set it on an <c>Image.Type.Sliced</c> Image, tint with Image.color. PPU 100.</summary>
        public static Sprite RoundedRect()
        {
            return GetOrCreate("roundrect", () =>
            {
                const int r = ROUNDED_RECT_RADIUS;
                const int size = r * 2 + 2; // a 2px flat middle to stretch
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        // Distance past the nearest corner circle's centre (0 along the flat edges).
                        float dx = Mathf.Max(r - (x + 0.5f), (x + 0.5f) - (size - r), 0f);
                        float dy = Mathf.Max(r - (y + 0.5f), (y + 0.5f) - (size - r), 0f);
                        float alpha = Mathf.Clamp01(r - Mathf.Sqrt(dx * dx + dy * dy) + 0.5f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                }
                tex.Apply();
                return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f,
                    0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
            });
        }

        private static Sprite GetOrCreate(string key, Func<Sprite> create)
        {
            // Unity's overloaded == treats a destroyed sprite as null — regenerate if so.
            if (_cache.TryGetValue(key, out Sprite sprite) && sprite != null)
                return sprite;

            sprite = create();
            _cache[key] = sprite;
            return sprite;
        }
    }
}
