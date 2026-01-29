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

            // Ensure MusicManager exists
            if (MusicManager.Instance == null)
            {
                GameObject musicObj = new GameObject("MusicManager");
                musicObj.AddComponent<MusicManager>();
            }

            // Apply sound setting
            AudioListener.volume = SaveDataManager.Instance.SoundOn ? 1f : 0f;
            MusicManager.Instance?.ApplySoundSetting();

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
            scaler.referenceResolution = UIStyles.REFERENCE_RESOLUTION;
            scaler.matchWidthOrHeight = UIStyles.MATCH_WIDTH_OR_HEIGHT;

            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<InputSystemUIInputModule>();
            }

            // Title
            CreateText(canvasObj, "Dogtor Burguer!", 0, 300, UIStyles.MENU_TITLE_SIZE, FontStyles.Bold, UIStyles.TEXT_HUD);

            // High Score
            int highScore = SaveDataManager.Instance != null ? SaveDataManager.Instance.HighScore : 0;
            _highScoreText = CreateText(canvasObj, $"Best: {highScore}", 0, 230, UIStyles.MENU_HIGHSCORE_SIZE, FontStyles.Normal, UIStyles.TEXT_HUD);

            // Gem counter (top-right)
            int gems = SaveDataManager.Instance != null ? SaveDataManager.Instance.Gems : 0;
            _gemText = CreateText(canvasObj, $"Gems: {gems}", 0, 400, UIStyles.MENU_GEM_SIZE, FontStyles.Bold, UIStyles.TEXT_HUD);
            RectTransform gemRect = _gemText.GetComponent<RectTransform>();
            gemRect.anchorMin = new Vector2(1f, 1f);
            gemRect.anchorMax = new Vector2(1f, 1f);
            gemRect.pivot = new Vector2(1f, 1f);
            gemRect.anchoredPosition = new Vector2(-20, -20);
            gemRect.sizeDelta = new Vector2(200, 40);
            _gemText.alignment = TextAlignmentOptions.TopRight;

            // Buttons
            float btnY = 80f;

            CreateMenuButton(canvasObj, "Play", btnY, UIStyles.BTN_PLAY, OnPlayClicked);
            CreateMenuButton(canvasObj, "Shop", btnY + UIStyles.MENU_BUTTON_SPACING, UIStyles.BTN_SHOP, OnShopClicked);
            CreateMenuButton(canvasObj, "Settings", btnY + UIStyles.MENU_BUTTON_SPACING * 2, UIStyles.BTN_SETTINGS, OnSettingsClicked);
            CreateMenuButton(canvasObj, "Leaderboard", btnY + UIStyles.MENU_BUTTON_SPACING * 3, UIStyles.BTN_LEADERBOARD, OnLeaderboardClicked);
            CreateMenuButton(canvasObj, "Credits", btnY + UIStyles.MENU_BUTTON_SPACING * 4, UIStyles.BTN_CLOSE, OnCreditsClicked);

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
            tmp.outlineWidth = UIStyles.OUTLINE_WIDTH_UI;
            tmp.outlineColor = UIStyles.OUTLINE_COLOR;

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
            btnRect.sizeDelta = UIStyles.MENU_BUTTON_SIZE;

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
            tmp.fontSize = UIStyles.MENU_BUTTON_TEXT_SIZE;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = UIStyles.TEXT_UI;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.outlineWidth = UIStyles.OUTLINE_WIDTH_UI;
            tmp.outlineColor = UIStyles.OUTLINE_COLOR;
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
            overlayImg.color = UIStyles.OVERLAY_DARK;

            Button closeBtn = _creditsOverlay.AddComponent<Button>();
            closeBtn.onClick.AddListener(() => { Destroy(_creditsOverlay); _creditsOverlay = null; });

            TextMeshProUGUI creditsTmp = CreateText(_creditsOverlay,
                "Dogtor Burguer!\n\nA game by Oscar\n\nPowered by Unity\n\nTap to close",
                0, 0, UIStyles.CREDITS_TEXT_SIZE, FontStyles.Normal, UIStyles.TEXT_UI);
        }

        private void OnDestroy()
        {
            if (SaveDataManager.Instance != null)
                SaveDataManager.Instance.OnGemsChanged -= UpdateGemDisplay;
        }
    }
}
