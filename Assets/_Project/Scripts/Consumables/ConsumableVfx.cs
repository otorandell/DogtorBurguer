using DG.Tweening;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Authored "in use" effects, purely cosmetic and fire-and-forget, self-destroyed when the
    /// tween ends. The lingering column GHOST plays the locked-on nozzle (see
    /// ConsumableEffect.GhostLingers) — ketchup squirts a stream from it down the column, its
    /// front sweeping rows in step with the staggered clear flashes; mustard's authored drop is
    /// its faller. The skewer leaves only its head pinned at the base — the stick "went through".
    /// Sizes in UIStyles (FX_*), timings in AnimConfig (FX_*).
    /// </summary>
    public static class ConsumableVfx
    {
        /// <summary>The squirt: a stream extends from the lingering ghost nozzle's tip down the
        /// column. Linear, so its front matches the row-by-row clear (tip speed ≈
        /// CELL_VISUAL_HEIGHT / KETCHUP_CLEAR_STAGGER — keep those knobs paired).</summary>
        public static void KetchupSquirt(Column column)
        {
            if (column == null) return;

            GameObject root = new GameObject("Vfx_Ketchup");
            Vector3 floor = column.GetWorldPositionForRow(0);
            float ghostCenterY = column.GetWorldPositionForRow(Constants.MAX_ROWS).y
                + UIStyles.CONSUMABLE_GHOST_Y_OFFSET;
            float streamTop = ghostCenterY - UIStyles.CONSUMABLE_GHOST_HEIGHT * 0.5f;
            float streamHeight = streamTop - (floor.y - UIStyles.FX_STREAM_FLOOR_OVERLAP);

            // The stream grows downward: a pivot at the ghost's tip scales 0→1 in Y while the
            // sprite hangs half its height below it, so the top edge stays pinned to the nozzle.
            GameObject pivot = new GameObject("StreamPivot");
            pivot.transform.SetParent(root.transform, false);
            pivot.transform.position = new Vector3(floor.x, streamTop, 0f);

            GameObject streamObj = new GameObject("Stream");
            streamObj.transform.SetParent(pivot.transform, false);
            streamObj.transform.localPosition = new Vector3(0f, -streamHeight * 0.5f, 0f);
            SpriteRenderer stream = streamObj.AddComponent<SpriteRenderer>();
            stream.sprite = RewardArt.KetchupStream;
            stream.sortingOrder = Constants.SORT_CONSUMABLE_FX_STREAM;
            SpriteFit.Height(stream, streamHeight);

            pivot.transform.localScale = new Vector3(1f, 0f, 1f);

            DOTween.Sequence().SetLink(root)
                .Append(pivot.transform.DOScaleY(1f, AnimConfig.FX_STREAM_EXTEND_DURATION).SetEase(Ease.Linear))
                .AppendInterval(AnimConfig.FX_HOLD_DURATION)
                .Append(stream.DOFade(0f, AnimConfig.FX_FADE_DURATION))
                .OnComplete(() => Object.Destroy(root));
        }

        /// <summary>After the skewer lands, only its head stays visible, slamming down to hover
        /// just above the pinned base — the depth read of the stick going through the stack.</summary>
        public static void SkewerPin(Column column)
        {
            if (column == null) return;

            GameObject root = new GameObject("Vfx_SkewerPin");
            Vector3 floor = column.GetWorldPositionForRow(0);
            float pinY = floor.y + UIStyles.FX_SKEWER_HEAD_PIN_Y;

            SpriteRenderer head = MakeSprite(root.transform, RewardArt.SkewerHead,
                new Vector3(floor.x, pinY + UIStyles.FX_SKEWER_HEAD_DROP_FROM, 0f),
                Constants.SORT_CONSUMABLE_FX_NOZZLE, UIStyles.FX_SKEWER_HEAD_HEIGHT);

            DOTween.Sequence().SetLink(root)
                .Append(head.transform.DOMoveY(pinY, AnimConfig.FX_SKEWER_PIN_DROP_DURATION).SetEase(Ease.InQuad))
                .AppendInterval(AnimConfig.FX_SKEWER_PIN_HOLD_DURATION)
                .Append(head.DOFade(0f, AnimConfig.FX_FADE_DURATION))
                .OnComplete(() => Object.Destroy(root));
        }

        private static SpriteRenderer MakeSprite(Transform parent, Sprite sprite, Vector3 position,
            int sortingOrder, float worldHeight)
        {
            GameObject obj = new GameObject("FxSprite");
            obj.transform.SetParent(parent, false);
            obj.transform.position = position;
            SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            SpriteFit.Height(renderer, worldHeight);
            return renderer;
        }
    }
}
