using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DogtorBurguer
{
    public class GameHUD : MonoBehaviour
    {
        private TextMeshProUGUI _scoreNumber;
        private TextMeshProUGUI _levelNumber;
        private Canvas _canvas;
        private SettingsPanel _settingsPanel;
        private bool _settingsPausedRun;

        private void Start()
        {
            // Screen Space - Camera so world sprites above order 50 (fairies,
            // popups, screen flash) render in front of the HUD.
            _canvas = UIFactory.CreateCanvas(transform, "HUD_Canvas", 50, Camera.main);
            CreateHUDElements();
            SubscribeEvents();
            RefreshAll();
        }

        private void CreateHUDElements()
        {
            TopBar.Build(_canvas.transform, OnShopClicked, OnConfigClicked);
            Vector2 topLeft = new(0f, 1f);
            _levelNumber = StatCard.Build(_canvas.transform, "LevelPanel", "Level", topLeft, UIStyles.HUD_LEVEL_PANEL_POS);
            _scoreNumber = StatCard.Build(_canvas.transform, "ScorePanel", "Score", topLeft, UIStyles.HUD_SCORE_PANEL_POS);

            // Relax runs carry a small mode tag so halved star popups don't read as a bug.
            bool relax = (SaveDataManager.Instance != null
                ? SaveDataManager.Instance.Mode : SaveDataManager.DEFAULT_GAME_MODE) == GameMode.Relax;
            if (relax)
            {
                TextMeshProUGUI modeTag = UIFactory.CreateText(_canvas.transform, "RELAX MODE", Vector2.zero,
                    new Vector2(240f, 32f), UIStyles.HUD_MODE_TAG_SIZE, FontStyles.Bold);
                UIFactory.StyleFillAndBorder(modeTag, UIStyles.SHOP_ACCENT, UIStyles.HUD_TEXT_BORDER,
                    UIStyles.HUD_TEXT_BORDER_WIDTH);
                RectTransform tagRect = modeTag.rectTransform;
                tagRect.anchorMin = tagRect.anchorMax = new Vector2(0.5f, 1f);
                tagRect.pivot = new Vector2(0.5f, 1f);
                tagRect.anchoredPosition = new Vector2(0f, UIStyles.HUD_MODE_TAG_Y);
            }
        }

        // In-game settings: same pause pattern as the shop — pause a running game, show the
        // panel on its own canvas (above game-over, below the shop), resume when it closes.
        private void OnConfigClicked()
        {
            if (_settingsPanel == null)
            {
                Canvas settingsCanvas = UIFactory.CreateCanvas(transform, "Settings_Canvas", UIStyles.SETTINGS_CANVAS_SORT);
                _settingsPanel = gameObject.AddComponent<SettingsPanel>();
                _settingsPanel.Initialize(settingsCanvas, showRunButtons: true);
                _settingsPanel.OnClosed += HandleSettingsClosed;
            }

            GameManager manager = GameManager.Instance;
            _settingsPausedRun = manager != null && manager.CurrentState == GameState.Playing && !manager.IsPaused;
            if (_settingsPausedRun) manager.PauseGame();
            _settingsPanel.Show();
        }

        private void HandleSettingsClosed()
        {
            if (!_settingsPausedRun) return;

            _settingsPausedRun = false;
            GameManager.Instance?.ResumeGame();
        }

        private void OnShopClicked() { ShopScreen.OpenInGame(); }

        private void SubscribeEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged += UpdateScore;
                GameManager.Instance.OnLevelChanged += UpdateLevel;
            }
        }

        // Seeds every readout from the live sources (single init path, no hardcoded duplicates).
        private void RefreshAll()
        {
            UpdateScore(GameManager.Instance != null ? GameManager.Instance.Score : 0);
            UpdateLevel(GameManager.Instance != null ? GameManager.Instance.CurrentLevel : 1);
        }

        private void UpdateScore(int score)
        {
            if (_scoreNumber != null)
                _scoreNumber.text = NumberFormat.Abbreviate(score);
        }

        private void UpdateLevel(int level)
        {
            if (_levelNumber != null)
                _levelNumber.text = level.ToString();
        }

        private void OnDestroy()
        {
            if (_settingsPanel != null)
                _settingsPanel.OnClosed -= HandleSettingsClosed;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScore;
                GameManager.Instance.OnLevelChanged -= UpdateLevel;
            }
        }
    }
}
