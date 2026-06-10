using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DogtorBurguer
{
    public class SettingsPanel : MonoBehaviour
    {
        private GameObject _panel;
        private Canvas _canvas;
        private TextMeshProUGUI _soundLabel;
        private TextMeshProUGUI _controlLabel;
        private TextMeshProUGUI _levelLabel;

        /// <summary>Injects the menu canvas to build into (F-77), instead of scanning the scene.</summary>
        public void Initialize(Canvas canvas)
        {
            _canvas = canvas;
        }

        public void Show()
        {
            if (_panel != null)
            {
                _panel.SetActive(true);
                UpdateSoundLabel();
                UpdateControlLabel();
                UpdateLevelLabel();
                return;
            }

            CreatePanel();
        }

        public void Hide()
        {
            if (_panel != null)
                _panel.SetActive(false);
        }

        private void CreatePanel()
        {
            // Overlay container
            _panel = UIFactory.CreateOverlay(_canvas.transform, UIStyles.OVERLAY_DARK);

            // Inner panel
            GameObject inner = UIFactory.CreatePanel(_panel.transform, UIStyles.SETTINGS_PANEL_SIZE, UIStyles.INNER_PANEL_BG);

            // Title
            UIFactory.CreateText(inner.transform, "Settings", UIStyles.SETTINGS_TITLE_POS, UIStyles.SETTINGS_TITLE_RECT,
                UIStyles.PANEL_TITLE_SIZE, FontStyles.Bold);

            // Sound toggle button (label text is set by UpdateSoundLabel; the
            // create-time string is the GameObject name)
            var soundBtn = UIFactory.CreateButton(inner.transform, "Sound", UIStyles.SETTINGS_SOUND_POS,
                UIStyles.SETTINGS_BUTTON_SIZE, UIStyles.BTN_SETTINGS_TOGGLE,
                UIStyles.SETTINGS_BUTTON_TEXT_SIZE, OnSoundToggleClicked);
            _soundLabel = soundBtn.label;
            UpdateSoundLabel();

            // Control mode toggle button (label text set by UpdateControlLabel)
            var controlBtn = UIFactory.CreateButton(inner.transform, "Controls", UIStyles.SETTINGS_CONTROL_POS,
                UIStyles.SETTINGS_BUTTON_SIZE, UIStyles.BTN_SETTINGS_TOGGLE,
                UIStyles.SETTINGS_BUTTON_TEXT_SIZE, OnControlToggleClicked);
            _controlLabel = controlBtn.label;
            UpdateControlLabel();

            // Starting-level stepper: [−] value [+]. Buttons step the persisted
            // StartingLevel by one, clamped 1..SETTINGS_LEVEL_CAP; the label shows the value.
            UIFactory.CreateButton(inner.transform, "-", UIStyles.SETTINGS_LEVEL_MINUS_POS,
                UIStyles.SETTINGS_STEPPER_BTN_SIZE, UIStyles.BTN_SETTINGS_TOGGLE,
                UIStyles.SETTINGS_BUTTON_TEXT_SIZE, () => OnLevelStep(-1));

            _levelLabel = UIFactory.CreateText(inner.transform, "Start Level: 1", UIStyles.SETTINGS_LEVEL_POS,
                UIStyles.SETTINGS_STEPPER_LABEL_SIZE, UIStyles.SETTINGS_BUTTON_TEXT_SIZE, FontStyles.Bold);
            UpdateLevelLabel();

            UIFactory.CreateButton(inner.transform, "+", UIStyles.SETTINGS_LEVEL_PLUS_POS,
                UIStyles.SETTINGS_STEPPER_BTN_SIZE, UIStyles.BTN_SETTINGS_TOGGLE,
                UIStyles.SETTINGS_BUTTON_TEXT_SIZE, () => OnLevelStep(1));

            // Close button
            UIFactory.CreateButton(inner.transform, "Close", UIStyles.SETTINGS_CLOSE_POS,
                UIStyles.CLOSE_BUTTON_SIZE, UIStyles.BTN_CLOSE,
                UIStyles.SETTINGS_BUTTON_TEXT_SIZE, Hide);
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
            _levelLabel.text = $"Start Level: {level}";
        }
    }
}
