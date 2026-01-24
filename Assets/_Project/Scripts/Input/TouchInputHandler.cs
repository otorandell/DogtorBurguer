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

        private Vector2 _touchStartPos;
        private bool _isDragging;

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
            if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
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
                _isDragging = true;
            }
            else if (mouse.leftButton.wasReleasedThisFrame && _isDragging)
            {
                _isDragging = false;
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
                    _isDragging = true;
                    break;

                case UnityEngine.InputSystem.TouchPhase.Ended:
                case UnityEngine.InputSystem.TouchPhase.Canceled:
                    if (_isDragging)
                    {
                        _isDragging = false;
                        ProcessInput(_touchStartPos, touch.screenPosition);
                    }
                    break;
            }
        }

        private void ProcessInput(Vector2 startScreenPos, Vector2 endScreenPos)
        {
            if (_chef == null) return;

            ControlMode mode = SaveDataManager.Instance != null
                ? SaveDataManager.Instance.ControlMode
                : ControlMode.Drag;

            float swipeDistance = Vector2.Distance(startScreenPos, endScreenPos);
            bool isSwipe = swipeDistance > _swipeThreshold;

            if (mode == ControlMode.Drag)
            {
                ProcessDragMode(startScreenPos, endScreenPos, isSwipe);
            }
            else
            {
                ProcessTapMode(startScreenPos, endScreenPos, isSwipe);
            }
        }

        private void ProcessDragMode(Vector2 startScreenPos, Vector2 endScreenPos, bool isSwipe)
        {
            if (isSwipe)
            {
                Vector2 swipeDir = endScreenPos - startScreenPos;
                if (Mathf.Abs(swipeDir.x) > Mathf.Abs(swipeDir.y))
                {
                    if (swipeDir.x > 0)
                        _chef.MoveRight();
                    else
                        _chef.MoveLeft();
                }
            }
            else
            {
                ProcessTap(startScreenPos);
            }
        }

        private void ProcessTapMode(Vector2 startScreenPos, Vector2 endScreenPos, bool isSwipe)
        {
            if (isSwipe)
            {
                // Swipe = move
                Vector2 swipeDir = endScreenPos - startScreenPos;
                if (Mathf.Abs(swipeDir.x) > Mathf.Abs(swipeDir.y))
                {
                    if (swipeDir.x > 0)
                        _chef.MoveRight();
                    else
                        _chef.MoveLeft();
                }
            }
            else
            {
                if (_camera == null) return;

                Vector3 worldPos = _camera.ScreenToWorldPoint(new Vector3(startScreenPos.x, startScreenPos.y, 10f));
                worldPos.z = 0;

                // Check preview tap
                if (_spawner != null && _spawner.TryTapPreview(worldPos))
                    return;

                // Check falling ingredient tap
                if (_spawner != null && _spawner.TryTapFallingIngredient(worldPos))
                    return;

                // Check if tapped on/near the chef → swap
                float chefDist = Vector2.Distance(worldPos, _chef.transform.position);
                if (chefDist < _chef.BubbleRadius * 2f)
                {
                    _chef.SwapPlates();
                    return;
                }

                // Tap left/right of chef = move
                if (worldPos.x < _chef.transform.position.x)
                    _chef.MoveLeft();
                else
                    _chef.MoveRight();
            }
        }

        private void ProcessTap(Vector2 screenPos)
        {
            if (_chef == null) return;

            // Check if tapped on a falling ingredient or preview first
            if (_camera != null)
            {
                Vector3 worldPos = _camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
                worldPos.z = 0;

                if (_spawner != null && _spawner.TryTapPreview(worldPos))
                    return;

                if (_spawner != null && _spawner.TryTapFallingIngredient(worldPos))
                    return;
            }

            // Any other tap → swap
            _chef.SwapPlates();
        }

        public void OnSwapButtonPressed()
        {
            _chef?.SwapPlates();
        }
    }
}
