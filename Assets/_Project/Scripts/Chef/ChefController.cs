using UnityEngine;
using DG.Tweening;

namespace DogtorBurguer
{
    public class ChefController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _moveSpeed = AnimConfig.CHEF_MOVE_DURATION;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        [Header("Tap Radius")]
        [Tooltip("World radius around the cook that counts as a tap-to-flip (× CHEF_TAP_RADIUS_MULT).")]
        [SerializeField] private float _bubbleRadius = UIStyles.BUBBLE_RADIUS;

        private int _currentPosition; // 0, 1, or 2 (between column pairs)
        private bool _isMoving;
        private Tween _moveTween;
        private Tween _flipTween;
        private bool _isFlipped;

        public int CurrentPosition => _currentPosition;
        public bool IsMoving => _isMoving;
        public int LeftColumnIndex => _currentPosition;
        public int RightColumnIndex => _currentPosition + 1;

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            // Skin the chef from the active theme; keep any scene-assigned sprite if none is authored.
            if (_spriteRenderer != null && Theme.Chef != null)
            {
                _spriteRenderer.sprite = Theme.Chef;
            }

            // The chef tucks behind the plates (its hands hide under them).
            if (_spriteRenderer != null)
                _spriteRenderer.sortingOrder = Constants.SORT_CHEF;
        }

        private void Start()
        {
            _currentPosition = Constants.CHEF_START_POSITION;
            transform.position = GetWorldPosition(_currentPosition);
            Debug.Log($"[Chef] Started at position {_currentPosition}");
        }

        private Vector3 GetWorldPosition(int position)
        {
            // Chef stands between two columns, so position between their X values
            float leftX = Constants.GRID_ORIGIN_X + (position * Constants.CELL_WIDTH);
            float rightX = Constants.GRID_ORIGIN_X + ((position + 1) * Constants.CELL_WIDTH);
            float x = (leftX + rightX) / 2f;

            // Anchor the chef's feet to a fixed bottom line and derive the centre from the live
            // sprite height, so resizing the sprite keeps it sitting on the bottom border.
            float feetY = Constants.GRID_ORIGIN_Y - Constants.CHEF_BOTTOM_OFFSET;
            float halfHeight = (_spriteRenderer != null && _spriteRenderer.sprite != null)
                ? _spriteRenderer.sprite.bounds.extents.y
                : 0f;

            return new Vector3(x, feetY + halfHeight, 0);
        }

        public void MoveToPosition(int newPosition)
        {
            if (_isMoving) return;

            newPosition = Mathf.Clamp(newPosition, 0, Constants.CHEF_POSITION_COUNT - 1);
            if (newPosition == _currentPosition) return;

            _currentPosition = newPosition;
            _isMoving = true;

            Vector3 targetPos = GetWorldPosition(_currentPosition);

            _moveTween?.Kill();
            _moveTween = transform
                .DOMove(targetPos, _moveSpeed)
                .SetEase(Ease.OutBack)
                .OnComplete(() => _isMoving = false);
        }

        public void MoveLeft()
        {
            MoveToPosition(_currentPosition - 1);
        }

        public void MoveRight()
        {
            MoveToPosition(_currentPosition + 1);
        }

        public void SwapPlates()
        {
            if (_isMoving) return;
            // No swapping while a burger is resolving — would corrupt the column being compressed (F-31).
            if (GameManager.Instance != null && GameManager.Instance.IsResolving) return;

            Debug.Log($"[Chef] Swapping columns {LeftColumnIndex} and {RightColumnIndex}");

            // Snap to the current logical state before starting the new flip — if
            // SwapPlates is called mid-flip, tweening from a partial angle (or with a
            // half-applied sprite swap) looks wrong.
            _flipTween?.Kill();
            transform.rotation = Quaternion.Euler(0, _isFlipped ? 180f : 0f, 0);
            ApplyFlipVisual(_isFlipped);

            bool target = !_isFlipped;
            _isFlipped = target;
            float targetY = target ? 180f : 0f;

            // 3D flip: a single 180° Y-rotation (unchanged motion). At the edge-on
            // midpoint the sprite is ~zero-width, so we swap Front<->Flipped there —
            // the half that expands back out reveals the correct authored art.
            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DORotate(new Vector3(0, targetY, 0), AnimConfig.CHEF_FLIP_DURATION)
                .SetEase(Ease.InOutQuad));
            seq.InsertCallback(AnimConfig.CHEF_FLIP_DURATION * 0.5f, () => ApplyFlipVisual(target));
            _flipTween = seq;

            // Slide the two plates to swap columns alongside the flip (positions only — the
            // plate sprite itself doesn't rotate/flip).
            PlateManager.Instance?.SwapColumns(LeftColumnIndex, RightColumnIndex, AnimConfig.CHEF_FLIP_DURATION);

            // Tell GridManager to swap with wave effect
            GridManager.Instance?.SwapColumnsWithWaveEffect(LeftColumnIndex, RightColumnIndex);
        }

        // Shows the correct chef sprite for the current facing. When flipped, the body is
        // rotated 180° on Y (which renders the sprite mirrored), so flipX cancels that
        // mirror and the Flipped art reads as drawn (e.g. the "D" badge stays correct).
        private void ApplyFlipVisual(bool flipped)
        {
            if (_spriteRenderer == null) return;
            Sprite front = Theme.Chef;
            Sprite back = Theme.ChefFlipped;
            // Prefer the facing-appropriate sprite, fall back to the other, and never blank
            // the renderer (a missing lookup must not make the chef vanish).
            Sprite chosen = flipped ? (back != null ? back : front)
                                    : (front != null ? front : back);
            if (chosen != null)
                _spriteRenderer.sprite = chosen;
            _spriteRenderer.flipX = flipped;
        }

        public Vector3 GetPositionWorldPos(int position)
        {
            return GetWorldPosition(position);
        }

        public float BubbleRadius => _bubbleRadius;

        private void OnDestroy()
        {
            _moveTween?.Kill();
            _flipTween?.Kill();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw chef position indicator
            Gizmos.color = Color.yellow;
            Vector3 pos = Application.isPlaying ? transform.position : GetWorldPosition(Constants.CHEF_START_POSITION);
            Gizmos.DrawWireSphere(pos, 0.3f);

            // Draw which columns the chef covers
            Gizmos.color = Color.cyan;
            int leftCol = Application.isPlaying ? LeftColumnIndex : Constants.CHEF_START_POSITION;
            int rightCol = Application.isPlaying ? RightColumnIndex : Constants.CHEF_START_POSITION + 1;

            Vector3 leftPos = new Vector3(
                Constants.GRID_ORIGIN_X + leftCol * Constants.CELL_WIDTH,
                Constants.GRID_ORIGIN_Y,
                0
            );
            Vector3 rightPos = new Vector3(
                Constants.GRID_ORIGIN_X + rightCol * Constants.CELL_WIDTH,
                Constants.GRID_ORIGIN_Y,
                0
            );

            Gizmos.DrawLine(pos, leftPos);
            Gizmos.DrawLine(pos, rightPos);
        }
#endif
    }
}
