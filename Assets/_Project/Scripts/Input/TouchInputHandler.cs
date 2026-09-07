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
        private PressPhase _press = PressPhase.None;

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
                CancelCarryIfActive();
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
            if (keyboard == null) return;

#if UNITY_EDITOR
            HandleDebugConsumableHotkeys(keyboard);
#endif

            if (_chef == null) return;

            if (TutorialMode.AllowMove)
            {
                if (keyboard.aKey.wasPressedThisFrame)
                    _chef.MoveLeft();
                else if (keyboard.dKey.wasPressedThisFrame)
                    _chef.MoveRight();
            }

            if (TutorialMode.AllowFlip && keyboard.spaceKey.wasPressedThisFrame)
                _chef.SwapPlates();
        }

#if UNITY_EDITOR
        // Debug (editor-only): 1/2/3 grant Ketchup/Mustard/Skewer so the effects can be tested
        // without farming fairies; 4 grants stars so the Shop can be exercised without grinding;
        // F spawns a fairy on demand (render-order/tap testing); V forces game over so that
        // screen can be inspected without dying. Never ships.
        private void HandleDebugConsumableHotkeys(Keyboard keyboard)
        {
            ConsumableInventory inv = ConsumableInventory.Instance;
            if (inv == null) return;

            if (keyboard.digit1Key.wasPressedThisFrame) inv.Add(ConsumableType.Ketchup);
            else if (keyboard.digit2Key.wasPressedThisFrame) inv.Add(ConsumableType.Mustard);
            else if (keyboard.digit3Key.wasPressedThisFrame) inv.Add(ConsumableType.Skewer);
            else if (keyboard.digit4Key.wasPressedThisFrame)
                SaveDataManager.Instance?.AddStars(MonetizationConfig.DEBUG_STAR_GRANT);
            else if (keyboard.fKey.wasPressedThisFrame)
                FindAnyObjectByType<BurgerFairySpawner>()?.DebugSpawn();
            else if (keyboard.vKey.wasPressedThisFrame)
                GameManager.Instance?.DebugTriggerGameOver();
            else if (keyboard.f12Key.wasPressedThisFrame)
            {
                // Store-listing screenshots: captures the Game view at its CURRENT resolution —
                // set it to a fixed 1080x1920 first so Play gets clean 9:16 1080p shots.
                string file = $"Docs/store-assets/screenshot_{System.DateTime.Now:HHmmss}.png";
                UnityEngine.ScreenCapture.CaptureScreenshot(file);
                Debug.Log($"[Screenshot] saved {file}");
            }
        }
