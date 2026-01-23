using UnityEngine;
using DG.Tweening;

namespace DogtorBurguer
{
    public class Ingredient : MonoBehaviour
    {
        [SerializeField] private IngredientType _type;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private Column _currentColumn;
        private int _currentRow;
        private bool _isLanded;
        private bool _isFalling;
        private Tween _currentTween;
        private Tween _waveTween;

        public IngredientType Type => _type;
        public Column CurrentColumn => _currentColumn;
        public int CurrentRow => _currentRow;
        public bool IsLanded => _isLanded;
        public bool IsFalling => _isFalling;

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        public void Initialize(IngredientType type, Column column, Sprite sprite = null)
        {
            _type = type;
            _currentColumn = column;
            _isLanded = false;
            _isFalling = false;

            // Auto-get SpriteRenderer if not assigned
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (sprite != null && _spriteRenderer != null)
            {
                _spriteRenderer.sprite = sprite;
            }

            // Position at spawn point
            transform.position = column.GetSpawnPosition();

            Debug.Log($"[Ingredient] Spawned {type} at column {column.ColumnIndex}, pos: {transform.position}");
        }

        public void SetColumnAndRow(Column column, int row)
        {
            _currentColumn = column;
            _currentRow = row;
            if (_spriteRenderer != null)
                _spriteRenderer.sortingOrder = row;
        }

        public void StartFalling(float stepDuration)
        {
            if (_isFalling) return;
            _isFalling = true;

            // Falling ingredients render in front of stacked ones
            if (_spriteRenderer != null)
                _spriteRenderer.sortingOrder = Constants.MAX_ROWS + 1;

            // Register as falling ingredient
            GridManager.Instance?.RegisterFallingIngredient(this);

            FallOneStep(stepDuration);
        }

        private void FallOneStep(float stepDuration)
        {
            if (_isLanded || !_isFalling) return;

            int targetRow = _currentColumn.StackHeight;
            Vector3 currentPos = transform.position;

            // Calculate the row we're currently at (based on visual height)
            int currentVisualRow = Mathf.RoundToInt(
                (currentPos.y - Constants.GRID_ORIGIN_Y) / Constants.CELL_VISUAL_HEIGHT
            );

            // If we've reached the target row, land
            if (currentVisualRow <= targetRow)
            {
                Land();
                return;
            }

            // Fall one step down (visual height)
            Vector3 targetPos = new Vector3(
                currentPos.x,
                currentPos.y - Constants.CELL_VISUAL_HEIGHT,
                currentPos.z
            );

            _currentTween?.Kill();
            _currentTween = transform
                .DOMove(targetPos, stepDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() => FallOneStep(stepDuration));
        }

        private void Land()
        {
            _isFalling = false;
            _isLanded = true;

            // Unregister as falling ingredient
            GridManager.Instance?.UnregisterFallingIngredient(this);

            // Add to column stack
            _currentColumn.AddIngredient(this);

            // Snap to exact position
            Vector3 landPos = _currentColumn.GetWorldPositionForRow(_currentRow);
            transform.position = landPos;

            // Landing squash animation
            _currentTween?.Kill();
            transform.localScale = Vector3.one;
            _currentTween = transform
                .DOPunchScale(new Vector3(0.2f, -0.2f, 0), 0.2f, 5, 0.5f);

            // Notify grid manager
            GridManager.Instance?.OnIngredientLanded(this);
        }

        public void FallToRow(int row)
        {
            _currentRow = row;
            Vector3 targetPos = _currentColumn.GetWorldPositionForRow(row);

            _currentTween?.Kill();
            _currentTween = transform
                .DOMove(targetPos, 0.15f)
                .SetEase(Ease.OutBounce);
        }

        public void AnimateToCurrentPosition()
        {
            Vector3 targetPos = _currentColumn.GetWorldPositionForRow(_currentRow);

            _currentTween?.Kill();
            _currentTween = transform
                .DOMove(targetPos, 0.2f)
                .SetEase(Ease.OutBack);
        }

        /// <summary>
        /// Wave effect - just visual bounce, no flip
        /// </summary>
        public void DoWaveEffect(float delay)
        {
            // Kill existing wave and reset scale
            _waveTween?.Kill();
            transform.localScale = Vector3.one;

            Sequence seq = DOTween.Sequence();
            seq.SetDelay(delay);
            seq.Append(transform.DOPunchScale(new Vector3(0.15f, -0.15f, 0), 0.2f, 4, 0.5f));
            _waveTween = seq;
        }

        /// <summary>
        /// Animate to position with wave effect (staggered movement)
        /// </summary>
        public void AnimateToCurrentPositionWithWave(float delay)
        {
            // Kill existing wave and reset scale
            _waveTween?.Kill();
            transform.localScale = Vector3.one;

            Vector3 targetPos = _currentColumn.GetWorldPositionForRow(_currentRow);

            Sequence seq = DOTween.Sequence();
            seq.SetDelay(delay);
            seq.Append(transform.DOMove(targetPos, 0.2f).SetEase(Ease.OutBack));
            seq.Join(transform.DOPunchScale(new Vector3(0.15f, -0.15f, 0), 0.25f, 4, 0.5f));
            _waveTween = seq;
        }

        /// <summary>
        /// Swaps this falling ingredient to a different column
        /// </summary>
        public void SwapToColumn(Column newColumn, float stepDuration)
        {
            // Kill current fall animation to avoid conflicts
            _currentTween?.Kill();

            _currentColumn = newColumn;

            // Snap X position immediately to new column
            float targetX = Constants.GRID_ORIGIN_X + (newColumn.ColumnIndex * Constants.CELL_WIDTH);
            Vector3 pos = transform.position;
            pos.x = targetX;
            transform.position = pos;

            // Resume falling
            if (_isFalling && !_isLanded)
            {
                FallOneStep(stepDuration);
            }
        }

        /// <summary>
        /// Gets current Y position in world space
        /// </summary>
        public float CurrentY => transform.position.y;

        public void DestroyWithAnimation()
        {
            _currentTween?.Kill();

            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));
            seq.Join(transform.DORotate(new Vector3(0, 0, 180), 0.2f, RotateMode.FastBeyond360));
            seq.OnComplete(() => Destroy(gameObject));
        }

        public void DestroyWithFlash()
        {
            _currentTween?.Kill();
            _waveTween?.Kill();

            Sequence seq = DOTween.Sequence();
            // Blink twice (visible -> invisible -> visible -> invisible -> visible)
            seq.Append(_spriteRenderer.DOColor(Color.clear, 0.04f));
            seq.Append(_spriteRenderer.DOColor(Color.white, 0.04f));
            seq.Append(_spriteRenderer.DOColor(Color.clear, 0.04f));
            seq.Append(_spriteRenderer.DOColor(Color.white, 0.04f));
            // Scale out and spin
            seq.Append(transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack));
            seq.Join(transform.DORotate(new Vector3(0, 0, 180), 0.15f, RotateMode.FastBeyond360));
            seq.OnComplete(() => Destroy(gameObject));
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();
            _waveTween?.Kill();
            // Ensure we're unregistered from falling list
            GridManager.Instance?.UnregisterFallingIngredient(this);
        }
    }
}
