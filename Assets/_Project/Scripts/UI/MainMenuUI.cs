using UnityEngine;

namespace DogtorBurguer
{
    public class MainMenuUI : MonoBehaviour
    {
        private Canvas _canvas;
        private SettingsPanel _settingsPanel;
        private UnityEngine.GameObject _creditsOverlay;

        private void Start()
        {
            AppBootstrap.EnsureCoreManagers();
            SoundSettings.Apply();

            CreateUI();
        }

        private void CreateUI()
        {
            _canvas = UIFactory.CreateCanvas(transform, "Menu_Canvas", 10);
            UIFactory.EnsureEventSystem();

            // Shared top bar: trophy (best score) + star + gem pills, same look/positions as
            // in-game. No icon buttons — the menu's big Shop/Settings buttons cover those.
            TopBar.Build(_canvas.transform);

            // Title
            UIFactory.CreateText(_canvas.transform, "Dogtor Burguer!", UIStyles.MENU_TITLE_POS, UIStyles.MENU_TEXT_RECT,
                UIStyles.MENU_TITLE_SIZE, TMPro.FontStyles.Bold, UIStyles.TEXT_HUD);

            // Buttons
            float btnY = UIStyles.MENU_BTN_START_Y;
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
            _settingsPanel = gameObject.AddComponent<SettingsPanel>();
            _settingsPanel.Initialize(_canvas);
        }

        private void OnPlayClicked()
        {
            SceneLoader.LoadGame();
        }

        private void OnShopClicked()
        {
            ShopScreen.Open();
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
                Vector2.zero, UIStyles.CREDITS_RECT,
                UIStyles.CREDITS_TEXT_SIZE);
        }
    }
}
