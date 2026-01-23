using UnityEngine;
using DG.Tweening;

namespace DogtorBurguer
{
    public class ChefController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _moveSpeed = 0.15f;
        [SerializeField] private int _startPosition = 1; // Middle position

        [Header("Visual")]
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private int _currentPosition; // 0, 1, or 2 (between column pairs)
        private bool _isMoving;
        private Tween _moveTween;
        private Tween _flipTween;
        private bool _isFlipped;

        public int CurrentPosition => _currentPosition;
        public int LeftColumnIndex => _currentPosition;
        public int RightColumnIndex => _currentPosition + 1;

        private void Awake()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        private void Start()
        {
            _currentPosition = Mathf.Clamp(_startPosition, 0, Constants.CHEF_POSITION_COUNT - 1);
            transform.position = GetWorldPosition(_currentPosition);
            Debug.Log($"[Chef] Started at position {_currentPosition}");
        }

        private Vector3 GetWorldPosition(int position)
        {
            // Chef stands between two columns, so position between their X values
            float leftX = Constants.GRID_ORIGIN_X + (position * Constants.CELL_WIDTH);
            float rightX = Constants.GRID_ORIGIN_X + ((position + 1) * Constants.CELL_WIDTH);
            float x = (leftX + rightX) / 2f;

            // Y position is below the grid
            float y = Constants.GRID_ORIGIN_Y - Constants.CELL_VISUAL_HEIGHT * 0.8f;

            return new Vector3(x, y, 0);
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

            Debug.Log($"[Chef] Moving to position {_currentPosition}");
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

            Debug.Log($"[Chef] Swapping columns {LeftColumnIndex} and {RightColumnIndex}");

            // Kill any existing flip and snap to clean state
            _flipTween?.Kill();
            float targetY = _isFlipped ? 0f : 180f;
            transform.rotation = Quaternion.Euler(0, _isFlipped ? 180f : 0f, 0);

            // 2D Flip effect - 180 degree rotation on Y axis
            _isFlipped = !_isFlipped;
            _flipTween = transform.DORotate(new Vector3(0, targetY, 0), 0.2f)
                .SetEase(Ease.InOutQuad);

            // Tell GridManager to swap with wave effect
            GridManager.Instance?.SwapColumnsWithWaveEffect(LeftColumnIndex, RightColumnIndex);
        }

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
            Vector3 pos = Application.isPlaying ? transform.position : GetWorldPosition(_startPosition);
            Gizmos.DrawWireSphere(pos, 0.3f);

            // Draw which columns the chef covers
            Gizmos.color = Color.cyan;
            int leftCol = Application.isPlaying ? LeftColumnIndex : _startPosition;
            int rightCol = Application.isPlaying ? RightColumnIndex : _startPosition + 1;

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
