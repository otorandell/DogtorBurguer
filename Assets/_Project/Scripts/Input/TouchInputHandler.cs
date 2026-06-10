using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace DogtorBurguer
{
    public class TouchInputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ChefController _chef;
        [SerializeField] private Camera _camera;
        [SerializeField] private IngredientSpawner _spawner;

        [Header("Settings")]
        [SerializeField] private float _swipeThreshold = 50f; // pixels

        // Camera projection distance for ScreenToWorldPoint (algorithmic, not a tuning value).
        private const float ScreenToWorldZ = 10f;

        private Vector2 _touchStartPos;
        private bool _pressActive; // true between press and release, in both control modes

        private void Awake()
        {
            if (_chef == null)
                _chef = FindAnyObjectByType<ChefController>();
            if (_camera == null)
                _camera = Camera.main;
            if (_spawner == null)
                _spawner = FindAnyObjectByType<IngredientSpawner>();
        }

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        private void Update()
        {
            // Block gameplay input unless actively playing. Pause is now a modifier on
            // the Playing state (F-21), so check IsPaused too — CurrentState stays Playing.
            if (GameManager.Instance != null &&
                (GameManager.Instance.CurrentState != GameState.Playing
                 || GameManager.Instance.IsPaused
                 || GameManager.Instance.IsResolving))
            {
                return;
            }

            HandleInput();
        }

        private void HandleInput()
        {
            // Keyboard input (always checked)
            HandleKeyboardInput();

            // Handle touch input first if available
            if (Touch.activeTouches.Count > 0)
            {
                HandleTouchInput();
            }
            else
            {
                // Fall back to mouse input
                HandleMouseInput();
            }
        }

        private void HandleKeyboardInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || _chef == null) return;

            if (keyboard.aKey.wasPressedThisFrame)
                _chef.MoveLeft();
            else if (keyboard.dKey.wasPressedThisFrame)
                _chef.MoveRight();

            if (keyboard.spaceKey.wasPressedThisFrame)
                _chef.SwapPlates();
        }

        private void HandleMouseInput()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _touchStartPos = mouse.position.ReadValue();
                _pressActive = true;
            }
            else if (mouse.leftButton.wasReleasedThisFrame && _pressActive)
            {
                _pressActive = false;
                Vector2 endPos = mouse.position.ReadValue();
                ProcessInput(_touchStartPos, endPos);
            }
        }

        private void HandleTouchInput()
        {
            Touch touch = Touch.activeTouches[0];

            switch (touch.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    _touchStartPos = touch.screenPosition;
                    _pressActive = true;
                    break;

                case UnityEngine.InputSystem.TouchPhase.Ended:
                case UnityEngine.InputSystem.TouchPhase.Canceled:
                    if (_pressActive)
                    {
                        _pressActive = false;
                        ProcessInput(_touchStartPos, touch.screenPosition);
                    }
                    break;
            }
        }

        private void ProcessInput(Vector2 startScreenPos, Vector2 endScreenPos)
        {
            if (_chef == null) return;

            // Gesture is mode-independent: a horizontal swipe moves the chef in either mode.
            Vector2 delta = endScreenPos - startScreenPos;
            if (delta.magnitude > _swipeThreshold)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    MoveChefHorizontal(delta.x);
                return;
            }

            // Tap: world-object taps (preview / falling / gem pack) work in both modes.
            if (_camera == null) return;
            Vector3 worldPos = _camera.ScreenToWorldPoint(new Vector3(startScreenPos.x, startScreenPos.y, ScreenToWorldZ));
            worldPos.z = 0f;

            if (_spawner != null && _spawner.TryTapPreview(worldPos)) return;
            if (_spawner != null && _spawner.TryTapFallingIngredient(worldPos)) return;
            if (GemPack.TryTapAt(worldPos)) return;

            // Only the remaining tap intent depends on the control mode.
            ControlMode mode = SaveDataManager.Instance != null
                ? SaveDataManager.Instance.ControlMode
                : SaveDataManager.DEFAULT_CONTROL_MODE;

            // Tapping the chef swaps plates — same in both modes. The radius bounds it, so a tap up
            // in the playfield (e.g. a near-miss on a falling piece) never swaps the cook.
            float chefDist = Vector2.Distance(worldPos, _chef.transform.position);
            bool tappedChef = chefDist < _chef.BubbleRadius * GameplayConfig.CHEF_TAP_RADIUS_MULT;

            if (mode == ControlMode.Drag)
            {
                // Drag mode moves by swiping (handled above); a tap only swaps, and only on the chef.
                if (tappedChef)
                    _chef.SwapPlates();
                return;
            }

            // Tap mode: tap the chef to swap, or tap to a side (below the playfield) to move there.
            if (tappedChef)
                _chef.SwapPlates();
            else if (worldPos.y < Constants.GRID_ORIGIN_Y)
            {
                if (worldPos.x < _chef.transform.position.x)
                    _chef.MoveLeft();
                else
                    _chef.MoveRight();
            }
        }

        private void MoveChefHorizontal(float deltaX)
        {
            if (deltaX > 0)
                _chef.MoveRight();
            else
                _chef.MoveLeft();
        }

        public void OnSwapButtonPressed()
        {
            _chef?.SwapPlates();
        }

#if UNITY_EDITOR
        // Debug gizmos for the chef's tap hit-zones (mirrors ProcessInput), mode-aware. Magenta =
        // flip: tapping within this radius swaps the cook — in BOTH modes. Cyan = move: tapping a
        // side below the grid floor walks the chef — Tap mode ONLY (Drag mode moves by swiping, so
        // it has no move-tap zone). Reads the live ControlMode in play; the default in edit mode.
        private void OnDrawGizmos()
        {
            ChefController chef = _chef != null ? _chef : FindAnyObjectByType<ChefController>();
            if (chef == null) return;

            Vector3 chefPos = Application.isPlaying
                ? chef.transform.position
                : chef.GetPositionWorldPos(Constants.CHEF_START_POSITION);
            float flipRadius = chef.BubbleRadius * GameplayConfig.CHEF_TAP_RADIUS_MULT;

            // Flip (swap) zone — tapping the chef swaps in both modes.
            Gizmos.color = GizmoStyles.ChefFlip;
            Gizmos.DrawWireSphere(chefPos, flipRadius);

            // Move side-zones only exist in Tap mode.
            ControlMode mode = (Application.isPlaying && SaveDataManager.Instance != null)
                ? SaveDataManager.Instance.ControlMode
                : SaveDataManager.DEFAULT_CONTROL_MODE;
            if (mode != ControlMode.Tap) return;

            float leftEdge = Constants.GRID_ORIGIN_X - Constants.CELL_WIDTH * 0.5f;
            float rightEdge = Constants.GRID_ORIGIN_X + (Constants.COLUMN_COUNT - 0.5f) * Constants.CELL_WIDTH;
            float top = Constants.GRID_ORIGIN_Y;
            float bottom = chefPos.y - flipRadius;
            Gizmos.color = GizmoStyles.ChefDrag;
            DrawXBand(leftEdge, chefPos.x, top, bottom);
            DrawXBand(chefPos.x, rightEdge, top, bottom);
        }

        private static void DrawXBand(float xMin, float xMax, float yTop, float yBottom)
        {
            Vector3 center = new Vector3((xMin + xMax) * 0.5f, (yTop + yBottom) * 0.5f, 0f);
            Vector3 size = new Vector3(xMax - xMin, yTop - yBottom, 0f);
            Gizmos.DrawWireCube(center, size);
        }
#endif
    }
}
