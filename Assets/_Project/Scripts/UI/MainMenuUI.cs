using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// Builds the main menu from the authored art (drop №3): top bar with the settings gear,
    /// the logo, the big PLAY button, and the checkered
    /// bottom strip carrying the authored CREDITS and SHOP buttons, all over the colored menu
    /// illustration (the MenuBackgroundSkin). Layout knobs: UIStyles.MENU_*.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Testing")]
        [Tooltip("Adds the start-level stepper under the Settings panel (a testing tool, not part of the shipped panel).")]
        [SerializeField] private bool _showLevelStepper = false;

        private Canvas _canvas;
        private SettingsPanel _settingsPanel;
        private CreditsPanel _creditsPanel;

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

            // (The old high-score plaque was dropped 2026-09-03 — the TopBar trophy pill already
            // shows the high score; the plaque was redundant. ui_hs_plaque stays in Resources.)

            // PLAY — authored button, text baked into the art.
            Sprite play = UiArt.Load("ui_play_button");
            UIFactory.CreateSpriteButton(_canvas.transform, "Play", play, new Vector2(0.5f, 0.5f),
                UIStyles.MENU_PLAY_POS, UIFactory.SizeByWidth(play, UIStyles.MENU_PLAY_W), OnPlayClicked);

            BuildBottomStrip();

            _settingsPanel = gameObject.AddComponent<SettingsPanel>();
            _settingsPanel.Initialize(_canvas, showLevelStepper: _showLevelStepper);

            _creditsPanel = gameObject.AddComponent<CreditsPanel>();
            _creditsPanel.Initialize(_canvas);
        }

        // The checkered diner strip pinned to the bottom edge, with CREDITS and SHOP on it.
        private void BuildBottomStrip()
        {
            Sprite strip = UiArt.Load("ui_menu_bottom");
            Vector2 stripSize = UIFactory.SizeByWidth(strip, UIStyles.REFERENCE_RESOLUTION.x);
            UIFactory.CreateImage(_canvas.transform, "BottomStrip", strip, new Vector2(0.5f, 0f),
                new Vector2(0f, stripSize.y * 0.5f), stripSize);

            CreateBottomButton("CREDITS", -UIStyles.MENU_BOTTOM_BTN_X, "ui_menu_btn_credits", OnCreditsClicked);
            Button shop = CreateBottomButton("SHOP", UIStyles.MENU_BOTTOM_BTN_X, "ui_menu_btn_shop", OnShopClicked);

            float shopHeight = shop.GetComponent<RectTransform>().sizeDelta.y;
            TextMeshProUGUI support = UIFactory.CreateText(shop.transform, "Support the devs!",
                new Vector2(0f, shopHeight * 0.5f + UIStyles.MENU_SUPPORT_LABEL_Y),
                new Vector2(UIStyles.MENU_BOTTOM_BTN_W, 30f), UIStyles.MENU_SUPPORT_LABEL_SIZE, FontStyles.Bold);
            UIFactory.StyleFillAndBorder(support, UIStyles.MENU_SUPPORT_FILL,
                UIStyles.HUD_TEXT_BORDER, UIStyles.HUD_TEXT_BORDER_WIDTH);
        }

        // An authored blank (the kit's red CREDITS / yellow SHOP buttons) with a HUD-palette word on it.
        private Button CreateBottomButton(string label, float x, string art, UnityEngine.Events.UnityAction onClick)
        {
            Sprite blank = UiArt.Load(art);
            Vector2 size = UIFactory.SizeByWidth(blank, UIStyles.MENU_BOTTOM_BTN_W);
            Button btn = UIFactory.CreateSpriteButton(_canvas.transform, label, blank,
                new Vector2(0.5f, 0f), new Vector2(x, UIStyles.MENU_BOTTOM_BTN_Y), size, onClick);

            TextMeshProUGUI word = UIFactory.CreateText(btn.transform, label, UIStyles.MENU_BOTTOM_LABEL_NUDGE,
                size, UIStyles.MENU_BOTTOM_LABEL_SIZE, FontStyles.Bold);
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
            _creditsPanel?.Show();
        }
    }
}
