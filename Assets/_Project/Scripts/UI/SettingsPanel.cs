using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace DogtorBurguer
{
    /// <summary>
    /// The Settings panel, on the shared ModalPanel chrome (full-canvas panel art, title, round X):
    /// wide blue rows — the Sound (SFX) and Music toggles, then (menu) the Privacy row — only for
    /// players the consent SDK says need it; it re-opens Google's privacy options form — and the
    /// Language cycle, or (in-game) Restart + Quit to Menu. The Privacy row took the START level
    /// row's slot on 2026-09-26. Opened by the menu gear and the in-game top-bar gear (that one pauses the
    /// run and resumes on close). Layout knobs: UIStyles.SETTINGS_*.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        private static readonly Vector2 Center = new(0.5f, 0.5f);

        private Canvas _canvas;
        private ModalPanel _modal;
        private TextMeshProUGUI _soundLabel;
        private TextMeshProUGUI _musicLabel;
        private TextMeshProUGUI _controlLabel;
        private TextMeshProUGUI _languageLabel;
        private bool _showRunButtons;
        private Language _languageAtShow;

        /// <summary>Fired when the panel closes — the in-game opener resumes the run on this.</summary>
        public event System.Action OnClosed;

        /// <summary>Injects the canvas to build into (F-77), instead of scanning the scene.
        /// Pass <paramref name="showRunButtons"/> from the in-game opener to get the
        /// Restart + Quit-to-menu rows in place of the Privacy + Language rows.</summary>
        public void Initialize(Canvas canvas, bool showRunButtons = false)
        {
            _canvas = canvas;
            _showRunButtons = showRunButtons;
        }

        public void Show()
        {
            if (_modal == null)
                CreatePanel();

            _languageAtShow = Loc.Current;
            RefreshTexts();
            _modal.Show();
        }

        public void Hide()
        {
            if (_modal == null) return;

            _modal.Hide();
            OnClosed?.Invoke();

            // Menu texts are built once at scene load — a language change applies to them by
            // reloading the menu when the panel closes (the panel itself relabels live).
            if (!_showRunButtons && Loc.Current != _languageAtShow)
                SceneLoader.LoadMainMenu();
        }

        private void CreatePanel()
        {
            _modal = ModalPanel.Build(_canvas, Loc.Get(LocKey.SettingsTitle), "ui_settings_panel",
                UIStyles.SETTINGS_PANEL_OFFSET, UIStyles.SETTINGS_CHROME_OFFSET, Hide);

            // Rows down the body, top to bottom. A running counter, not fixed indices, so a
            // hidden row (Controls) closes its gap instead of leaving a hole. The label strings
            // are set by the Update* refreshers.
            int row = 0;
            // Sound = SFX, Music = the soundtrack: two independent toggles since 2026-09-08
            // (they shared one master mute before, so you couldn't keep music without effects).
            _soundLabel = CreateRowButton("Sound", new Vector2(0f, RowY(row++)), UIStyles.SETTINGS_ROW_W, OnSoundToggleClicked);
            _musicLabel = CreateRowButton("Music", new Vector2(0f, RowY(row++)), UIStyles.SETTINGS_ROW_W, OnMusicToggleClicked);

            // The Controls (Drag/Tap) row is retired — Tap is the only scheme now. The toggle is
            // intact behind the flag; see GameplayConfig.CONTROL_MODE_SELECTABLE for the revert
            // (and its note about the sheet only fitting four rows).
            if (GameplayConfig.CONTROL_MODE_SELECTABLE)
                _controlLabel = CreateRowButton("Controls", new Vector2(0f, RowY(row++)), UIStyles.SETTINGS_ROW_W, OnControlToggleClicked);

            // Remaining rows: in-game Restart + Quit to Menu (scene loads reset timeScale, so
            // leaving from the paused panel is safe). In the menu, Privacy (when required) + Language.
            if (_showRunButtons)
            {
                // Restart returned 2026-09-08 (dropped 2026-09-05 for space) — the 4-row sheet
                // fits it again. Ad-free by design: interstitials live ONLY on game-over Retry.
                CreateRowButton(Loc.Get(LocKey.SettingsRestart), new Vector2(0f, RowY(row++)), UIStyles.SETTINGS_ROW_W, OnRestartClicked);
                CreateRowButton(Loc.Get(LocKey.SettingsQuit), new Vector2(0f, RowY(row++)), UIStyles.SETTINGS_ROW_W, OnQuitClicked);
            }
            else
            {
                // Privacy: re-opens the consent form (GDPR requires a way to change the choice).
                // Only for players the CMP flags — elsewhere the running counter closes the gap.
                if (AdManager.Instance != null && AdManager.Instance.PrivacyOptionsRequired)
                    CreateRowButton(Loc.Get(LocKey.SettingsPrivacy), new Vector2(0f, RowY(row++)), UIStyles.SETTINGS_ROW_W,
                        () => AdManager.Instance.ShowPrivacyOptions(null));
                // Last row (menu-only): cycles the language. A mid-run swap would
                // leave already-built HUD text in the old language, so the in-game panel skips it.
                _languageLabel = CreateRowButton("Language", new Vector2(0f, RowY(row++)), UIStyles.SETTINGS_ROW_W, OnLanguageClicked);
            }
        }

        private static float RowY(int row) => UIStyles.SETTINGS_ROW_TOP_Y - row * UIStyles.SETTINGS_ROW_PITCH;

        // A wide blue blank sized by width (height follows the art) with a HUD-palette word on it.
        private TextMeshProUGUI CreateRowButton(string label, Vector2 pos, float width, UnityAction onClick)
        {
            Sprite blank = UiArt.Load("ui_btn_blue_wide");
            Vector2 size = UIFactory.SizeByWidth(blank, width);
            Button btn = UIFactory.CreateSpriteButton(_modal.Panel, label, blank, Center, pos, size, onClick);
            CreateRowLabel(btn.transform, label, size);
            return btn.GetComponentInChildren<TextMeshProUGUI>();
        }

        private static TextMeshProUGUI CreateRowLabel(Transform row, string label, Vector2 size)
        {
            TextMeshProUGUI word = UIFactory.CreateText(row, label, UIStyles.SETTINGS_ROW_LABEL_NUDGE,
                size, UIStyles.SETTINGS_ROW_LABEL_SIZE, FontStyles.Bold);
            word.gameObject.name = "Label";
            UIFactory.StyleHudText(word);
            UIFactory.AutoFit(word, UIStyles.SETTINGS_ROW_LABEL_SIZE_MIN, UIStyles.SETTINGS_ROW_LABEL_SIZE);
            return word;
        }

        // Quit is "pause and leave", not a forfeit (2026-09-13): the run is written down and the
        // menu offers RESUME, so the player can browse the shop or change a skin and come back
        // to the same board. Nothing is paid out — the end-of-run star payout still happens at
        // the real game over, whenever that comes.
        private void OnQuitClicked()
        {
            GameManager.Instance?.SaveResumePoint();
            SceneLoader.LoadMainMenu();
        }

        // Restart DOES forfeit: it starts a fresh run, and StartGame discards the saved one.
        // The scene reload resets timeScale, so restarting from the paused panel is safe.
        private void OnRestartClicked()
        {
            SceneLoader.LoadGame();
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
            _soundLabel.text = Loc.Get(SoundSettings.SoundOn
                ? LocKey.SettingsSoundOn : LocKey.SettingsSoundOff);
        }

        private void OnMusicToggleClicked()
        {
            if (SaveDataManager.Instance == null) return;

            SaveDataManager.Instance.SetMusicOn(!SaveDataManager.Instance.MusicOn);
            SoundSettings.Apply();
            UpdateMusicLabel();
        }

        private void UpdateMusicLabel()
        {
            if (_musicLabel == null) return;
            _musicLabel.text = Loc.Get(SoundSettings.MusicOn
                ? LocKey.SettingsMusicOn : LocKey.SettingsMusicOff);
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
            _controlLabel.text = Loc.Get(mode == ControlMode.Drag
                ? LocKey.SettingsControlsDrag : LocKey.SettingsControlsTap);
        }

        private void OnLanguageClicked()
        {
            if (SaveDataManager.Instance == null) return;

            SaveDataManager.Instance.SetLanguage(LanguageInfo.Next(SaveDataManager.Instance.Language));
            RefreshTexts(); // the panel relabels itself live; other screens read Loc when built
        }

        private void UpdateLanguageLabel()
        {
            if (_languageLabel == null) return;
            _languageLabel.text = Loc.Format(LocKey.SettingsLanguage, LanguageInfo.NativeName(Loc.Current));
        }

        // Every Loc-driven text on the panel — Show and the language cycle both come through here.
        private void RefreshTexts()
        {
            if (_modal != null) _modal.Title.text = Loc.Get(LocKey.SettingsTitle);
            UpdateSoundLabel();
            UpdateMusicLabel();
            UpdateControlLabel();
            UpdateLanguageLabel();
        }

        private void OnDestroy()
        {
            _modal?.Kill();
        }
    }
}
