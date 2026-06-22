using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DogtorBurguer
{
    public class GameHUD : MonoBehaviour
    {
        private TextMeshProUGUI _scoreNumber;
        private TextMeshProUGUI _levelNumber;
        private TextMeshProUGUI _gemNumber;
        private TextMeshProUGUI _highScoreNumber;
        private TextMeshProUGUI _starNumber;
        private Canvas _canvas;

        private void Start()
        {
            _canvas = UIFactory.CreateCanvas(transform, "HUD_Canvas", 50);
            CreateHUDElements();
            SubscribeEvents();
            RefreshAll();
        }

        private void CreateHUDElements()
        {
            CreateTopBar();
            _levelNumber = BuildStatPanel("LevelPanel", "Level", UIStyles.HUD_LEVEL_PANEL_POS);
            _scoreNumber = BuildStatPanel("ScorePanel", "Score", UIStyles.HUD_SCORE_PANEL_POS);
        }

        // Builds the top status bar: star + gem + high-score currency widgets, plus settings/shop buttons.
        private void CreateTopBar()
        {
            // Star isn't a real currency yet (seeded to 0) — widget added now, wired soon.
            _starNumber = BuildCurrencyWidget("Stars", "ui_star", UIStyles.TOPBAR_STAR_POS);
            _gemNumber = BuildCurrencyWidget("Gems", "ui_gem", UIStyles.TOPBAR_GEM_POS);
            _highScoreNumber = BuildCurrencyWidget("HighScore", "ui_score_trophy", UIStyles.TOPBAR_SCORE_POS);

            // TODO: in-game shop/settings need pause + panel wiring (panels currently only exist in
            // the menu scene). Visual placeholders for now so the bar layout is complete.
            UIFactory.CreateSpriteButton(_canvas.transform, "ConfigButton", UiArt.Load("ui_config_button"),
                new Vector2(0f, 1f), UIStyles.TOPBAR_CONFIG_POS, UIStyles.TOPBAR_BUTTON_SIZE, OnConfigClicked);
            UIFactory.CreateSpriteButton(_canvas.transform, "ShopButton", UiArt.Load("ui_shop_button"),
                new Vector2(0f, 1f), UIStyles.TOPBAR_SHOP_POS, UIStyles.TOPBAR_BUTTON_SIZE, OnShopClicked);
        }

        // A currency pill (baked box) with an overhanging icon and a number; returns the number label.
        private TextMeshProUGUI BuildCurrencyWidget(string name, string iconArt, Vector2 pos)
        {
            Image box = UIFactory.CreateImage(_canvas.transform, name, UiArt.Load("ui_currency_box"),
                new Vector2(0f, 1f), pos, UIStyles.TOPBAR_BOX_SIZE);

            UIFactory.CreateImage(box.transform, "Icon", UiArt.Load(iconArt),
                new Vector2(0.5f, 0.5f), new Vector2(UIStyles.TOPBAR_ICON_X, 0f), UIStyles.TOPBAR_ICON_SIZE);

            TextMeshProUGUI number = UIFactory.CreateText(box.transform, "0",
                new Vector2(UIStyles.TOPBAR_NUMBER_X, 0f), UIStyles.TOPBAR_NUMBER_RECT,
                UIStyles.TOPBAR_NUMBER_SIZE, FontStyles.Bold, UIStyles.HUD_PANEL_NUMBER_COLOR);
            AutoFit(number, UIStyles.TOPBAR_NUMBER_SIZE_MIN, UIStyles.TOPBAR_NUMBER_SIZE);
            return number;
        }

        // Shrink-to-fit: the box stays fixed and the text scales down to stay inside it.
        private static void AutoFit(TextMeshProUGUI tmp, float min, float max)
        {
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = min;
            tmp.fontSizeMax = max;
        }

        private void OnConfigClicked() { Debug.Log("[GameHUD] Config button — in-game settings not wired yet."); }
        private void OnShopClicked() { Debug.Log("[GameHUD] Shop button — in-game shop not wired yet."); }

        // Builds an authored stat card (dotted cream panel + blank red title tab with the word as
        // TMP) and returns the number label. The blank-tab + TMP is a placeholder until the artist's
        // final per-word tab art arrives.
        private TextMeshProUGUI BuildStatPanel(string name, string title, Vector2 pos)
        {
            Image card = UIFactory.CreateImage(_canvas.transform, name, UiArt.Load("ui_panel_card"),
                new Vector2(0f, 1f), pos, UIStyles.HUD_PANEL_SIZE);

            Image tab = UIFactory.CreateImage(card.transform, "Title", UiArt.Load("ui_title_tab"),
                new Vector2(0.5f, 0.5f), new Vector2(0f, UIStyles.HUD_PANEL_TITLE_Y), UIStyles.HUD_PANEL_TITLE_SIZE);

            TextMeshProUGUI titleLabel = UIFactory.CreateText(tab.transform, title, Vector2.zero,
                UIStyles.HUD_PANEL_TITLE_SIZE, UIStyles.HUD_TITLE_LABEL_SIZE, FontStyles.Bold,
                UIStyles.HUD_TITLE_LABEL_COLOR);
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
            tmp.color = UIStyles.HUD_PANEL_NUMBER_COLOR;
            tmp.fontStyle = FontStyles.Bold;
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

            if (SaveDataManager.Instance != null)
                SaveDataManager.Instance.OnGemsChanged += UpdateGems;
        }

        // Seeds every readout from the live sources (single init path, no hardcoded duplicates).
        private void RefreshAll()
        {
            UpdateScore(GameManager.Instance != null ? GameManager.Instance.Score : 0);
            UpdateLevel(GameManager.Instance != null ? GameManager.Instance.CurrentLevel : 1);
            UpdateGems(SaveDataManager.Instance != null ? SaveDataManager.Instance.Gems : 0);
            // High score only changes at game over, so a one-time seed is enough (no live event).
            if (_highScoreNumber != null)
                _highScoreNumber.text = NumberFormat.Abbreviate(SaveDataManager.Instance != null ? SaveDataManager.Instance.HighScore : 0);
            // Stars aren't a real currency yet — placeholder 0 until the system is built.
            if (_starNumber != null)
                _starNumber.text = "0";
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

        private void UpdateGems(int gems)
        {
            if (_gemNumber != null)
                _gemNumber.text = NumberFormat.Abbreviate(gems);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnScoreChanged -= UpdateScore;
                GameManager.Instance.OnLevelChanged -= UpdateLevel;
            }

            if (SaveDataManager.Instance != null)
                SaveDataManager.Instance.OnGemsChanged -= UpdateGems;
        }
    }
}
