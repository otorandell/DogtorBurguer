using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using DG.Tweening;

namespace DogtorBurguer
{
    public class GameOverPanel : MonoBehaviour
    {
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private GameObject _panel;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _scoreText;
        private TextMeshProUGUI _levelText;
        private Button _restartButton;

        private void Start()
        {
            CreateUI();
            Hide();

            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += HandleStateChanged;
        }

        private void CreateUI()
        {
            // Canvas
            GameObject canvasObj = new GameObject("GameOver_Canvas");
            canvasObj.transform.SetParent(transform);

            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(540, 960);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
            _canvasGroup = canvasObj.AddComponent<CanvasGroup>();

            // Ensure EventSystem exists for button input
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<InputSystemUIInputModule>();
            }

            // Dark overlay
            GameObject overlay = new GameObject("Overlay");
            overlay.transform.SetParent(canvasObj.transform, false);
            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;
            Image overlayImg = overlay.AddComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.7f);

            // Panel
            _panel = new GameObject("Panel");
            _panel.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRect = _panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(400, 350);
            Image panelImg = _panel.AddComponent<Image>();
            panelImg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // Title
            _titleText = CreatePanelText("GAME OVER", 0, 100, 42, FontStyles.Bold);

            // Score
            _scoreText = CreatePanelText("Score: 0", 0, 20, 30, FontStyles.Normal);

            // Level
            _levelText = CreatePanelText("Level: 1", 0, -30, 24, FontStyles.Normal);

            // Restart button
            GameObject btnObj = new GameObject("RestartButton");
            btnObj.transform.SetParent(_panel.transform, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = new Vector2(0, -110);
            btnRect.sizeDelta = new Vector2(200, 60);

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.7f, 0.3f, 1f);

            _restartButton = btnObj.AddComponent<Button>();
            _restartButton.targetGraphic = btnImg;
            _restartButton.onClick.AddListener(OnRestartClicked);

            TextMeshProUGUI btnText = CreateChildText(btnObj, "Restart", 0, 0, 24, FontStyles.Bold);
            btnText.color = Color.white;
        }

        private TextMeshProUGUI CreatePanelText(string text, float x, float y, float size, FontStyles style)
        {
            GameObject textObj = new GameObject(text);
            textObj.transform.SetParent(_panel.transform, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(350, 50);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return tmp;
        }

        private TextMeshProUGUI CreateChildText(GameObject parent, string text, float x, float y, float size, FontStyles style)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent.transform, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;

            return tmp;
        }

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.GameOver)
                Show();
        }

        private void Show()
        {
            int score = GameManager.Instance != null ? GameManager.Instance.Score : 0;
            DifficultyManager dm = FindAnyObjectByType<DifficultyManager>();
            int level = dm != null ? dm.CurrentLevel : 1;

            _scoreText.text = $"Score: {score}";
            _levelText.text = $"Level: {level}";

            _canvas.gameObject.SetActive(true);
            _canvasGroup.alpha = 0;
            _panel.transform.localScale = Vector3.one * 0.5f;

            DOTween.Sequence()
                .Append(DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 1f, 0.3f))
                .Join(_panel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack))
                .SetUpdate(true);
        }

        private void Hide()
        {
            _canvas.gameObject.SetActive(false);
        }

        private void OnRestartClicked()
        {
            GameManager.Instance?.RestartGame();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }
}
