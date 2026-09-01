using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// Builds the main menu from the authored art (drop №3): top bar with the settings gear,
    /// the logo, the high-score plaque tucked behind the big PLAY button, and the checkered
    /// bottom strip carrying the CREDITS and SHOP buttons. Layout knobs: UIStyles.MENU_*.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Testing")]
        [Tooltip("Adds the start-level stepper under the Settings panel (a testing tool, not part of the shipped panel).")]
        [SerializeField] private bool _showLevelStepper = false;

        private Canvas _canvas;
        private SettingsPanel _settingsPanel;
        private GameObject _creditsOverlay;

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

            // Shared top bar: trophy + star + gem pills, plus the gear (shop stays a big
            // bottom button, so no top-bar shop icon here).
            TopBar.Build(_canvas.transform, onSettings: OnSettingsClicked);

            // Logo — top-anchored so it clears the top bar on tall screens.
            Sprite logo = UiArt.Load("ui_logo");
            UIFactory.CreateImage(_canvas.transform, "Logo", logo, new Vector2(0.5f, 1f),
                UIStyles.MENU_LOGO_POS, UIFactory.SizeByWidth(logo, UIStyles.MENU_LOGO_W));

            // High-score plaque, built before PLAY so the button overlaps its bottom edge.
            Sprite plaque = UiArt.Load("ui_hs_plaque");
            UIFactory.CreateImage(_canvas.transform, "HighScorePlaque", plaque, new Vector2(0.5f, 0.5f),
                UIStyles.MENU_HS_PLAQUE_POS, UIFactory.SizeByWidth(plaque, UIStyles.MENU_HS_PLAQUE_W));

            SaveDataManager save = SaveDataManager.Instance;
            UIFactory.CreateText(_canvas.transform,
                NumberFormat.Abbreviate(save != null ? save.HighScore : 0),
                UIStyles.MENU_HS_NUMBER_POS, UIStyles.MENU_HS_NUMBER_RECT,
                UIStyles.MENU_HS_NUMBER_SIZE, FontStyles.Bold, UIStyles.TOPBAR_NUMBER_COLOR);

            // PLAY — authored button, text baked into the art.
            Sprite play = UiArt.Load("ui_play_button");
            UIFactory.CreateSpriteButton(_canvas.transform, "Play", play, new Vector2(0.5f, 0.5f),
                UIStyles.MENU_PLAY_POS, UIFactory.SizeByWidth(play, UIStyles.MENU_PLAY_W), OnPlayClicked);

            BuildBottomStrip();

            _settingsPanel = gameObject.AddComponent<SettingsPanel>();
            _settingsPanel.Initialize(_canvas, showLevelStepper: _showLevelStepper);
        }

        // The checkered diner strip pinned to the bottom edge, with CREDITS and SHOP on it.
        private void BuildBottomStrip()
        {
            Sprite strip = UiArt.Load("ui_menu_bottom");
            Vector2 stripSize = UIFactory.SizeByWidth(strip, UIStyles.REFERENCE_RESOLUTION.x);
            UIFactory.CreateImage(_canvas.transform, "BottomStrip", strip, new Vector2(0.5f, 0f),
                new Vector2(0f, stripSize.y * 0.5f), stripSize);

            CreateBottomButton("CREDITS", -UIStyles.MENU_BOTTOM_BTN_X, UIStyles.MENU_CREDITS_TINT, OnCreditsClicked);
            Button shop = CreateBottomButton("SHOP", UIStyles.MENU_BOTTOM_BTN_X, UIStyles.MENU_SHOP_TINT, OnShopClicked);

            TextMeshProUGUI support = UIFactory.CreateText(shop.transform, "Support the devs!",
                new Vector2(0f, UIStyles.MENU_BOTTOM_BTN_SIZE.y * 0.5f + UIStyles.MENU_SUPPORT_LABEL_Y),
                new Vector2(UIStyles.MENU_BOTTOM_BTN_SIZE.x, 30f), UIStyles.MENU_SUPPORT_LABEL_SIZE, FontStyles.Bold);
            UIFactory.StyleFillAndBorder(support, UIStyles.MENU_SUPPORT_FILL,
                UIStyles.HUD_TEXT_BORDER, UIStyles.HUD_TEXT_BORDER_WIDTH);
        }

        // A tinted cream blank (no red/orange blank in the kit) with a HUD-palette word on it.
        private Button CreateBottomButton(string label, float x, Color tint, UnityEngine.Events.UnityAction onClick)
        {
            Button btn = UIFactory.CreateSpriteButton(_canvas.transform, label, UiArt.Load("ui_btn_cream"),
                new Vector2(0.5f, 0f), new Vector2(x, UIStyles.MENU_BOTTOM_BTN_Y),
                UIStyles.MENU_BOTTOM_BTN_SIZE, onClick);
            btn.image.color = tint;

            TextMeshProUGUI word = UIFactory.CreateText(btn.transform, label, Vector2.zero,
                UIStyles.MENU_BOTTOM_BTN_SIZE, UIStyles.MENU_BOTTOM_LABEL_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(word);
            return btn;
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

        private void OnCreditsClicked()
        {
            if (_creditsOverlay != null)
            {
                Destroy(_creditsOverlay);
                return;
            }

            _creditsOverlay = UIFactory.CreateOverlay(_canvas.transform, UIStyles.OVERLAY_DARK);

            Button closeBtn = _creditsOverlay.AddComponent<Button>();
            closeBtn.onClick.AddListener(() => { Destroy(_creditsOverlay); _creditsOverlay = null; });

            UIFactory.CreateText(_creditsOverlay.transform,
                "Dogtor Burguer!\n\nA game by Oscar\n\nPowered by Unity\n\nTap to close",
                Vector2.zero, UIStyles.CREDITS_RECT,
                UIStyles.CREDITS_TEXT_SIZE);
        }
    }
}
