using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DogtorBurguer
{
    public class GameHUD : MonoBehaviour
    {
        private TextMeshProUGUI _scoreNumber;
        private TextMeshProUGUI _levelNumber;
        private TextMeshProUGUI _gemText; // temporary — moves to the top status bar in a later step
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
            _levelNumber = BuildStatPanel("LevelPanel", "Level", UIStyles.HUD_LEVEL_PANEL_POS);
            _scoreNumber = BuildStatPanel("ScorePanel", "Score", UIStyles.HUD_SCORE_PANEL_POS);

            // Temporary gem readout, parked below the panels until the status-bar step.
            float gemY = UIStyles.HUD_LEVEL_PANEL_POS.y - UIStyles.HUD_PANEL_SIZE.y;
            _gemText = CreateHUDText("GemText", gemY);
            _gemText.fontSize = UIStyles.HUD_GEM_SIZE;
        }

        // Builds an authored stat card (9-sliced cream panel + red title tab with a TMP label)
        // and returns the number label.
        private TextMeshProUGUI BuildStatPanel(string name, string title, Vector2 pos)
        {
            Image card = UIFactory.CreateImage(_canvas.transform, name, UiArt.Load("ui_panel_card"),
                new Vector2(0f, 1f), pos, UIStyles.HUD_PANEL_SIZE, sliced: true, ppuMultiplier: UIStyles.HUD_CARD_PPU_MULT);

            Image tab = UIFactory.CreateImage(card.transform, "Title", UiArt.Load("ui_title_tab"),
                new Vector2(0.5f, 0.5f), new Vector2(0f, UIStyles.HUD_PANEL_TITLE_Y), UIStyles.HUD_PANEL_TITLE_SIZE,
                sliced: true, ppuMultiplier: UIStyles.HUD_TITLE_PPU_MULT);

            TextMeshProUGUI titleLabel = UIFactory.CreateText(tab.transform, title, Vector2.zero,
                UIStyles.HUD_PANEL_TITLE_SIZE, UIStyles.HUD_TITLE_LABEL_SIZE, FontStyles.Bold,
                UIStyles.HUD_TITLE_LABEL_COLOR);
            titleLabel.textWrappingMode = TextWrappingModes.NoWrap;

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
            tmp.fontSize = UIStyles.HUD_PANEL_NUMBER_SIZE;
            tmp.fontStyle = FontStyles.Bold;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            return tmp;
        }

        private TextMeshProUGUI CreateHUDText(string name, float yOffset)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(_canvas.transform, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = UIStyles.HUD_ANCHOR_MIN;
            rect.anchorMax = UIStyles.HUD_ANCHOR_MAX;
            rect.pivot = UIStyles.HUD_PIVOT;
            rect.anchoredPosition = new Vector2(UIStyles.HUD_TEXT_X, yOffset);
            rect.sizeDelta = UIStyles.HUD_TEXT_RECT;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = UIStyles.TEXT_HUD;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.outlineWidth = UIStyles.OUTLINE_WIDTH_UI;
            tmp.outlineColor = UIStyles.OUTLINE_COLOR;

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
        }

        private void UpdateScore(int score)
        {
            if (_scoreNumber != null)
                _scoreNumber.text = score.ToString();
        }

        private void UpdateLevel(int level)
        {
            if (_levelNumber != null)
                _levelNumber.text = level.ToString();
        }

        private void UpdateGems(int gems)
        {
            if (_gemText != null)
                _gemText.text = $"Gems: {gems}";
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
