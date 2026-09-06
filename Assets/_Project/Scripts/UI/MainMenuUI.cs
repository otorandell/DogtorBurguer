using DG.Tweening;
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
        [Tooltip("TESTER BUILD: everything unlocked (skins, currency, consumables), ads bypassed, store mocked, level stepper shown, red TEST BUILD label on the menu. Never ship with this on.")]
        [SerializeField] private bool _testBuild = false;

        private Canvas _canvas;
        private SettingsPanel _settingsPanel;
        private CreditsPanel _creditsPanel;
        private HowToPlayPanel _howToPanel;

        private void Start()
        {
            // Before the managers: AdManager/IapManager pick their provider in Awake.
            if (_testBuild) TestBuild.Enable();
            AppBootstrap.EnsureCoreManagers();
            TestBuild.TopUpStash(SaveDataManager.Instance);
            SoundSettings.Apply();

            CreateUI();
        }

        private void CreateUI()
        {
            _canvas = UIFactory.CreateCanvas(transform, "Menu_Canvas", 10);
            UIFactory.EnsureEventSystem();

            // Shared top bar: trophy + star + gem pills, the "?" help button and the gear —
            // identical placement and size to the in-game bar (shop stays a big bottom button).
            TopBar.Build(_canvas.transform, onHelp: OnHelpClicked, onSettings: OnSettingsClicked);

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
            if (TestBuild.IsEnabled) BuildTestBuildLabel();

            _settingsPanel = gameObject.AddComponent<SettingsPanel>();
            _settingsPanel.Initialize(_canvas, showLevelStepper: _showLevelStepper || TestBuild.IsEnabled);

            _creditsPanel = gameObject.AddComponent<CreditsPanel>();
            _creditsPanel.Initialize(_canvas);

            _howToPanel = gameObject.AddComponent<HowToPlayPanel>();
            _howToPanel.Initialize(_canvas);
        }

        // The checkered diner strip pinned to the bottom edge, with CREDITS and SHOP on it.
        private void BuildBottomStrip()
        {
            // The strip art carries transparent side margins, so it's sized wider than the canvas
            // (MENU_BOTTOM_STRIP_W) for the checker to actually reach the screen edges.
            Sprite strip = UiArt.Load("ui_menu_bottom");
            Vector2 stripSize = UIFactory.SizeByWidth(strip, UIStyles.MENU_BOTTOM_STRIP_W);
            UIFactory.CreateImage(_canvas.transform, "BottomStrip", strip, new Vector2(0.5f, 0f),
                new Vector2(0f, stripSize.y * 0.5f), stripSize);

            CreateBottomButton("CREDITS", -UIStyles.MENU_BOTTOM_BTN_X, "ui_menu_btn_credits",
                UIStyles.MENU_CREDITS_LABEL_SIZE, OnCreditsClicked);
            Button shop = CreateBottomButton("SHOP", UIStyles.MENU_BOTTOM_BTN_X, "ui_menu_btn_shop",
                UIStyles.MENU_SHOP_LABEL_SIZE, OnShopClicked);

            float shopHeight = shop.GetComponent<RectTransform>().sizeDelta.y;
            TextMeshProUGUI support = UIFactory.CreateText(shop.transform, "Support the devs!",
                new Vector2(0f, shopHeight * 0.5f + UIStyles.MENU_SUPPORT_LABEL_Y),
                new Vector2(UIStyles.MENU_SUPPORT_LABEL_W, 36f), UIStyles.MENU_SUPPORT_LABEL_SIZE, FontStyles.Bold);
            // Flashy per the mock: green vertical gradient + dark outline + the downward shadow ring.
            UIFactory.StyleFillAndBorder(support, Color.white,
                UIStyles.HUD_TEXT_BORDER, UIStyles.HUD_TEXT_BORDER_WIDTH, UIStyles.HUD_TEXT_BORDER);
            support.enableVertexGradient = true;
            support.colorGradient = new VertexGradient(UIStyles.MENU_SUPPORT_TOP, UIStyles.MENU_SUPPORT_TOP,
                UIStyles.MENU_SUPPORT_BOTTOM, UIStyles.MENU_SUPPORT_BOTTOM);
            // Fills the button's width (auto-size grows to the rect) and pulses for attention.
            UIFactory.AutoFit(support, UIStyles.MENU_SUPPORT_LABEL_MIN, UIStyles.MENU_SUPPORT_LABEL_SIZE);
            support.transform.DOScale(AnimConfig.MENU_SUPPORT_PULSE_SCALE, AnimConfig.MENU_SUPPORT_PULSE_DURATION)
                .SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetLink(support.gameObject);
        }

        // The unmissable red stamp: this build is the tester one (TestBuild). Never on a release.
        private void BuildTestBuildLabel()
        {
            TextMeshProUGUI label = UIFactory.CreateText(_canvas.transform, "TEST BUILD",
                UIStyles.MENU_TEST_BUILD_POS, UIStyles.MENU_TEST_BUILD_RECT, UIStyles.MENU_TEST_BUILD_SIZE,
                FontStyles.Bold);
            UIFactory.StyleFillAndBorder(label, UIStyles.MENU_TEST_BUILD_COLOR,
                UIStyles.HUD_TEXT_BORDER, UIStyles.HUD_TEXT_BORDER_WIDTH, UIStyles.HUD_TEXT_BORDER);
        }

        // An authored blank (the kit's red CREDITS / yellow SHOP buttons) with a HUD-palette word on it.
        private Button CreateBottomButton(string label, float x, string art, float labelSize,
            UnityEngine.Events.UnityAction onClick)
        {
            Sprite blank = UiArt.Load(art);
            Vector2 size = UIFactory.SizeByWidth(blank, UIStyles.MENU_BOTTOM_BTN_W);
            Button btn = UIFactory.CreateSpriteButton(_canvas.transform, label, blank,
                new Vector2(0.5f, 0f), new Vector2(x, UIStyles.MENU_BOTTOM_BTN_Y), size, onClick);

            TextMeshProUGUI word = UIFactory.CreateText(btn.transform, label, UIStyles.MENU_BOTTOM_LABEL_NUDGE,
                size, labelSize, FontStyles.Bold);
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

        private void OnHelpClicked()
        {
            _howToPanel?.Show();
        }
    }
}
