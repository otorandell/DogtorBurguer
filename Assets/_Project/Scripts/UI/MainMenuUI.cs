using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using DG.Tweening;

namespace DogtorBurguer
{
    public class MainMenuUI : MonoBehaviour
    {
        private Canvas _canvas;
        private TextMeshProUGUI _gemText;
        private TextMeshProUGUI _highScoreText;
        private ShopPanel _shopPanel;
        private SettingsPanel _settingsPanel;
        private GameObject _creditsOverlay;

        private void Start()
        {
            // Ensure SaveDataManager exists
            if (SaveDataManager.Instance == null)
            {
                GameObject saveObj = new GameObject("SaveDataManager");
                saveObj.AddComponent<SaveDataManager>();
            }

            // Ensure AdManager exists
            if (AdManager.Instance == null)
            {
                GameObject adObj = new GameObject("AdManager");
                adObj.AddComponent<AdManager>();
            }

            // Apply sound setting
            AudioListener.volume = SaveDataManager.Instance.SoundOn ? 1f : 0f;

            CreateUI();

            if (SaveDataManager.Instance != null)
                SaveDataManager.Instance.OnGemsChanged += UpdateGemDisplay;
        }

        private void CreateUI()
        {
            // Canvas
            GameObject canvasObj = new GameObject("Menu_Canvas");
            canvasObj.transform.SetParent(transform);

            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(540, 960);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<InputSystemUIInputModule>();
            }

            // Title
            CreateText(canvasObj, "Dogtor Burguer!", 0, 300, 48, FontStyles.Bold,
                new Color(1f, 0.85f, 0.2f));

            // High Score
            int highScore = SaveDataManager.Instance != null ? SaveDataManager.Instance.HighScore : 0;
            _highScoreText = CreateText(canvasObj, $"Best: {highScore}", 0, 230, 24, FontStyles.Normal, Color.gray);

            // Gem counter (top-right)
            int gems = SaveDataManager.Instance != null ? SaveDataManager.Instance.Gems : 0;
            _gemText = CreateText(canvasObj, $"Gems: {gems}", 0, 400, 22, FontStyles.Bold,
                new Color(1f, 0.85f, 0f));
            RectTransform gemRect = _gemText.GetComponent<RectTransform>();
            gemRect.anchorMin = new Vector2(1f, 1f);
            gemRect.anchorMax = new Vector2(1f, 1f);
            gemRect.pivot = new Vector2(1f, 1f);
            gemRect.anchoredPosition = new Vector2(-20, -20);
            gemRect.sizeDelta = new Vector2(200, 40);
            _gemText.alignment = TextAlignmentOptions.TopRight;

            // Buttons
            float btnY = 80f;
            float btnSpacing = -85f;

            CreateMenuButton(canvasObj, "Play", btnY, new Color(0.2f, 0.8f, 0.3f), OnPlayClicked);
            CreateMenuButton(canvasObj, "Shop", btnY + btnSpacing, new Color(0.9f, 0.7f, 0.1f), OnShopClicked);
            CreateMenuButton(canvasObj, "Settings", btnY + btnSpacing * 2, new Color(0.4f, 0.6f, 0.9f), OnSettingsClicked);
            CreateMenuButton(canvasObj, "Leaderboard", btnY + btnSpacing * 3, new Color(0.6f, 0.4f, 0.8f), OnLeaderboardClicked);
            CreateMenuButton(canvasObj, "Credits", btnY + btnSpacing * 4, new Color(0.5f, 0.5f, 0.5f), OnCreditsClicked);

            // Create sub-panels (hidden initially)
            _shopPanel = gameObject.AddComponent<ShopPanel>();
            _settingsPanel = gameObject.AddComponent<SettingsPanel>();
        }

        private TextMeshProUGUI CreateText(GameObject parent, string text, float x, float y,
            float size, FontStyles style, Color color)
        {
            GameObject textObj = new GameObject(text);
            textObj.transform.SetParent(parent.transform, false);

            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(400, 60);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;

            return tmp;
        }

        private void CreateMenuButton(GameObject parent, string label, float y, Color color, UnityEngine.Events.UnityAction onClick)
        {
            GameObject btnObj = new GameObject($"{label}Button");
            btnObj.transform.SetParent(parent.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = new Vector2(0, y);
            btnRect.sizeDelta = new Vector2(300, 65);

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = color;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(onClick);

            // Button text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 28;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
        }

        private void UpdateGemDisplay(int gems)
        {
            if (_gemText != null)
                _gemText.text = $"Gems: {gems}";
        }

        private void OnPlayClicked()
        {
            SceneLoader.LoadGame();
        }

        private void OnShopClicked()
        {
            _shopPanel?.Show();
        }

        private void OnSettingsClicked()
        {
            _settingsPanel?.Show();
        }

        private void OnLeaderboardClicked()
        {
            Debug.Log("[MainMenu] Leaderboard - Coming Soon!");
        }

        private void OnCreditsClicked()
        {
            if (_creditsOverlay != null)
            {
                Destroy(_creditsOverlay);
                return;
            }

            _creditsOverlay = new GameObject("Credits");
            _creditsOverlay.transform.SetParent(_canvas.transform, false);

            RectTransform overlayRect = _creditsOverlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.sizeDelta = Vector2.zero;

            Image overlayImg = _creditsOverlay.AddComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.85f);

            Button closeBtn = _creditsOverlay.AddComponent<Button>();
            closeBtn.onClick.AddListener(() => { Destroy(_creditsOverlay); _creditsOverlay = null; });

            TextMeshProUGUI creditsTmp = CreateText(_creditsOverlay,
                "Dogtor Burguer!\n\nA game by Oscar\n\nPowered by Unity\n\nTap to close",
                0, 0, 24, FontStyles.Normal, Color.white);
        }

        private void OnDestroy()
        {
            if (SaveDataManager.Instance != null)
                SaveDataManager.Instance.OnGemsChanged -= UpdateGemDisplay;
        }
    }
}
