using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Scales a SpriteRenderer's transform so the sprite renders at a target world height,
    /// independent of the source texture's pixel size / PPU. Used for all consumable-system art
    /// (reward sprites import large), keeping sizing predictable from one UIStyles constant.
    /// </summary>
    public static class SpriteFit
    {
        public static void Height(SpriteRenderer renderer, float worldHeight)
        {
            if (renderer == null || renderer.sprite == null) return;

            float spriteHeight = renderer.sprite.bounds.size.y;
            if (spriteHeight <= 0.0001f) return;

            float scale = worldHeight / spriteHeight;
            renderer.transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
