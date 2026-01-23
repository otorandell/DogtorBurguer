using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DogtorBurguer
{
    public class GameHUD : MonoBehaviour
    {
        private TextMeshProUGUI _scoreText;
        private TextMeshProUGUI _levelText;
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
            scaler.referenceResolution = new Vector2(540, 960);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        private void CreateHUDElements()
        {
            // Score text (top-left)
            _scoreText = CreateText("ScoreText", new Vector2(20, -20), TextAlignmentOptions.TopLeft);
            _scoreText.fontSize = 28;
            _scoreText.fontStyle = FontStyles.Bold;

            // Level text (top-right)
            _levelText = CreateText("LevelText", new Vector2(-20, -20), TextAlignmentOptions.TopRight);
            _levelText.fontSize = 24;
        }

        private TextMeshProUGUI CreateText(string name, Vector2 offset, TextAlignmentOptions alignment)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(_canvas.transform, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();

            if (alignment == TextAlignmentOptions.TopLeft)
            {
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
            }
            else
            {
                rect.anchorMin = new Vector2(1, 1);
                rect.anchorMax = new Vector2(1, 1);
                rect.pivot = new Vector2(1, 1);
            }

            rect.anchoredPosition = offset;
            rect.sizeDelta = new Vector2(200, 50);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.enableWordWrapping = false;

            return tmp;
        }

        private void SubscribeEvents()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnScoreChanged += UpdateScore;

            DifficultyManager dm = FindAnyObjectByType<DifficultyManager>();
            if (dm != null)
                dm.OnLevelChanged += UpdateLevel;
        }

        private void UpdateScore(int score)
        {
            _scoreText.text = $"Score: {score}";
        }

        private void UpdateLevel(int level)
        {
            _levelText.text = $"Lv.{level}";
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnScoreChanged -= UpdateScore;

            DifficultyManager dm = FindAnyObjectByType<DifficultyManager>();
            if (dm != null)
                dm.OnLevelChanged -= UpdateLevel;
        }
    }
}
