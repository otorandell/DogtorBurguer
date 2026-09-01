using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace DogtorBurguer
{
    /// <summary>
    /// The Settings panel, built from the authored art (drop №3): the full-canvas panel (orange
    /// title tab and dotted cream body baked in), the round X over the tab's corner, and wide blue
    /// rows — the Sound and Controls toggles, plus a Restart | Quit-to-menu pair in-game. Opened by
    /// the menu gear and the in-game top-bar gear (that one pauses the run and resumes on close).
    /// Layout knobs: UIStyles.SETTINGS_*. The start-level stepper is a dev-only row under the panel.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        private static readonly Vector2 Center = new(0.5f, 0.5f);

        private Canvas _canvas;
        private GameObject _root;        // overlay + everything under it; toggled on show/hide
        private CanvasGroup _canvasGroup;
        private Transform _panel;        // the panel art; the pop-in scales it (and its children)
        private TextMeshProUGUI _soundLabel;
        private TextMeshProUGUI _controlLabel;
        private bool _showRunButtons;

        /// <summary>Fired when the panel closes — the in-game opener resumes the run on this.</summary>
        public event System.Action OnClosed;

        /// <summary>Injects the canvas to build into (F-77), instead of scanning the scene.
        /// Pass <paramref name="showRunButtons"/> from the in-game opener to add the
        /// Restart / Quit-to-menu row (meaningless in the menu, so off by default).</summary>
        public void Initialize(Canvas canvas, bool showRunButtons = false)
        {
            _canvas = canvas;
            _showRunButtons = showRunButtons;
        }

        public void Show()
        {
            if (_root == null)
                CreatePanel();

            _root.SetActive(true);
            UpdateSoundLabel();
            UpdateControlLabel();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            UpdateLevelLabel();
#endif
            PlayPopIn();
        }

        public void Hide()
        {
            if (_root == null) return;

            _root.SetActive(false);
            OnClosed?.Invoke();
        }

        private void CreatePanel()
        {
            _root = UIFactory.CreateOverlay(_canvas.transform, UIStyles.MODAL_OVERLAY);
            _canvasGroup = _root.AddComponent<CanvasGroup>();

            // The panel art is a full-phone canvas: shown at the reference resolution it lands exactly
            // where the artist drew it. Everything else is a child so the pop-in scales the whole panel.
            Image panel = UIFactory.CreateImage(_root.transform, "Panel", UiArt.Load("ui_settings_panel"),
                Center, Vector2.zero, UIStyles.REFERENCE_RESOLUTION);
            _panel = panel.transform;

            TextMeshProUGUI title = UIFactory.CreateText(_panel, "SETTINGS", UIStyles.SETTINGS_TITLE_POS,
                UIStyles.SETTINGS_TITLE_RECT, UIStyles.SETTINGS_TITLE_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(title);

            Sprite close = UiArt.Load("ui_btn_close_x");
            UIFactory.CreateSpriteButton(_panel, "Close", close, Center, UIStyles.SETTINGS_CLOSE_POS,
                UIFactory.SizeByHeight(close, UIStyles.SETTINGS_CLOSE_H), Hide);

            // Rows down the body. The label strings are set by the Update*Label refreshers.
            _soundLabel = CreateRowButton("Sound", new Vector2(0f, RowY(0)), UIStyles.SETTINGS_ROW_W, OnSoundToggleClicked);
            _controlLabel = CreateRowButton("Controls", new Vector2(0f, RowY(1)), UIStyles.SETTINGS_ROW_W, OnControlToggleClicked);

            // In-game run controls share the third row as a half-width pair. Scene loads reset
            // timeScale (SceneLoader), so leaving from the paused panel is safe.
            if (_showRunButtons)
            {
                CreateRowButton("Restart", new Vector2(-UIStyles.SETTINGS_PAIR_X, RowY(2)), UIStyles.SETTINGS_PAIR_W, OnRestartClicked);
                CreateRowButton("Quit to Menu", new Vector2(UIStyles.SETTINGS_PAIR_X, RowY(2)), UIStyles.SETTINGS_PAIR_W, OnQuitClicked);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            BuildDevStepper();
#endif
        }

        private static float RowY(int row) => UIStyles.SETTINGS_ROW_TOP_Y - row * UIStyles.SETTINGS_ROW_PITCH;

        // A wide blue blank sized by width (height follows the art) with a HUD-palette word on it.
        private TextMeshProUGUI CreateRowButton(string label, Vector2 pos, float width, UnityAction onClick)
        {
            Sprite blank = UiArt.Load("ui_btn_blue_wide");
            Vector2 size = UIFactory.SizeByWidth(blank, width);
            Button btn = UIFactory.CreateSpriteButton(_panel, label, blank, Center, pos, size, onClick);

            TextMeshProUGUI word = UIFactory.CreateText(btn.transform, label, UIStyles.SETTINGS_ROW_LABEL_NUDGE,
                size, UIStyles.SETTINGS_ROW_LABEL_SIZE, FontStyles.Bold);
            word.gameObject.name = "Label";
            UIFactory.StyleHudText(word);
            UIFactory.AutoFit(word, UIStyles.SETTINGS_ROW_LABEL_SIZE_MIN, UIStyles.SETTINGS_ROW_LABEL_SIZE);
            return word;
        }

        private void PlayPopIn()
        {
            _canvasGroup.DOKill();
            _panel.DOKill();
            _canvasGroup.alpha = 0f;
            _panel.localScale = Vector3.one * AnimConfig.PANEL_START_SCALE;

            DOTween.Sequence()
                .Append(_canvasGroup.DOFade(1f, AnimConfig.PANEL_FADE_DURATION))
                .Join(_panel.DOScale(1f, AnimConfig.PANEL_SCALE_DURATION).SetEase(Ease.OutBack))
                .SetUpdate(true); // the in-game opener pauses the run (timeScale 0)
        }

        private void OnRestartClicked()
        {
            // Same interstitial cadence as the game-over restart — a restart is a new game.
            if (AdManager.Instance != null)
                AdManager.Instance.MaybeShowInterstitial(() => GameManager.Instance?.RestartGame());
            else
                GameManager.Instance?.RestartGame();
        }

        private void OnQuitClicked()
        {
            SceneLoader.LoadMainMenu();
        }

        private void OnSoundToggleClicked()
        {
            if (SaveDataManager.Instance == null) return;

            bool newState = !SaveDataManager.Instance.SoundOn;
            SaveDataManager.Instance.SetSoundOn(newState);
            SoundSettings.Apply();
            UpdateSoundLabel();
        }

        private void UpdateSoundLabel()
        {
            if (_soundLabel == null) return;
            bool soundOn = SaveDataManager.Instance != null
                ? SaveDataManager.Instance.SoundOn
                : SaveDataManager.DEFAULT_SOUND_ON;
            _soundLabel.text = soundOn ? "Sound: ON" : "Sound: OFF";
        }

        private void OnControlToggleClicked()
        {
            if (SaveDataManager.Instance == null) return;

            ControlMode current = SaveDataManager.Instance.ControlMode;
            ControlMode next = current == ControlMode.Drag ? ControlMode.Tap : ControlMode.Drag;
            SaveDataManager.Instance.SetControlMode(next);
            UpdateControlLabel();
        }

        private void UpdateControlLabel()
        {
            if (_controlLabel == null) return;
            ControlMode mode = SaveDataManager.Instance != null
                ? SaveDataManager.Instance.ControlMode
                : SaveDataManager.DEFAULT_CONTROL_MODE;
            _controlLabel.text = mode == ControlMode.Drag ? "Controls: Drag" : "Controls: Tap";
        }

        private void OnDestroy()
        {
            if (_canvasGroup != null) _canvasGroup.DOKill();
            if (_panel != null) _panel.DOKill();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        // Start-level stepper — a testing tool, not part of the shipped panel: [−] Lv N [+] in flat
        // placeholder widgets below the art. Steps the persisted StartingLevel by one, clamped
        // 1..SETTINGS_LEVEL_CAP (see GameplayConfig — lower the cap to MAX_LEVEL before release).
        private TextMeshProUGUI _levelLabel;

        private void BuildDevStepper()
        {
            float y = UIStyles.SETTINGS_DEV_STEPPER_Y;
            UIFactory.CreateButton(_panel, "-", new Vector2(-UIStyles.SETTINGS_DEV_STEPPER_X, y),
                UIStyles.SETTINGS_STEPPER_BTN_SIZE, UIStyles.BTN_DEV_STEPPER,
                UIStyles.SETTINGS_DEV_TEXT_SIZE, () => OnLevelStep(-1));

            _levelLabel = UIFactory.CreateText(_panel, "Lv 1", new Vector2(0f, y),
                UIStyles.SETTINGS_STEPPER_LABEL_SIZE, UIStyles.SETTINGS_DEV_TEXT_SIZE, FontStyles.Bold);

            // The trial font renders "+" as a placeholder sliver glyph, so the increment
            // button uses the authored plus art instead of a text label.
            UIFactory.CreateSpriteButton(_panel, "Plus", UiArt.Load("ui_consumable_plus"),
                Center, new Vector2(UIStyles.SETTINGS_DEV_STEPPER_X, y),
                UIStyles.SETTINGS_STEPPER_BTN_SIZE, () => OnLevelStep(1));
        }

        private void OnLevelStep(int delta)
        {
            if (SaveDataManager.Instance == null) return;

            SaveDataManager.Instance.SetStartingLevel(SaveDataManager.Instance.StartingLevel + delta);
            UpdateLevelLabel();
        }

        private void UpdateLevelLabel()
        {
            if (_levelLabel == null) return;
            int level = SaveDataManager.Instance != null
                ? SaveDataManager.Instance.StartingLevel
                : SaveDataManager.DEFAULT_STARTING_LEVEL;
            _levelLabel.text = $"Lv {level}";
        }
#endif
    }
}
