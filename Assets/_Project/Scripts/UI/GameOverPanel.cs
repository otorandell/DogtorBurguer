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
        private TextMeshProUGUI _starsText;
        private Button _continueGemsButton;
        private GameObject _continueGemsObj;
        private GameObject _continueAdObj;
        private Button _continueAdButton;
        private TextMeshProUGUI _continueGemsText;
        private TextMeshProUGUI _continueAdText;

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
            UIFactory.CreateText(_panel.transform, "GAME OVER", UIStyles.GAMEOVER_TITLE_POS, UIStyles.GAMEOVER_TEXT_RECT,
                UIStyles.GAMEOVER_TITLE_SIZE, FontStyles.Bold);

            _scoreText = UIFactory.CreateText(_panel.transform, "Score: 0", UIStyles.GAMEOVER_SCORE_POS, UIStyles.GAMEOVER_TEXT_RECT,
                UIStyles.PANEL_SCORE_SIZE);

            _levelText = UIFactory.CreateText(_panel.transform, "Level: 1", UIStyles.GAMEOVER_LEVEL_POS, UIStyles.GAMEOVER_TEXT_RECT,
                UIStyles.PANEL_LEVEL_SIZE);

            _starsText = UIFactory.CreateText(_panel.transform, "", UIStyles.GAMEOVER_STARS_POS, UIStyles.GAMEOVER_TEXT_RECT,
                UIStyles.PANEL_LEVEL_SIZE, FontStyles.Bold, UIStyles.GOLD);

            // Continue with gems button
            var gemsBtn = UIFactory.CreateButton(_panel.transform, $"Continue ({MonetizationConfig.CONTINUE_GEM_COST} gems)",
                new Vector2(0, UIStyles.GAMEOVER_BTN_START_Y), UIStyles.PANEL_BUTTON_SIZE, UIStyles.BTN_CONTINUE_GEMS,
                UIStyles.PANEL_BUTTON_TEXT_SIZE, OnContinueGemsClicked);
            _continueGemsObj = gemsBtn.obj;
            _continueGemsButton = gemsBtn.button;
            _continueGemsText = gemsBtn.label;

            // Continue with ad button (availability-driven — see RefreshAdButton)
            var adBtn = UIFactory.CreateButton(_panel.transform, "Watch Ad to Continue",
                new Vector2(0, UIStyles.GAMEOVER_BTN_START_Y + UIStyles.GAMEOVER_BTN_SPACING), UIStyles.PANEL_BUTTON_SIZE, UIStyles.BTN_CONTINUE_AD,
                UIStyles.PANEL_BUTTON_TEXT_SIZE, OnContinueAdClicked);
            _continueAdObj = adBtn.obj;
            _continueAdButton = adBtn.button;
            _continueAdText = adBtn.label;

            // Restart button
            UIFactory.CreateButton(_panel.transform, "Restart",
                new Vector2(0, UIStyles.GAMEOVER_BTN_START_Y + UIStyles.GAMEOVER_BTN_SPACING * 2), UIStyles.PANEL_BUTTON_SIZE, UIStyles.BTN_RESTART,
                UIStyles.PANEL_BUTTON_TEXT_SIZE, OnRestartClicked);

            // Main Menu button
            UIFactory.CreateButton(_panel.transform, "Main Menu",
                new Vector2(0, UIStyles.GAMEOVER_BTN_START_Y + UIStyles.GAMEOVER_BTN_SPACING * 3), UIStyles.PANEL_BUTTON_SIZE, UIStyles.BTN_CLOSE,
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
            int level = GameManager.Instance != null ? GameManager.Instance.CurrentLevel : 1;

            _scoreText.text = $"Score: {score}";
            _levelText.text = $"Level: {level}";
            // High score is persisted by GameManager's game-over flow (F-67), not here.
            // Stars (orders + score payout) are already granted by that flow too — display only.
            int stars = GameManager.Instance != null ? GameManager.Instance.StarsEarnedThisRun : 0;
            _starsText.text = stars > 0 ? $"+{stars} Stars earned!" : "";

            _continueGemsObj.SetActive(!_hasContinued);
            _continueAdObj.SetActive(!_hasContinued);

            if (!_hasContinued && SaveDataManager.Instance != null)
            {
                int gems = SaveDataManager.Instance.Gems;
                _continueGemsButton.interactable = gems >= MonetizationConfig.CONTINUE_GEM_COST;
                _continueGemsText.text = $"Continue ({MonetizationConfig.CONTINUE_GEM_COST} gems)";
            }
            RefreshAdButton();

            _canvas.gameObject.SetActive(true);
            _canvasGroup.alpha = 0;
            _panel.transform.localScale = Vector3.one * AnimConfig.GAMEOVER_START_SCALE;

            DOTween.Sequence()
                .Append(_canvasGroup.DOFade(1f, AnimConfig.GAMEOVER_FADE_DURATION))
                .Join(_panel.transform.DOScale(1f, AnimConfig.GAMEOVER_SCALE_DURATION).SetEase(Ease.OutBack))
                .SetUpdate(true);
        }

        private void Hide()
        {
            _canvas.gameObject.SetActive(false);
        }

        // A rewarded ad can finish loading (or fail) while the panel sits open, so the
        // ad-continue button tracks live availability instead of freezing its Show-time state.
        private void Update()
        {
            if (_canvas == null || !_canvas.gameObject.activeSelf) return;
            RefreshAdButton();
        }

        private void RefreshAdButton()
        {
            if (_continueAdObj == null || !_continueAdObj.activeSelf) return;

            bool available = AdManager.Instance != null && AdManager.Instance.IsRewardedAvailable;
            _continueAdButton.interactable = available;
            _continueAdText.text = available ? "Watch Ad to Continue" : "Ad loading...";
        }

        private void OnContinueGemsClicked()
        {
            if (SaveDataManager.Instance == null) return;
            if (!SaveDataManager.Instance.SpendGems(MonetizationConfig.CONTINUE_GEM_COST)) return;

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
            if (AdManager.Instance != null)
                AdManager.Instance.MaybeShowInterstitial(() => GameManager.Instance?.RestartGame());
            else
                GameManager.Instance?.RestartGame();
        }

        private void OnMenuClicked()
        {
            SceneLoader.LoadMainMenu();
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= HandleStateChanged;

            if (_panel != null) _panel.transform.DOKill();
            if (_canvasGroup != null) _canvasGroup.DOKill();
        }
    }
}
