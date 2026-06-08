using UnityEngine;
using DG.Tweening;

namespace DogtorBurguer
{
    /// <summary>
    /// Routes gameplay events to their feedback: score/burger popups, camera shake, and a
    /// screen flash. Construction of those visuals lives in their own types (ScorePopup /
    /// BurgerPopup Spawn factories, ScreenFlashOverlay) — this is just the orchestration (F-18).
    /// </summary>
    public class FeedbackManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private Camera _mainCamera;

        private Tween _shakeTween;
        private Vector3 _cameraOriginalPos;
        private ScreenFlashOverlay _flash;

        private void Awake()
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            _cameraOriginalPos = _mainCamera.transform.position;

            GameObject flashObj = new GameObject("ScreenFlash");
            _flash = flashObj.AddComponent<ScreenFlashOverlay>();
            _flash.Initialize(_mainCamera);
        }

        private void OnEnable()
        {
            if (_gridManager != null)
            {
                _gridManager.OnMatchEffect += HandleMatchEffect;
                _gridManager.OnBurgerEffect += HandleBurgerEffect;
            }
        }

        private void OnDisable()
        {
            if (_gridManager != null)
            {
                _gridManager.OnMatchEffect -= HandleMatchEffect;
                _gridManager.OnBurgerEffect -= HandleBurgerEffect;
            }
        }

        private void HandleMatchEffect(Vector3 position, int points)
        {
            ScorePopup.Spawn(position, points, UIStyles.SCORE_POPUP);
            ShakeCamera(AnimConfig.MATCH_SHAKE_STRENGTH);
        }

        private void HandleBurgerEffect(Vector3 position, int points, string burgerName, int ingredientCount)
        {
            BurgerPopup.Spawn(position + Vector3.up * 0.5f, points, burgerName, UIStyles.BURGER_POPUP);
            ShakeCamera(AnimConfig.BURGER_SHAKE_STRENGTH);
            _flash.Trigger();
        }

        private void ShakeCamera(float strength)
        {
            _shakeTween?.Kill();
            _mainCamera.transform.position = _cameraOriginalPos;
            _shakeTween = _mainCamera.transform
                .DOShakePosition(AnimConfig.SCREEN_SHAKE_DURATION, strength, 10, 90, false)
                .OnComplete(() => _mainCamera.transform.position = _cameraOriginalPos);
        }

        private void OnDestroy()
        {
            _shakeTween?.Kill();
            if (_mainCamera != null)
                _mainCamera.transform.position = _cameraOriginalPos;
        }
    }
}
