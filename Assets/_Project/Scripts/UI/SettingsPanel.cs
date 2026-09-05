using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace DogtorBurguer
{
    /// <summary>
    /// The Settings panel, on the shared ModalPanel chrome (full-canvas panel art, title, round X):
    /// wide blue rows — the Sound and Controls toggles, the menu-only Mode (Classic/Relax) toggle,
    /// plus a full-width Quit to Menu in-game (which takes the third row instead of Mode).
    /// Opened by the menu gear and the in-game top-bar gear (that one pauses the run and resumes on close).
    /// Layout knobs: UIStyles.SETTINGS_*. The start-level stepper is a testing row under the panel,
    /// opt-in from the MainMenuUI inspector (never in-game — the value only applies to the next run).
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        private static readonly Vector2 Center = new(0.5f, 0.5f);

        private Canvas _canvas;
        private ModalPanel _modal;
        private TextMeshProUGUI _soundLabel;
        private TextMeshProUGUI _controlLabel;
        private TextMeshProUGUI _modeLabel;
        private bool _showRunButtons;
        private bool _showLevelStepper;

        /// <summary>Fired when the panel closes — the in-game opener resumes the run on this.</summary>
        public event System.Action OnClosed;

        /// <summary>Injects the canvas to build into (F-77), instead of scanning the scene.
        /// Pass <paramref name="showRunButtons"/> from the in-game opener to add the
        /// Restart / Quit-to-menu row (meaningless in the menu, so off by default); pass
        /// <paramref name="showLevelStepper"/> from the menu to add the start-level testing row.</summary>
        public void Initialize(Canvas canvas, bool showRunButtons = false, bool showLevelStepper = false)
        {
            _canvas = canvas;
            _showRunButtons = showRunButtons;
            _showLevelStepper = showLevelStepper;
        }

        public void Show()
        {
            if (_modal == null)
                CreatePanel();

            UpdateSoundLabel();
            UpdateControlLabel();
            UpdateModeLabel();
            UpdateLevelLabel();
            _modal.Show();
        }

        public void Hide()
        {
            if (_modal == null) return;

            _modal.Hide();
            OnClosed?.Invoke();
        }

        private void CreatePanel()
        {
            _modal = ModalPanel.Build(_canvas, "SETTINGS", "ui_modal_panel", Vector2.zero, Vector2.zero, Hide);

            // Rows down the body. The label strings are set by the Update*Label refreshers.
            _soundLabel = CreateRowButton("Sound", new Vector2(0f, RowY(0)), UIStyles.SETTINGS_ROW_W, OnSoundToggleClicked);
            _controlLabel = CreateRowButton("Controls", new Vector2(0f, RowY(1)), UIStyles.SETTINGS_ROW_W, OnControlToggleClicked);

            // Mode toggle — MENU only: the value applies to the NEXT run (managers read it once
            // at scene load), so an in-game toggle would only mislead; Quit to Menu takes its row.
            if (!_showRunButtons)
                _modeLabel = CreateRowButton("Mode", new Vector2(0f, RowY(2)), UIStyles.SETTINGS_ROW_W, OnModeToggleClicked);

            // In-game the third row is a full-width Quit to Menu (the Restart half was dropped
            // 2026-09-05 — game over already offers Retry). Scene loads reset timeScale
            // (SceneLoader), so leaving from the paused panel is safe.
            if (_showRunButtons)
                CreateRowButton("Quit to Menu", new Vector2(0f, RowY(2)), UIStyles.SETTINGS_ROW_W, OnQuitClicked);

            if (_showLevelStepper)
                BuildLevelStepper();
        }

        private static float RowY(int row) => UIStyles.SETTINGS_ROW_TOP_Y - row * UIStyles.SETTINGS_ROW_PITCH;

        // A wide blue blank sized by width (height follows the art) with a HUD-palette word on it.
        private TextMeshProUGUI CreateRowButton(string label, Vector2 pos, float width, UnityAction onClick)
        {
            Sprite blank = UiArt.Load("ui_btn_blue_wide");
            Vector2 size = UIFactory.SizeByWidth(blank, width);
            Button btn = UIFactory.CreateSpriteButton(_modal.Panel, label, blank, Center, pos, size, onClick);

            TextMeshProUGUI word = UIFactory.CreateText(btn.transform, label, UIStyles.SETTINGS_ROW_LABEL_NUDGE,
                size, UIStyles.SETTINGS_ROW_LABEL_SIZE, FontStyles.Bold);
            word.gameObject.name = "Label";
            UIFactory.StyleHudText(word);
            UIFactory.AutoFit(word, UIStyles.SETTINGS_ROW_LABEL_SIZE_MIN, UIStyles.SETTINGS_ROW_LABEL_SIZE);
            return word;
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

        private void OnModeToggleClicked()
        {
            if (SaveDataManager.Instance == null) return;

            GameMode next = SaveDataManager.Instance.Mode == GameMode.Classic
                ? GameMode.Relax : GameMode.Classic;
            SaveDataManager.Instance.SetGameMode(next);
            UpdateModeLabel();
        }

        private void UpdateModeLabel()
        {
            if (_modeLabel == null) return;
            GameMode mode = SaveDataManager.Instance != null
                ? SaveDataManager.Instance.Mode : SaveDataManager.DEFAULT_GAME_MODE;
            _modeLabel.text = mode == GameMode.Classic ? "Mode: Classic" : "Mode: Relax";
        }

        private void OnDestroy()
        {
            _modal?.Kill();
        }

        // Start-level stepper — a testing tool, not part of the shipped panel: [−] Lv N [+] in flat
        // placeholder widgets below the art. Steps the persisted StartingLevel by one, clamped
        // 1..SETTINGS_LEVEL_CAP (see GameplayConfig — lower the cap to MAX_LEVEL before release).
        private TextMeshProUGUI _levelLabel;

        private void BuildLevelStepper()
        {
            Transform root = _modal.Panel;
            float y = UIStyles.SETTINGS_DEV_STEPPER_Y;
            UIFactory.CreateButton(root, "-", new Vector2(-UIStyles.SETTINGS_DEV_STEPPER_X, y),
                UIStyles.SETTINGS_STEPPER_BTN_SIZE, UIStyles.BTN_DEV_STEPPER,
                UIStyles.SETTINGS_DEV_TEXT_SIZE, () => OnLevelStep(-1));

            _levelLabel = UIFactory.CreateText(root, "Lv 1", new Vector2(0f, y),
                UIStyles.SETTINGS_STEPPER_LABEL_SIZE, UIStyles.SETTINGS_DEV_TEXT_SIZE, FontStyles.Bold);

            // The trial font renders "+" as a placeholder sliver glyph, so the increment
            // button uses the authored plus art instead of a text label.
            UIFactory.CreateSpriteButton(root, "Plus", UiArt.Load("ui_consumable_plus"),
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
    }
}
