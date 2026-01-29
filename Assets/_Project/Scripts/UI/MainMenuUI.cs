using UnityEngine;
using TMPro;

namespace DogtorBurguer
{
    public class MainMenuUI : MonoBehaviour
    {
        private Canvas _canvas;
        private TextMeshProUGUI _gemText;
        private TextMeshProUGUI _highScoreText;
        private ShopPanel _shopPanel;
        private SettingsPanel _settingsPanel;
        private UnityEngine.GameObject _creditsOverlay;

        private void Start()
        {
            // Ensure managers exist
            if (SaveDataManager.Instance == null)
            {
                GameObject saveObj = new GameObject("SaveDataManager");
                saveObj.AddComponent<SaveDataManager>();
            }

            if (AdManager.Instance == null)
            {
                GameObject adObj = new GameObject("AdManager");
                adObj.AddComponent<AdManager>();
            }

            if (MusicManager.Instance == null)
            {
                GameObject musicObj = new GameObject("MusicManager");
                musicObj.AddComponent<MusicManager>();
            }

            AudioListener.volume = SaveDataManager.Instance.SoundOn ? 1f : 0f;
            MusicManager.Instance?.ApplySoundSetting();

            CreateUI();

            if (SaveDataManager.Instance != null)
                SaveDataManager.Instance.OnGemsChanged += UpdateGemDisplay;
        }

        private void CreateUI()
        {
            _canvas = UIFactory.CreateCanvas(transform, "Menu_Canvas", 10);
            UIFactory.EnsureEventSystem();

            // Title
            UIFactory.CreateText(_canvas.transform, "Dogtor Burguer!", new Vector2(0, 300), new Vector2(400, 60),
                UIStyles.MENU_TITLE_SIZE, TMPro.FontStyles.Bold, UIStyles.TEXT_HUD);

            // High Score
            int highScore = SaveDataManager.Instance != null ? SaveDataManager.Instance.HighScore : 0;
            _highScoreText = UIFactory.CreateText(_canvas.transform, $"Best: {highScore}", new Vector2(0, 230), new Vector2(400, 60),
                UIStyles.MENU_HIGHSCORE_SIZE, color: UIStyles.TEXT_HUD);

            // Gem counter (top-right)
            int gems = SaveDataManager.Instance != null ? SaveDataManager.Instance.Gems : 0;
            _gemText = UIFactory.CreateText(_canvas.transform, $"Gems: {gems}", new Vector2(0, 400), new Vector2(200, 40),
                UIStyles.MENU_GEM_SIZE, TMPro.FontStyles.Bold, UIStyles.TEXT_HUD);
            RectTransform gemRect = _gemText.GetComponent<RectTransform>();
            gemRect.anchorMin = new Vector2(1f, 1f);
            gemRect.anchorMax = new Vector2(1f, 1f);
            gemRect.pivot = new Vector2(1f, 1f);
            gemRect.anchoredPosition = new Vector2(-20, -20);
            _gemText.alignment = TMPro.TextAlignmentOptions.TopRight;

            // Buttons
            float btnY = 80f;
            UIFactory.CreateButton(_canvas.transform, "Play", new Vector2(0, btnY),
                UIStyles.MENU_BUTTON_SIZE, UIStyles.BTN_PLAY, UIStyles.MENU_BUTTON_TEXT_SIZE, OnPlayClicked);

            UIFactory.CreateButton(_canvas.transform, "Shop", new Vector2(0, btnY + UIStyles.MENU_BUTTON_SPACING),
                UIStyles.MENU_BUTTON_SIZE, UIStyles.BTN_SHOP, UIStyles.MENU_BUTTON_TEXT_SIZE, OnShopClicked);

            UIFactory.CreateButton(_canvas.transform, "Settings", new Vector2(0, btnY + UIStyles.MENU_BUTTON_SPACING * 2),
                UIStyles.MENU_BUTTON_SIZE, UIStyles.BTN_SETTINGS, UIStyles.MENU_BUTTON_TEXT_SIZE, OnSettingsClicked);

            UIFactory.CreateButton(_canvas.transform, "Leaderboard", new Vector2(0, btnY + UIStyles.MENU_BUTTON_SPACING * 3),
                UIStyles.MENU_BUTTON_SIZE, UIStyles.BTN_LEADERBOARD, UIStyles.MENU_BUTTON_TEXT_SIZE, OnLeaderboardClicked);

            UIFactory.CreateButton(_canvas.transform, "Credits", new Vector2(0, btnY + UIStyles.MENU_BUTTON_SPACING * 4),
                UIStyles.MENU_BUTTON_SIZE, UIStyles.BTN_CLOSE, UIStyles.MENU_BUTTON_TEXT_SIZE, OnCreditsClicked);

            // Sub-panels
            _shopPanel = gameObject.AddComponent<ShopPanel>();
            _settingsPanel = gameObject.AddComponent<SettingsPanel>();
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

            _creditsOverlay = UIFactory.CreateOverlay(_canvas.transform, UIStyles.OVERLAY_DARK);

            UnityEngine.UI.Button closeBtn = _creditsOverlay.AddComponent<UnityEngine.UI.Button>();
            closeBtn.onClick.AddListener(() => { Destroy(_creditsOverlay); _creditsOverlay = null; });

            UIFactory.CreateText(_creditsOverlay.transform,
                "Dogtor Burguer!\n\nA game by Oscar\n\nPowered by Unity\n\nTap to close",
                Vector2.zero, new Vector2(400, 300),
                UIStyles.CREDITS_TEXT_SIZE);
        }

        private void OnDestroy()
        {
            if (SaveDataManager.Instance != null)
                SaveDataManager.Instance.OnGemsChanged -= UpdateGemDisplay;
        }
    }
}
