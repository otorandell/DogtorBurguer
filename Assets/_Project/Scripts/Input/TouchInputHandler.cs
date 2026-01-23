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

        [Header("Settings")]
        [SerializeField] private float _chefTapRadius = 1.0f;
        [SerializeField] private float _swipeThreshold = 50f; // pixels

        private Vector2 _touchStartPos;
        private bool _isDragging;

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
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

            // Check if it's a swipe or a tap
            float swipeDistance = Vector2.Distance(startScreenPos, endScreenPos);

            if (swipeDistance > _swipeThreshold)
            {
                // It's a swipe - determine direction
                Vector2 swipeDir = endScreenPos - startScreenPos;
                if (Mathf.Abs(swipeDir.x) > Mathf.Abs(swipeDir.y))
                {
                    // Horizontal swipe
                    if (swipeDir.x > 0)
                        _chef.MoveRight();
                    else
                        _chef.MoveLeft();
                }
            }
            else
            {
                // It's a tap - check what was tapped
                ProcessTap(startScreenPos);
            }
        }

        private void ProcessTap(Vector2 screenPos)
        {
            if (_camera == null || _chef == null) return;

            Vector3 worldPos = _camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            worldPos.z = 0;

            // Check if tapped on the chef
            float distanceToChef = Vector2.Distance(worldPos, _chef.transform.position);

            if (distanceToChef < _chefTapRadius)
            {
                // Tapped on chef - swap plates
                _chef.SwapPlates();
                Debug.Log("[Input] Tapped chef - swapping");
            }
            else
            {
                // Tapped elsewhere - check if left or right of chef
                if (worldPos.x < _chef.transform.position.x)
                {
                    _chef.MoveLeft();
                    Debug.Log("[Input] Tapped left - moving left");
                }
                else
                {
                    _chef.MoveRight();
                    Debug.Log("[Input] Tapped right - moving right");
                }
            }
        }

        // Alternative: Use position buttons (for more precise control)
        public void OnPositionButtonPressed(int position)
        {
            _chef?.MoveToPosition(position);
        }

        public void OnSwapButtonPressed()
        {
            _chef?.SwapPlates();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_chef == null) return;

            // Draw tap radius around chef
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(_chef.transform.position, _chefTapRadius);
        }
#endif
    }
}
