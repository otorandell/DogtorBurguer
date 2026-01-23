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
        private Button _continueGemsButton;
        private Button _continueAdButton;
        private Button _restartButton;
        private Button _menuButton;
        private GameObject _continueGemsObj;
        private GameObject _continueAdObj;
        private TextMeshProUGUI _continueGemsText;

        private bool _hasContinued;

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

            // Panel (taller to fit more buttons)
            _panel = new GameObject("Panel");
            _panel.transform.SetParent(canvasObj.transform, false);
            RectTransform panelRect = _panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(400, 500);
            Image panelImg = _panel.AddComponent<Image>();
            panelImg.color = new Color(0.15f, 0.15f, 0.2f, 0.95f);

            // Title
            _titleText = CreatePanelText("GAME OVER", 0, 200, 42, FontStyles.Bold);

            // Score
            _scoreText = CreatePanelText("Score: 0", 0, 140, 30, FontStyles.Normal);

            // Level
            _levelText = CreatePanelText("Level: 1", 0, 100, 24, FontStyles.Normal);

            // Continue with gems button
            _continueGemsObj = CreateButton("ContinueGemsBtn", 0, 30,
                new Color(0.9f, 0.7f, 0.1f, 1f), $"Continue ({Constants.CONTINUE_GEM_COST} gems)",
                OnContinueGemsClicked, out _continueGemsButton, out _continueGemsText);

            // Continue with ad button
            _continueAdObj = CreateButton("ContinueAdBtn", 0, -45,
                new Color(0.3f, 0.5f, 0.9f, 1f), "Watch Ad to Continue",
                OnContinueAdClicked, out _continueAdButton, out _);

            // Restart button
            CreateButton("RestartBtn", 0, -120,
                new Color(0.2f, 0.7f, 0.3f, 1f), "Restart",
                OnRestartClicked, out _restartButton, out _);

            // Main Menu button
            CreateButton("MenuBtn", 0, -195,
                new Color(0.5f, 0.5f, 0.5f, 1f), "Main Menu",
                OnMenuClicked, out _menuButton, out _);
        }

        private GameObject CreateButton(string name, float x, float y, Color color, string label,
            UnityEngine.Events.UnityAction onClick, out Button button, out TextMeshProUGUI text)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(_panel.transform, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = new Vector2(x, y);
            btnRect.sizeDelta = new Vector2(320, 55);

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = color;

            button = btnObj.AddComponent<Button>();
            button.targetGraphic = btnImg;
            button.onClick.AddListener(onClick);

            text = CreateChildText(btnObj, label, 0, 0, 20, FontStyles.Bold);
            text.color = Color.white;

            return btnObj;
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
            else if (state == GameState.Playing)
                Hide();
        }

        private void Show()
        {
            int score = GameManager.Instance != null ? GameManager.Instance.Score : 0;
            DifficultyManager dm = FindAnyObjectByType<DifficultyManager>();
            int level = dm != null ? dm.CurrentLevel : 1;

            _scoreText.text = $"Score: {score}";
            _levelText.text = $"Level: {level}";

            // Update high score
            if (SaveDataManager.Instance != null)
                SaveDataManager.Instance.SetHighScore(score);

            // Show/hide continue buttons based on whether already used
            _continueGemsObj.SetActive(!_hasContinued);
            _continueAdObj.SetActive(!_hasContinued);

            // Update gem button text with current balance
            if (!_hasContinued && SaveDataManager.Instance != null)
            {
                int gems = SaveDataManager.Instance.Gems;
                _continueGemsButton.interactable = gems >= Constants.CONTINUE_GEM_COST;
                _continueGemsText.text = $"Continue ({Constants.CONTINUE_GEM_COST} gems)";
            }

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

        private void OnContinueGemsClicked()
        {
            if (SaveDataManager.Instance == null) return;
            if (!SaveDataManager.Instance.SpendGems(Constants.CONTINUE_GEM_COST)) return;

            _hasContinued = true;
            GameManager.Instance?.ContinueGame();
        }

        private void OnContinueAdClicked()
        {
            if (AdManager.Instance == null) return;

            AdManager.Instance.ShowRewarded((success) =>
            {
                if (success)
                {
                    _hasContinued = true;
                    GameManager.Instance?.ContinueGame();
                }
            });
        }

        private void OnRestartClicked()
        {
            // Show interstitial if applicable
            if (SaveDataManager.Instance != null && SaveDataManager.Instance.ShouldShowInterstitial())
            {
                AdManager.Instance?.ShowInterstitial(() =>
                {
                    GameManager.Instance?.RestartGame();
                });
            }
            else
            {
                GameManager.Instance?.RestartGame();
            }
        }

        private void OnMenuClicked()
        {
            SceneLoader.LoadMainMenu();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }
}
