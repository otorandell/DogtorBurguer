using UnityEngine;
using DG.Tweening;
using TMPro;

namespace DogtorBurguer
{
    public class FeedbackManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GridManager _gridManager;
        [SerializeField] private Camera _mainCamera;

        [Header("Score Popup")]
        [SerializeField] private Color _matchColor = Color.yellow;
        [SerializeField] private Color _burgerColor = new Color(1f, 0.5f, 0f); // Orange
        [SerializeField] private float _popupFontSize = 5f;

        [Header("Screen Shake")]
        [SerializeField] private float _matchShakeStrength = 0.15f;
        [SerializeField] private float _burgerShakeStrength = 0.3f;
        [SerializeField] private float _shakeDuration = 0.2f;

        private Tween _shakeTween;
        private Vector3 _cameraOriginalPos;

        private void Awake()
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            _cameraOriginalPos = _mainCamera.transform.position;
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
            SpawnScorePopup(position, points, _matchColor);
            ShakeCamera(_matchShakeStrength);
        }

        private void HandleBurgerEffect(Vector3 position, int points, string burgerName)
        {
            SpawnScorePopup(position, points, _burgerColor);
            ShakeCamera(_burgerShakeStrength);
        }

        private void SpawnScorePopup(Vector3 position, int points, Color color)
        {
            GameObject popupObj = new GameObject("ScorePopup");
            popupObj.transform.position = position;

            TextMeshPro tmp = popupObj.AddComponent<TextMeshPro>();
            tmp.fontSize = _popupFontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.sortingOrder = 100;
            tmp.rectTransform.sizeDelta = new Vector2(4f, 2f);

            ScorePopup popup = popupObj.AddComponent<ScorePopup>();
            popup.Initialize(points, color);
        }

        private void ShakeCamera(float strength)
        {
            _shakeTween?.Kill();
            _mainCamera.transform.position = _cameraOriginalPos;
            _shakeTween = _mainCamera.transform
                .DOShakePosition(_shakeDuration, strength, 10, 90, false)
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
