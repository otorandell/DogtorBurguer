using DG.Tweening;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Authored "in use" effects, purely cosmetic and fire-and-forget: spawned on impact over the
    /// normal removal animations (non-blocking — the grid resolves underneath) and self-destroyed
    /// when the tween ends. The nozzles LOCK onto the used column (per the art direction):
    /// ketchup's squirts a stream down it, mustard's bursts in place (its drop is the faller).
    /// The skewer leaves only its head pinned at the base — the stick "went through" the stack.
    /// Sizes in UIStyles (FX_*), timings in AnimConfig (FX_*).
    /// </summary>
    public static class ConsumableVfx
    {
        public static void KetchupSquirt(Column column)
        {
            if (column == null) return;

            GameObject root = new GameObject("Vfx_Ketchup");
            float x = column.GetWorldPositionForRow(0).x;
            float nozzleY = column.GetWorldPositionForRow(Constants.MAX_ROWS).y + UIStyles.FX_NOZZLE_TOP_OFFSET;

            SpriteRenderer nozzle = MakeSprite(root.transform, RewardArt.KetchupNozzle,
                new Vector3(x, nozzleY, 0f), Constants.SORT_CONSUMABLE_FX_NOZZLE, UIStyles.FX_NOZZLE_HEIGHT);

            // The stream grows downward: a pivot at the nozzle tip scales 0→1 in Y while the
            // sprite hangs half its height below it, so the top edge stays pinned to the nozzle.
            float streamTop = nozzleY - UIStyles.FX_NOZZLE_HEIGHT * 0.5f;
            float streamHeight = streamTop - (column.GetWorldPositionForRow(0).y - UIStyles.FX_STREAM_FLOOR_OVERLAP);

            GameObject pivot = new GameObject("StreamPivot");
            pivot.transform.SetParent(root.transform, false);
            pivot.transform.position = new Vector3(x, streamTop, 0f);

            GameObject streamObj = new GameObject("Stream");
            streamObj.transform.SetParent(pivot.transform, false);
            streamObj.transform.localPosition = new Vector3(0f, -streamHeight * 0.5f, 0f);
            SpriteRenderer stream = streamObj.AddComponent<SpriteRenderer>();
            stream.sprite = RewardArt.KetchupStream;
            stream.sortingOrder = Constants.SORT_CONSUMABLE_FX_STREAM;
            SpriteFit.Height(stream, streamHeight);

            Vector3 nozzleFit = nozzle.transform.localScale;
            nozzle.transform.localScale = Vector3.zero;
            pivot.transform.localScale = new Vector3(1f, 0f, 1f);

            DOTween.Sequence().SetLink(root)
                .Append(nozzle.transform.DOScale(nozzleFit, AnimConfig.FX_NOZZLE_POP_DURATION).SetEase(Ease.OutBack))
                .Append(pivot.transform.DOScaleY(1f, AnimConfig.FX_STREAM_EXTEND_DURATION).SetEase(Ease.OutQuad))
                .AppendInterval(AnimConfig.FX_HOLD_DURATION)
                .Append(nozzle.DOFade(0f, AnimConfig.FX_FADE_DURATION))
                .Join(stream.DOFade(0f, AnimConfig.FX_FADE_DURATION))
                .OnComplete(() => Object.Destroy(root));
        }

        /// <summary>The mustard nozzle locks over the used column and bursts (the falling drop is
        /// the faller sprite — see MustardEffect.FallerSprite).</summary>
        public static void MustardBurst(Column column)
        {
            if (column == null) return;

            GameObject root = new GameObject("Vfx_Mustard");
            float x = column.GetWorldPositionForRow(0).x;
            float y = column.GetWorldPositionForRow(Constants.MAX_ROWS).y + UIStyles.FX_NOZZLE_TOP_OFFSET;

            SpriteRenderer nozzle = MakeSprite(root.transform, RewardArt.MustardNozzle,
                new Vector3(x, y, 0f), Constants.SORT_CONSUMABLE_FX_NOZZLE, UIStyles.FX_NOZZLE_HEIGHT);

            Vector3 nozzleFit = nozzle.transform.localScale;
            nozzle.transform.localScale = Vector3.zero;

            DOTween.Sequence().SetLink(root)
                .Append(nozzle.transform.DOScale(nozzleFit, AnimConfig.FX_NOZZLE_POP_DURATION).SetEase(Ease.OutBack))
                .AppendInterval(AnimConfig.FX_MUSTARD_HOLD_DURATION)
                .Append(nozzle.DOFade(0f, AnimConfig.FX_FADE_DURATION))
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
