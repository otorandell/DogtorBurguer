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
            _canvas = UIFactory.CreateCanvas(transform, "HUD_Canvas", 50);
            CreateHUDElements();
            SubscribeEvents();
            RefreshAll();
        }

        private void CreateHUDElements()
        {
            TopBar.Build(_canvas.transform, OnShopClicked, OnConfigClicked);
            _levelNumber = BuildStatPanel("LevelPanel", "Level", UIStyles.HUD_LEVEL_PANEL_POS);
            _scoreNumber = BuildStatPanel("ScorePanel", "Score", UIStyles.HUD_SCORE_PANEL_POS);
        }

        // Shrink-to-fit: the box stays fixed and the text scales down to stay inside it.
        private static void AutoFit(TextMeshProUGUI tmp, float min, float max)
        {
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = min;
            tmp.fontSizeMax = max;
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

        // Builds an authored stat card (dotted cream panel + blank red title tab with the word as
        // TMP) and returns the number label. The blank-tab + TMP is a placeholder until the artist's
        // final per-word tab art arrives.
        private TextMeshProUGUI BuildStatPanel(string name, string title, Vector2 pos)
        {
            Image card = UIFactory.CreateImage(_canvas.transform, name, UiArt.Load("ui_panel_card"),
                new Vector2(0f, 1f), pos, UIStyles.HUD_PANEL_SIZE);

            // Size the tab by height, width following native aspect — never force a size (distorts).
            Sprite tabSprite = UiArt.Load("ui_title_tab");
            float tabAspect = tabSprite != null ? tabSprite.rect.width / tabSprite.rect.height : 1f;
            Vector2 tabSize = new(UIStyles.HUD_PANEL_TITLE_HEIGHT * tabAspect, UIStyles.HUD_PANEL_TITLE_HEIGHT);
            Image tab = UIFactory.CreateImage(card.transform, "Title", tabSprite,
                new Vector2(0.5f, 0.5f), new Vector2(0f, UIStyles.HUD_PANEL_TITLE_Y), tabSize);

            TextMeshProUGUI titleLabel = UIFactory.CreateText(tab.transform, title, Vector2.zero,
                tabSize, UIStyles.HUD_TITLE_LABEL_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(titleLabel);
            AutoFit(titleLabel, UIStyles.HUD_TITLE_LABEL_SIZE_MIN, UIStyles.HUD_TITLE_LABEL_SIZE);

            GameObject numObj = new GameObject("Number");
            numObj.transform.SetParent(card.transform, false);
            RectTransform rect = numObj.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, UIStyles.HUD_PANEL_NUMBER_Y);
            rect.sizeDelta = UIStyles.HUD_PANEL_SIZE;

            TextMeshProUGUI tmp = numObj.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            UIFactory.StyleHudText(tmp);
            AutoFit(tmp, UIStyles.HUD_PANEL_NUMBER_SIZE_MIN, UIStyles.HUD_PANEL_NUMBER_SIZE);
            return tmp;
        }

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
