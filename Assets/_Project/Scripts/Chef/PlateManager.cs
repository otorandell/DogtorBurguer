using DG.Tweening;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Owns the four decorative plates — one under each column — that the bottom ingredient
    /// appears to rest on. Purely cosmetic. When the chef flips a column pair, the two affected
    /// plates <b>slide</b> to each other's column (one passing in front of the chef, one behind),
    /// mirroring the swap — but the plate sprite itself never rotates or flips.
    /// </summary>
    public class PlateManager : Singleton<PlateManager>
    {
        // The plate currently occupying each column (index = column). Swapped on each flip.
        private SpriteRenderer[] _platesByColumn;
        private Tween _swapTween;
        private int _swapLeft = -1;
        private int _swapRight = -1;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;
            CreatePlates();
        }

        protected override void OnDestroy()
        {
            _swapTween?.Kill();
            base.OnDestroy();
        }

        private void CreatePlates()
        {
            Sprite sprite = Theme.Plate;
            _platesByColumn = new SpriteRenderer[Constants.COLUMN_COUNT];
            for (int col = 0; col < Constants.COLUMN_COUNT; col++)
            {
                GameObject go = new GameObject($"Plate_{col}");
                go.transform.SetParent(transform, false);
                go.transform.position = PlatePosition(col);
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = Constants.SORT_PLATE;
                _platesByColumn[col] = sr;
            }
        }

        private static Vector3 PlatePosition(int column)
        {
            float x = Constants.GRID_ORIGIN_X + (column * Constants.CELL_WIDTH);
            float y = Constants.GRID_ORIGIN_Y - Constants.PLATE_Y_OFFSET;
            return new Vector3(x, y, 0f);
        }

        /// <summary>
        /// Slides the two plates at <paramref name="leftCol"/>/<paramref name="rightCol"/> to each
        /// other's column over <paramref name="duration"/> (matching the chef flip), one passing in
        /// front of the chef and one behind. No rotation — the plate sprite stays upright.
        /// </summary>
        public void SwapColumns(int leftCol, int rightCol, float duration)
        {
            if (_platesByColumn == null) return;
            if (leftCol < 0 || rightCol < 0 ||
                leftCol >= _platesByColumn.Length || rightCol >= _platesByColumn.Length) return;

            // A previous swap still sliding? Settle it instantly before starting the new one.
            SettleActiveSwap();

            SpriteRenderer left = _platesByColumn[leftCol];
            SpriteRenderer right = _platesByColumn[rightCol];
            if (left == null || right == null) return;

            _swapLeft = leftCol;
            _swapRight = rightCol;

            // Plain position swap — both plates stay at the same sort order, no straddle.
            Sequence seq = DOTween.Sequence();
            seq.Join(left.transform.DOMove(PlatePosition(rightCol), duration).SetEase(Ease.InOutQuad));
            seq.Join(right.transform.DOMove(PlatePosition(leftCol), duration).SetEase(Ease.InOutQuad));
            seq.OnComplete(SettleActiveSwap);
            _swapTween = seq;
        }

        // Snaps any in-progress swap to its resting state: the two plates land on the swapped
        // columns with sort restored. Safe to call when no swap is active.
        private void SettleActiveSwap()
        {
            _swapTween?.Kill();
            _swapTween = null;
            if (_swapLeft < 0 || _swapRight < 0) return;

            SpriteRenderer left = _platesByColumn[_swapLeft];
            SpriteRenderer right = _platesByColumn[_swapRight];

            // The plate that slid from leftCol now lives at rightCol and vice versa.
            _platesByColumn[_swapLeft] = right;
            _platesByColumn[_swapRight] = left;

            RestPlate(_platesByColumn[_swapLeft], _swapLeft);
            RestPlate(_platesByColumn[_swapRight], _swapRight);

            _swapLeft = -1;
            _swapRight = -1;
        }

        private void RestPlate(SpriteRenderer plate, int column)
        {
            if (plate == null) return;
            plate.transform.position = PlatePosition(column);
            plate.sortingOrder = Constants.SORT_PLATE;
        }
    }
}
