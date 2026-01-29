using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace DogtorBurguer
{
    public class GameOverPanel : MonoBehaviour
    {
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private GameObject _panel;
        private TextMeshProUGUI _scoreText;
        private TextMeshProUGUI _levelText;
        private Button _continueGemsButton;
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
            _canvas = UIFactory.CreateCanvas(transform, "GameOver_Canvas", 100);
            _canvasGroup = _canvas.gameObject.AddComponent<CanvasGroup>();
            UIFactory.EnsureEventSystem();

            // Dark overlay
            UIFactory.CreateOverlay(_canvas.transform, UIStyles.OVERLAY_DIM);

            // Panel
            _panel = UIFactory.CreatePanel(_canvas.transform, UIStyles.GAMEOVER_PANEL_SIZE, UIStyles.PANEL_BG);

            // Title, score, level
            UIFactory.CreateText(_panel.transform, "GAME OVER", new Vector2(0, 200), new Vector2(350, 50),
                UIStyles.GAMEOVER_TITLE_SIZE, FontStyles.Bold);

            _scoreText = UIFactory.CreateText(_panel.transform, "Score: 0", new Vector2(0, 140), new Vector2(350, 50),
                UIStyles.PANEL_SCORE_SIZE);

            _levelText = UIFactory.CreateText(_panel.transform, "Level: 1", new Vector2(0, 100), new Vector2(350, 50),
                UIStyles.PANEL_LEVEL_SIZE);

            // Continue with gems button
            var gemsBtn = UIFactory.CreateButton(_panel.transform, $"Continue ({Constants.CONTINUE_GEM_COST} gems)",
                new Vector2(0, 30), UIStyles.PANEL_BUTTON_SIZE, UIStyles.BTN_CONTINUE_GEMS,
                UIStyles.PANEL_BUTTON_TEXT_SIZE, OnContinueGemsClicked);
            _continueGemsObj = gemsBtn.obj;
            _continueGemsButton = gemsBtn.button;
            _continueGemsText = gemsBtn.label;

            // Continue with ad button
            var adBtn = UIFactory.CreateButton(_panel.transform, "Watch Ad to Continue",
                new Vector2(0, -45), UIStyles.PANEL_BUTTON_SIZE, UIStyles.BTN_CONTINUE_AD,
                UIStyles.PANEL_BUTTON_TEXT_SIZE, OnContinueAdClicked);
            _continueAdObj = adBtn.obj;

            // Restart button
            UIFactory.CreateButton(_panel.transform, "Restart",
                new Vector2(0, -120), UIStyles.PANEL_BUTTON_SIZE, UIStyles.BTN_RESTART,
                UIStyles.PANEL_BUTTON_TEXT_SIZE, OnRestartClicked);

            // Main Menu button
            UIFactory.CreateButton(_panel.transform, "Main Menu",
                new Vector2(0, -195), UIStyles.PANEL_BUTTON_SIZE, UIStyles.BTN_CLOSE,
                UIStyles.PANEL_BUTTON_TEXT_SIZE, OnMenuClicked);
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

            if (SaveDataManager.Instance != null)
                SaveDataManager.Instance.SetHighScore(score);

            _continueGemsObj.SetActive(!_hasContinued);
            _continueAdObj.SetActive(!_hasContinued);

            if (!_hasContinued && SaveDataManager.Instance != null)
            {
                int gems = SaveDataManager.Instance.Gems;
                _continueGemsButton.interactable = gems >= Constants.CONTINUE_GEM_COST;
                _continueGemsText.text = $"Continue ({Constants.CONTINUE_GEM_COST} gems)";
            }

            _canvas.gameObject.SetActive(true);
            _canvasGroup.alpha = 0;
            _panel.transform.localScale = Vector3.one * AnimConfig.GAMEOVER_START_SCALE;

            DOTween.Sequence()
                .Append(DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 1f, AnimConfig.GAMEOVER_FADE_DURATION))
                .Join(_panel.transform.DOScale(1f, AnimConfig.GAMEOVER_SCALE_DURATION).SetEase(Ease.OutBack))
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
