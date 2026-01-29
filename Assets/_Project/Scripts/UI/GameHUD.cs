using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DogtorBurguer
{
    public class GameHUD : MonoBehaviour
    {
        private TextMeshProUGUI _scoreText;
        private TextMeshProUGUI _levelText;
        private TextMeshProUGUI _gemText;
        private Canvas _canvas;

        private void Start()
        {
            CreateCanvas();
            CreateHUDElements();
            SubscribeEvents();
            UpdateScore(0);
            UpdateLevel(1);
        }

        private void CreateCanvas()
        {
            GameObject canvasObj = new GameObject("HUD_Canvas");
            canvasObj.transform.SetParent(transform);

            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 50;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = UIStyles.REFERENCE_RESOLUTION;
            scaler.matchWidthOrHeight = UIStyles.MATCH_WIDTH_OR_HEIGHT;

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        private void CreateHUDElements()
        {
            float startY = -10f;

            _scoreText = CreatePanelText("ScoreText", startY);
            _scoreText.fontSize = UIStyles.HUD_SCORE_SIZE;
            _scoreText.fontStyle = FontStyles.Bold;

            _levelText = CreatePanelText("LevelText", startY - UIStyles.HUD_LINE_SPACING);
            _levelText.fontSize = UIStyles.HUD_LEVEL_SIZE;

            _gemText = CreatePanelText("GemText", startY - UIStyles.HUD_LINE_SPACING * 2);
            _gemText.fontSize = UIStyles.HUD_GEM_SIZE;
            _gemText.color = UIStyles.TEXT_HUD;
            int gems = SaveDataManager.Instance != null ? SaveDataManager.Instance.Gems : 0;
            _gemText.text = $"Gems: {gems}";
        }

        private TextMeshProUGUI CreatePanelText(string name, float yOffset)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(_canvas.transform, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.06f, 0.93f);
            rect.anchorMax = new Vector2(0.46f, 0.93f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(10f, yOffset);
            rect.sizeDelta = new Vector2(0, 35);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.color = UIStyles.TEXT_HUD;
            tmp.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            tmp.outlineWidth = UIStyles.OUTLINE_WIDTH_UI;
            tmp.outlineColor = UIStyles.OUTLINE_COLOR;

            return tmp;
        }

        private void SubscribeEvents()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnScoreChanged += UpdateScore;

            DifficultyManager dm = FindAnyObjectByType<DifficultyManager>();
            if (dm != null)
                dm.OnLevelChanged += UpdateLevel;

            if (SaveDataManager.Instance != null)
                SaveDataManager.Instance.OnGemsChanged += UpdateGems;
        }

        private void UpdateScore(int score)
        {
            _scoreText.text = $"Score: {score}";
        }

        private void UpdateLevel(int level)
        {
            _levelText.text = $"Lv.{level}";
        }

        private void UpdateGems(int gems)
        {
            if (_gemText != null)
                _gemText.text = $"Gems: {gems}";
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnScoreChanged -= UpdateScore;

            DifficultyManager dm = FindAnyObjectByType<DifficultyManager>();
            if (dm != null)
                dm.OnLevelChanged -= UpdateLevel;

            if (SaveDataManager.Instance != null)
                SaveDataManager.Instance.OnGemsChanged -= UpdateGems;
        }
    }
}
