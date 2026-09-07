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
        private IngredientState _state = IngredientState.Spawned;
        private Tween _currentTween;
        private Tween _waveTween;
        private Tween _slideTween; // lateral glide into a swapped lane (independent of the fall)

        public IngredientType Type => _type;
        public Column CurrentColumn => _currentColumn;
        public int CurrentRow => _currentRow;
        public IngredientState State => _state;

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
            _state = IngredientState.Spawned;

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
            if (_state == IngredientState.Falling) return;
            _state = IngredientState.Falling;

            // Falling ingredients render in front of stacked ones
            if (_spriteRenderer != null)
                _spriteRenderer.sortingOrder = Constants.MAX_ROWS + 1;

            // Register as falling ingredient
            GridManager.Instance?.RegisterFallingIngredient(this);

            FallOneStep(stepDuration);
        }

        public void PauseFalling()
        {
            _currentTween?.Pause();
        }

        public void ResumeFalling()
        {
            _currentTween?.Play();
        }

        private void FallOneStep(float stepDuration)
        {
            if (_state != IngredientState.Falling) return;

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

            // Fall one step down (visual height) — Y only: the lateral slide into a swapped
            // lane runs on its own FASTER tween (_slideTween), so the axes never fight.
            _currentTween?.Kill();
            _currentTween = transform
                .DOMoveY(currentPos.y - Constants.CELL_VISUAL_HEIGHT, stepDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() => FallOneStep(stepDuration));
        }

        private void Land()
        {
            _state = IngredientState.Landed;

            // Unregister as falling ingredient
            GridManager.Instance?.UnregisterFallingIngredient(this);

            // Add to column stack
            _currentColumn.AddIngredient(this);

            // Snap to exact position (finish any lateral slide first so it can't fight the snap)
            _slideTween?.Kill();
            Vector3 landPos = _currentColumn.GetWorldPositionForRow(_currentRow);
            transform.position = landPos;

            // Landing squash animation
            _currentTween?.Kill();
            transform.localScale = Vector3.one;
            _currentTween = transform
                .DOPunchScale(new Vector3(AnimConfig.LAND_PUNCH_SCALE, -AnimConfig.LAND_PUNCH_SCALE, 0), AnimConfig.LAND_PUNCH_DURATION, AnimConfig.LAND_PUNCH_VIBRATO, AnimConfig.LAND_PUNCH_ELASTICITY);

            // Notify grid manager
            GridManager.Instance?.OnIngredientLanded(this);
        }

        public void FallToRow(int row)
        {
            _currentRow = row;
            Vector3 targetPos = _currentColumn.GetWorldPositionForRow(row);

            _currentTween?.Kill();
            _currentTween = transform
                .DOMove(targetPos, AnimConfig.COLLAPSE_DURATION)
                .SetEase(Ease.OutBounce);
        }

        public void AnimateToCurrentPosition()
        {
            Vector3 targetPos = _currentColumn.GetWorldPositionForRow(_currentRow);

            _currentTween?.Kill();
            _currentTween = transform
                .DOMove(targetPos, AnimConfig.MOVE_TO_POSITION_DURATION)
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
            seq.Append(transform.DOPunchScale(new Vector3(AnimConfig.WAVE_PUNCH_SCALE, -AnimConfig.WAVE_PUNCH_SCALE, 0), AnimConfig.WAVE_PUNCH_DURATION, AnimConfig.WAVE_PUNCH_VIBRATO, AnimConfig.WAVE_PUNCH_ELASTICITY));
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
            seq.Append(transform.DOMove(targetPos, AnimConfig.WAVE_MOVE_DURATION).SetEase(Ease.OutBack));
            seq.Join(transform.DOPunchScale(new Vector3(AnimConfig.WAVE_PUNCH_SCALE, -AnimConfig.WAVE_PUNCH_SCALE, 0), AnimConfig.WAVE_COMBINED_PUNCH_DURATION, AnimConfig.WAVE_PUNCH_VIBRATO, AnimConfig.WAVE_PUNCH_ELASTICITY));
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

            // The lateral slide is its own FAST tween (much quicker than a fall step — the
            // one-step diagonal read as sluggish) while the Y-only fall continues independently.
            if (_state == IngredientState.Falling)
            {
                float targetX = Constants.GRID_ORIGIN_X + newColumn.ColumnIndex * Constants.CELL_WIDTH;
                _slideTween?.Kill();
                _slideTween = transform.DOMoveX(targetX, AnimConfig.SWAP_SLIDE_DURATION)
                    .SetEase(Ease.OutQuad).SetLink(gameObject);
                FallOneStep(stepDuration);
            }
        }

        /// <summary>
        /// Gets current Y position in world space
        /// </summary>
        public float CurrentY => transform.position.y;

        public void FastDrop()
        {
            if (_state != IngredientState.Falling) return;

            // Calculate distance remaining
            float landingY = Constants.GRID_ORIGIN_Y + (_currentColumn.StackHeight * Constants.CELL_VISUAL_HEIGHT);
            float distanceToLand = transform.position.y - landingY;

            // Award points based on distance
            int points = Scoring.FastDropPoints(distanceToLand);
            if (points > 0)
            {
                GameManager.Instance?.AddExtraScore(points);
                FloatingText.Spawn(transform.position, $"{points}!", UIStyles.FAST_DROP_POPUP, 3f, "ui_popup_plate");
            }

            // Cancel current fall and drop fast (a mid-flight lane slide completes instantly
            // so the drop captures the true lane X).
            _slideTween?.Complete();
            _currentTween?.Kill();
            Vector3 targetPos = new Vector3(transform.position.x, landingY, transform.position.z);
            _currentTween = transform
                .DOMove(targetPos, AnimConfig.FAST_DROP_DURATION)
                .SetEase(Ease.InQuad)
                .OnComplete(() => Land());

            // Visual: brief stretch effect
            transform.DOScaleY(AnimConfig.FAST_DROP_STRETCH_Y, AnimConfig.FAST_DROP_STRETCH_DURATION).OnComplete(() =>
                transform.DOScaleY(1f, AnimConfig.FAST_DROP_STRETCH_DURATION));
        }

        public void DestroyWithAnimation()
        {
            _currentTween?.Kill();
            _waveTween?.Kill();
            transform.DOKill();

            if (this == null || gameObject == null) return;

            Sequence seq = DOTween.Sequence();
            seq.SetTarget(gameObject);
            seq.Append(transform.DOScale(Vector3.zero, AnimConfig.DESTROY_SPIN_DURATION).SetEase(Ease.InBack));
            seq.Join(transform.DORotate(new Vector3(0, 0, 180), AnimConfig.DESTROY_SPIN_DURATION, RotateMode.FastBeyond360));
            seq.OnComplete(() =>
            {
                if (this != null && gameObject != null)
                    Destroy(gameObject);
            });
        }

        /// <summary>Flash-blink out and destroy. <paramref name="delay"/> postpones the visual only
        /// (state is already removed) — the ketchup clear staggers rows with it.</summary>
        public void DestroyWithFlash(float delay = 0f)
        {
            _currentTween?.Kill();
            _waveTween?.Kill();
            transform.DOKill();
            if (_spriteRenderer != null) _spriteRenderer.DOKill();

            if (this == null || gameObject == null) return;

            Sequence seq = DOTween.Sequence();
            seq.SetTarget(gameObject);
            if (delay > 0f) seq.AppendInterval(delay);
            // Blink twice (visible -> invisible -> visible -> invisible -> visible)
            seq.Append(_spriteRenderer.DOColor(Color.clear, AnimConfig.FLASH_BLINK_DURATION));
            seq.Append(_spriteRenderer.DOColor(Color.white, AnimConfig.FLASH_BLINK_DURATION));
            seq.Append(_spriteRenderer.DOColor(Color.clear, AnimConfig.FLASH_BLINK_DURATION));
            seq.Append(_spriteRenderer.DOColor(Color.white, AnimConfig.FLASH_BLINK_DURATION));
            // Scale out and spin
            seq.Append(transform.DOScale(Vector3.zero, AnimConfig.FLASH_SCALE_OUT_DURATION).SetEase(Ease.InBack));
            seq.Join(transform.DORotate(new Vector3(0, 0, 180), AnimConfig.FLASH_SCALE_OUT_DURATION, RotateMode.FastBeyond360));
            seq.OnComplete(() =>
            {
                if (this != null && gameObject != null)
                    Destroy(gameObject);
            });
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();
            _waveTween?.Kill();
            _slideTween?.Kill();
            DOTween.Kill(gameObject);
            // Tweens that target the RENDERER directly (the burger gold flash, the blink in
            // DestroyWithFlash) aren't reached by the gameObject-target kill — they were the
            // "destroyed SpriteRenderer" DOTween spam in the editor log (2026-09-07).
            if (_spriteRenderer != null) _spriteRenderer.DOKill();
            // Ensure we're unregistered from falling list
            GridManager.Instance?.UnregisterFallingIngredient(this);
        }
    }
}
