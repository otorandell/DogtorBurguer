using DG.Tweening;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Authored "in use" effects, purely cosmetic and fire-and-forget: spawned on impact over the
    /// normal removal animations (non-blocking — the grid resolves underneath) and self-destroyed
    /// when the tween ends. Ketchup: a giant nozzle over the column squirts a stream down it.
    /// Mustard: the nozzle sweeps across the whole board (it removes a type board-wide).
    /// The skewer has no authored effect art yet — its faller + collapse carry the moment.
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

        public static void MustardSweep(Column column)
        {
            if (column == null) return;

            GameObject root = new GameObject("Vfx_Mustard");
            float y = column.GetWorldPositionForRow(Constants.MAX_ROWS).y + UIStyles.FX_NOZZLE_TOP_OFFSET;
            float xStart = Constants.GRID_ORIGIN_X;
            float xEnd = Constants.GRID_ORIGIN_X + (Constants.COLUMN_COUNT - 1) * Constants.CELL_WIDTH;

            SpriteRenderer nozzle = MakeSprite(root.transform, RewardArt.MustardNozzle,
                new Vector3(xStart, y, 0f), Constants.SORT_CONSUMABLE_FX_NOZZLE, UIStyles.FX_NOZZLE_HEIGHT);

            Vector3 nozzleFit = nozzle.transform.localScale;
            nozzle.transform.localScale = Vector3.zero;

            DOTween.Sequence().SetLink(root)
                .Append(nozzle.transform.DOScale(nozzleFit, AnimConfig.FX_NOZZLE_POP_DURATION).SetEase(Ease.OutBack))
                .Append(nozzle.transform.DOMoveX(xEnd, AnimConfig.FX_MUSTARD_SWEEP_DURATION).SetEase(Ease.InOutQuad))
                .Append(nozzle.DOFade(0f, AnimConfig.FX_FADE_DURATION))
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
