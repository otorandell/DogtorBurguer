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

            if (mode == ControlMode.Drag)
            {
                _chef.SwapPlates();
                return;
            }

            // Tap mode: tapping near the chef swaps; tapping to a side moves that way.
            float chefDist = Vector2.Distance(worldPos, _chef.transform.position);
            if (chefDist < _chef.BubbleRadius * GameplayConfig.CHEF_TAP_RADIUS_MULT)
                _chef.SwapPlates();
            else if (worldPos.x < _chef.transform.position.x)
                _chef.MoveLeft();
            else
                _chef.MoveRight();
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
    }
}
