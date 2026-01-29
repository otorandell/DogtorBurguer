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
            SpawnScorePopup(position, points, UIStyles.SCORE_POPUP);
            ShakeCamera(AnimConfig.MATCH_SHAKE_STRENGTH);
        }

        private void HandleBurgerEffect(Vector3 position, int points, string burgerName, int ingredientCount)
        {
            SpawnBurgerPopup(position, points, burgerName);
            ShakeCamera(AnimConfig.BURGER_SHAKE_STRENGTH);
            FlashScreen();
        }

        private void SpawnScorePopup(Vector3 position, int points, Color color)
        {
            GameObject popupObj = new GameObject("ScorePopup");
            popupObj.transform.position = position;

            TextMeshPro tmp = popupObj.AddComponent<TextMeshPro>();
            tmp.fontSize = UIStyles.WORLD_SCORE_POPUP_SIZE;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.sortingOrder = 100;
            tmp.outlineWidth = UIStyles.OUTLINE_WIDTH_WORLD;
            tmp.outlineColor = UIStyles.OUTLINE_COLOR;
            tmp.rectTransform.sizeDelta = new Vector2(4f, 2f);

            ScorePopup popup = popupObj.AddComponent<ScorePopup>();
            popup.Initialize(points, color);
        }

        private void SpawnBurgerPopup(Vector3 position, int points, string burgerName)
        {
            GameObject popupObj = new GameObject("BurgerPopup");
            popupObj.transform.position = position + Vector3.up * 0.5f;

            BurgerPopup popup = popupObj.AddComponent<BurgerPopup>();
            popup.Initialize(burgerName, points, UIStyles.BURGER_POPUP);
        }

        private void FlashScreen()
        {
            if (_flashRenderer == null) return;

            DOTween.Kill(_flashRenderer);
            _flashRenderer.color = UIStyles.SCREEN_FLASH;
            _flashRenderer.DOColor(Color.clear, AnimConfig.SCREEN_FLASH_DURATION).SetEase(Ease.OutQuad);
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
            if (_flashRenderer != null)
                DOTween.Kill(_flashRenderer);
            if (_mainCamera != null)
                _mainCamera.transform.position = _cameraOriginalPos;
        }
    }
}