#endif

        private void HandleMouseInput()
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.wasPressedThisFrame)
                BeginPress(mouse.position.ReadValue());
            else if (mouse.leftButton.wasReleasedThisFrame)
                EndPress(mouse.position.ReadValue());
            else
                ContinuePress(mouse.position.ReadValue());
        }

        // A swipe resolves the moment the finger crosses the threshold, not on lift — waiting for
        // the release added a full gesture's worth of latency (first device test, 2026-09-06).
        // The press is consumed so the lift can't fire a second move or a tap.
        private void TrySwipeEarly(Vector2 screenPos)
        {
            if (!IsSwipe(_touchStartPos, screenPos, out float deltaX)) return;

            _press = PressPhase.None;
            MoveChefHorizontal(deltaX);
        }

        // A horizontal swipe = past the threshold AND more sideways than vertical. Vertical drags
        // past the threshold are consumed as "not a tap" by ResolveRelease (unchanged).
        private static bool IsSwipeCandidate(Vector2 start, Vector2 end, float threshold) =>
            (end - start).magnitude > threshold;

        private bool IsSwipe(Vector2 start, Vector2 end, out float deltaX)
        {
            Vector2 delta = end - start;
            deltaX = delta.x;
            return IsSwipeCandidate(start, end, _swipeThreshold) && Mathf.Abs(delta.x) > Mathf.Abs(delta.y);
        }

        private void HandleTouchInput()
        {
            Touch touch = Touch.activeTouches[0];

            switch (touch.phase)
            {
                case UnityEngine.InputSystem.TouchPhase.Began:
                    BeginPress(touch.screenPosition);
                    break;

                case UnityEngine.InputSystem.TouchPhase.Moved:
                case UnityEngine.InputSystem.TouchPhase.Stationary:
                    ContinuePress(touch.screenPosition);
                    break;

                case UnityEngine.InputSystem.TouchPhase.Ended:
                case UnityEngine.InputSystem.TouchPhase.Canceled:
                    EndPress(touch.screenPosition);
                    break;
            }
        }

        // ---- The press lifecycle (touch and mouse share it) ----

        // Press: a slot press becomes a carry. Otherwise, in Tap mode the tap intent resolves RIGHT
        // NOW (side-move, swap, fast-drop, preview, fairy) — waiting for the lift read as lag on
        // device (2026-09-06). Drag mode keeps taps on the lift: there a press on the chef or a
        // piece is usually the start of a swipe, and firing on press would swap/fast-drop first.
        private void BeginPress(Vector2 screenPos)
        {
            _touchStartPos = screenPos;
            _press = PressPhase.Open;
            TryBeginCarry(screenPos);
            if (IsCarrying) return;

            if (CurrentControlMode == ControlMode.Tap)
            {
                ResolveTap(screenPos);
                _press = PressPhase.SwipeOnly;
            }
        }

        private void ContinuePress(Vector2 screenPos)
        {
            if (_press == PressPhase.None) return;

            if (IsCarrying)
                UpdateCarry(screenPos);
            else
                TrySwipeEarly(screenPos);
        }

        private void EndPress(Vector2 screenPos)
        {
            if (_press == PressPhase.None) return;
            PressPhase phase = _press;
            _press = PressPhase.None;

            if (IsCarrying)
                ConsumableDragController.Instance.Release(ToWorld(screenPos));
            else
                ResolveRelease(_touchStartPos, screenPos, allowTap: phase == PressPhase.Open);
        }

        private static ControlMode CurrentControlMode =>
            SaveDataManager.Instance != null ? SaveDataManager.Instance.ControlMode : SaveDataManager.DEFAULT_CONTROL_MODE;

        // The lift: a horizontal swipe moves the chef in either mode (normally already fired early
        // by TrySwipeEarly — this catches a swipe that crossed the threshold and lifted within the
        // same frame). Any drag past the threshold is not a tap; a short press is a tap only when
        // the press didn't already resolve it (Drag mode).
        private void ResolveRelease(Vector2 startScreenPos, Vector2 endScreenPos, bool allowTap)
        {
            if (_chef == null) return;

            if (IsSwipeCandidate(startScreenPos, endScreenPos, _swipeThreshold))
            {
                if (IsSwipe(startScreenPos, endScreenPos, out float deltaX))
                    MoveChefHorizontal(deltaX);
                return;
            }

            if (allowTap) ResolveTap(startScreenPos);
        }

        // The tap intent at one screen point: world-object taps (fairy / preview / falling) in both
        // modes, then the mode-specific chef taps.
        private void ResolveTap(Vector2 screenPos)
        {
            if (_chef == null || _camera == null) return;
            Vector3 worldPos = _camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, ScreenToWorldZ));
            worldPos.z = 0f;

            // Fairy first: it's a transient flying reward rendered on top of the playfield, so a tap
            // on it should collect it rather than be eaten by the preview/falling hit-tests beneath.
            if (BurgerFairy.TryTapAt(worldPos)) return;
            if (_spawner != null && _spawner.TryTapPreview(worldPos)) return;
            if (TutorialMode.AllowFastDrop && _spawner != null && _spawner.TryTapFallingIngredient(worldPos)) return;

            // Only the remaining tap intent depends on the control mode.
            ControlMode mode = CurrentControlMode;

            // Tapping the chef swaps plates — same in both modes. The radius bounds it, so a tap up
            // in the playfield (e.g. a near-miss on a falling piece) never swaps the cook.
            float chefDist = Vector2.Distance(worldPos, _chef.transform.position);
            bool tappedChef = chefDist < _chef.BubbleRadius * GameplayConfig.CHEF_TAP_RADIUS_MULT;

            if (mode == ControlMode.Drag)
            {
                // Drag mode moves by swiping (handled above); a tap only swaps, and only on the chef.
                if (tappedChef && TutorialMode.AllowFlip)
                    _chef.SwapPlates();
                return;
            }

            // Tap mode: tap the chef to swap, or tap to a side (below the playfield) to move there.
            if (tappedChef)
            {
                if (TutorialMode.AllowFlip)
                    _chef.SwapPlates();
            }
            else if (TutorialMode.AllowMove &&
                     worldPos.y < Constants.GRID_ORIGIN_Y + GameplayConfig.CHEF_MOVE_ZONE_TOP_OFFSET)
            {
                if (worldPos.x < _chef.transform.position.x)
                    _chef.MoveLeft();
                else
                    _chef.MoveRight();
            }
        }

        private void MoveChefHorizontal(float deltaX)
        {
            if (!TutorialMode.AllowMove) return;
            if (deltaX > 0)
                _chef.MoveRight();
            else
                _chef.MoveLeft();
        }

        public void OnSwapButtonPressed()
        {
            if (TutorialMode.AllowFlip)
                _chef?.SwapPlates();
        }

        // ---- Consumable carry (drag-to-column) ----
        // A press that starts on an inventory slot becomes a carry; the drag controller owns the
        // gesture for its duration and chef logic is suppressed (we route to Release, not
        // ResolveTap). Origin disambiguates: a press anywhere else stays normal gameplay.

        private bool IsCarrying =>
            ConsumableDragController.Instance != null && ConsumableDragController.Instance.IsCarrying;

        private void TryBeginCarry(Vector2 screenPos)
        {
            // Slots are screen-space UGUI, so TryBegin hit-tests the raw screen position; once a carry
            // starts, position the icon immediately (world space) so it doesn't flash at the origin.
            if (!TutorialMode.AllowConsumable) return;
            ConsumableDragController drag = ConsumableDragController.Instance;
            if (drag != null && drag.TryBegin(screenPos))
                drag.UpdateCarry(ToWorld(screenPos));
        }

        private void UpdateCarry(Vector2 screenPos)
        {
            ConsumableDragController.Instance?.UpdateCarry(ToWorld(screenPos));
        }

        private void CancelCarryIfActive()
        {
            if (IsCarrying)
                ConsumableDragController.Instance.Cancel();
        }

        private Vector3 ToWorld(Vector2 screenPos)
        {
            if (_camera == null) return Vector3.zero;
            Vector3 world = _camera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, ScreenToWorldZ));
            world.z = 0f;
            return world;
        }

#if UNITY_EDITOR
        // Debug gizmos for the chef's tap hit-zones (mirrors ResolveTap), mode-aware. Magenta =
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
            float top = Constants.GRID_ORIGIN_Y + GameplayConfig.CHEF_MOVE_ZONE_TOP_OFFSET;
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
