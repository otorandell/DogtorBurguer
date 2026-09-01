using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace DogtorBurguer
{
    /// <summary>
    /// The Game Over screen, built from the authored art (drop №3): the full-canvas panel (red
    /// title bar, cream body and the "Continue" band are baked in), the same Level/Score stat
    /// cards as the HUD (StatCard), the gem / watch-ad continue pair, and Main Menu / Retry.
    /// Layout knobs: UIStyles.GAMEOVER_*.
    /// </summary>
    public class GameOverPanel : MonoBehaviour
    {
        private static readonly Vector2 Center = new(0.5f, 0.5f);

        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private GameObject _panel;
        private TextMeshProUGUI _levelNumber;
        private TextMeshProUGUI _scoreNumber;
        private TextMeshProUGUI _starsText;
        private TextMeshProUGUI _continueLabel;
        private Button _continueGemsButton;
        private Button _continueAdButton;
        private TextMeshProUGUI _continueAdLabel;

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

            UIFactory.CreateOverlay(_canvas.transform, UIStyles.MODAL_OVERLAY);

            // The panel art is a full-phone canvas: shown at the reference resolution it lands exactly
            // where the artist drew it. Everything else is a child so the pop-in scales the whole screen.
            Image panel = UIFactory.CreateImage(_canvas.transform, "Panel", UiArt.Load("ui_gameover_panel"),
                Center, Vector2.zero, UIStyles.REFERENCE_RESOLUTION);
            _panel = panel.gameObject;
            Transform root = panel.transform;

            TextMeshProUGUI title = UIFactory.CreateText(root, "GAME OVER...", UIStyles.GAMEOVER_TITLE_POS,
                UIStyles.GAMEOVER_TITLE_RECT, UIStyles.GAMEOVER_TITLE_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(title);

            _levelNumber = StatCard.Build(root, "LevelCard", "Level", Center,
                UIStyles.GAMEOVER_LEVEL_CARD_POS, UIStyles.GAMEOVER_CARD_SCALE);
            _scoreNumber = StatCard.Build(root, "ScoreCard", "Score", Center,
                UIStyles.GAMEOVER_SCORE_CARD_POS, UIStyles.GAMEOVER_CARD_SCALE);

            BuildContinueRow(root);
            BuildNavRow(root);

            // The run's star haul, called out under the panel (gold, HUD border).
            _starsText = UIFactory.CreateText(root, "StarsEarned", UIStyles.GAMEOVER_STARS_POS,
                UIStyles.GAMEOVER_STARS_RECT, UIStyles.GAMEOVER_STARS_SIZE, FontStyles.Bold);
            UIFactory.StyleFillAndBorder(_starsText, UIStyles.GOLD, UIStyles.HUD_TEXT_BORDER, UIStyles.HUD_TEXT_BORDER_WIDTH);
        }

        // The "Continue" heading plus the gem-cost (cream) and watch-ad (blue, TV icon baked in) buttons.
        private void BuildContinueRow(Transform root)
        {
            _continueLabel = UIFactory.CreateText(root, "Continue", UIStyles.GAMEOVER_CONTINUE_LABEL_POS,
                UIStyles.GAMEOVER_CONTINUE_LABEL_RECT, UIStyles.GAMEOVER_CONTINUE_LABEL_SIZE, FontStyles.Bold);
            UIFactory.StyleFillAndBorder(_continueLabel, UIStyles.TOPBAR_NUMBER_COLOR,
                UIStyles.GAMEOVER_CONTINUE_BORDER, UIStyles.GAMEOVER_CONTINUE_BORDER_WIDTH);

            Sprite cream = UiArt.Load("ui_btn_cream");
            _continueGemsButton = UIFactory.CreateSpriteButton(root, "ContinueGems", cream, Center,
                new Vector2(-UIStyles.GAMEOVER_CONTINUE_BTN_X, UIStyles.GAMEOVER_CONTINUE_BTN_Y),
                UIFactory.SizeByWidth(cream, UIStyles.GAMEOVER_CONTINUE_BTN_W), OnContinueGemsClicked);
            Sprite gem = UiArt.Load("ui_gem");
            UIFactory.CreateImage(_continueGemsButton.transform, "Icon", gem, Center,
                new Vector2(UIStyles.GAMEOVER_GEM_ICON_X, 0f), UIFactory.SizeByHeight(gem, UIStyles.GAMEOVER_GEM_ICON_H));
            TextMeshProUGUI cost = UIFactory.CreateText(_continueGemsButton.transform,
                MonetizationConfig.CONTINUE_GEM_COST.ToString(), UIStyles.GAMEOVER_GEM_COST_POS,
                UIStyles.GAMEOVER_GEM_COST_RECT, UIStyles.GAMEOVER_GEM_COST_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(cost);

            Sprite blue = UiArt.Load("ui_btn_blue_watch");
            _continueAdButton = UIFactory.CreateSpriteButton(root, "ContinueAd", blue, Center,
                new Vector2(UIStyles.GAMEOVER_CONTINUE_BTN_X, UIStyles.GAMEOVER_CONTINUE_BTN_Y),
                UIFactory.SizeByWidth(blue, UIStyles.GAMEOVER_CONTINUE_BTN_W), OnContinueAdClicked);
            _continueAdLabel = UIFactory.CreateText(_continueAdButton.transform, "Watch", UIStyles.GAMEOVER_WATCH_LABEL_POS,
                UIStyles.GAMEOVER_WATCH_LABEL_RECT, UIStyles.GAMEOVER_WATCH_LABEL_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(_continueAdLabel);
            UIFactory.AutoFit(_continueAdLabel, UIStyles.GAMEOVER_WATCH_LABEL_SIZE_MIN, UIStyles.GAMEOVER_WATCH_LABEL_SIZE);
        }

        private void BuildNavRow(Transform root)
        {
            CreateNavButton(root, "MainMenu", "Main\nMenu", "ui_btn_green", -UIStyles.GAMEOVER_NAV_BTN_X, OnMenuClicked);
            CreateNavButton(root, "Retry", "Retry", "ui_btn_yellow", UIStyles.GAMEOVER_NAV_BTN_X, OnRestartClicked);
        }

        // An authored blank with a HUD-palette word on it.
        private static void CreateNavButton(Transform root, string name, string label, string art, float x, UnityAction onClick)
        {
            Sprite sprite = UiArt.Load(art);
            Button btn = UIFactory.CreateSpriteButton(root, name, sprite, Center,
                new Vector2(x, UIStyles.GAMEOVER_NAV_BTN_Y), UIFactory.SizeByWidth(sprite, UIStyles.GAMEOVER_NAV_BTN_W), onClick);
            TextMeshProUGUI word = UIFactory.CreateText(btn.transform, label, UIStyles.GAMEOVER_NAV_LABEL_NUDGE,
                UIStyles.GAMEOVER_NAV_LABEL_RECT, UIStyles.GAMEOVER_NAV_LABEL_SIZE, FontStyles.Bold);
            word.gameObject.name = "Label";
            UIFactory.StyleHudText(word);
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
            GameManager manager = GameManager.Instance;
            _levelNumber.text = (manager != null ? manager.CurrentLevel : 1).ToString();
            _scoreNumber.text = NumberFormat.Abbreviate(manager != null ? manager.Score : 0);
            // High score is persisted by GameManager's game-over flow (F-67), not here.
            // Stars (orders + score payout) are already granted by that flow too — display only.
            int stars = manager != null ? manager.StarsEarnedThisRun : 0;
            _starsText.text = stars > 0 ? $"{stars} stars earned!" : "";

            // One continue per run: afterwards the band keeps its heading but loses the buttons.
            bool canContinue = !_hasContinued;
            _continueLabel.text = canContinue ? "Continue" : "No more continues";
            _continueGemsButton.gameObject.SetActive(canContinue);
            _continueAdButton.gameObject.SetActive(canContinue);
            if (canContinue && SaveDataManager.Instance != null)
                _continueGemsButton.interactable = SaveDataManager.Instance.Gems >= MonetizationConfig.CONTINUE_GEM_COST;
            RefreshAdButton();

            _canvas.gameObject.SetActive(true);
            _canvasGroup.alpha = 0;
            _panel.transform.localScale = Vector3.one * AnimConfig.PANEL_START_SCALE;

            DOTween.Sequence()
                .Append(_canvasGroup.DOFade(1f, AnimConfig.PANEL_FADE_DURATION))
                .Join(_panel.transform.DOScale(1f, AnimConfig.PANEL_SCALE_DURATION).SetEase(Ease.OutBack))
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
            if (_continueAdButton == null || !_continueAdButton.gameObject.activeSelf) return;

            bool available = AdManager.Instance != null && AdManager.Instance.IsRewardedAvailable;
            _continueAdButton.interactable = available;
            _continueAdLabel.text = available ? "Watch" : "Loading...";
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
