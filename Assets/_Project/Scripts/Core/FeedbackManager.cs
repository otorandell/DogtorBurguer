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

        [Header("Screen Flash")]
        [SerializeField] private Color _flashColor = new Color(1f, 1f, 1f, 0.6f);
        [SerializeField] private float _flashDuration = 0.3f;

        private Tween _shakeTween;
        private Vector3 _cameraOriginalPos;
        private SpriteRenderer _flashRenderer;

        private void Awake()
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;

            _cameraOriginalPos = _mainCamera.transform.position;
            CreateFlashSprite();
        }

        private void CreateFlashSprite()
        {
            GameObject flashObj = new GameObject("ScreenFlash");
            flashObj.transform.SetParent(_mainCamera.transform);
            flashObj.transform.localPosition = new Vector3(0, 0, 1f);

            _flashRenderer = flashObj.AddComponent<SpriteRenderer>();
            _flashRenderer.sprite = CreateWhiteSprite();
            _flashRenderer.sortingOrder = 200;
            _flashRenderer.color = Color.clear;

            // Scale to cover camera view
            float camHeight = _mainCamera.orthographicSize * 2f;
            float camWidth = camHeight * _mainCamera.aspect;
            flashObj.transform.localScale = new Vector3(camWidth + 1f, camHeight + 1f, 1f);
        }

        private Sprite CreateWhiteSprite()
        {
            Texture2D tex = new Texture2D(4, 4);
            Color[] pixels = new Color[16];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
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
            SpawnBurgerPopup(position, points, burgerName);
            ShakeCamera(_burgerShakeStrength);
            FlashScreen();
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

        private void SpawnBurgerPopup(Vector3 position, int points, string burgerName)
        {
            GameObject popupObj = new GameObject("BurgerPopup");
            popupObj.transform.position = position + Vector3.up * 0.5f;

            BurgerPopup popup = popupObj.AddComponent<BurgerPopup>();
            popup.Initialize(burgerName, points, _burgerColor);
        }

        private void FlashScreen()
        {
            if (_flashRenderer == null) return;

            _flashRenderer.DOKill();
            _flashRenderer.color = _flashColor;
            _flashRenderer.DOColor(Color.clear, _flashDuration).SetEase(Ease.OutQuad);
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
            _flashRenderer?.DOKill();
            if (_mainCamera != null)
                _mainCamera.transform.position = _cameraOriginalPos;
        }
    }
}
