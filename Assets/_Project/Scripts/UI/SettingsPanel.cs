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

        public void Show()
        {
            if (_panel != null)
            {
                _panel.SetActive(true);
                UpdateSoundLabel();
                UpdateControlLabel();
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
            _canvas = FindAnyObjectByType<Canvas>();

            // Overlay container
            _panel = UIFactory.CreateOverlay(_canvas.transform, UIStyles.OVERLAY_DARK);

            // Inner panel
            GameObject inner = UIFactory.CreatePanel(_panel.transform, UIStyles.SETTINGS_PANEL_SIZE, UIStyles.INNER_PANEL_BG);

            // Title
            UIFactory.CreateText(inner.transform, "Settings", new Vector2(0, 130), new Vector2(300, 50),
                UIStyles.PANEL_TITLE_SIZE, FontStyles.Bold);

            // Sound toggle button
            var soundBtn = UIFactory.CreateButton(inner.transform, "", new Vector2(0, 50),
                UIStyles.SETTINGS_BUTTON_SIZE, UIStyles.BTN_SETTINGS_TOGGLE,
                UIStyles.SETTINGS_BUTTON_TEXT_SIZE, OnSoundToggleClicked);
            _soundLabel = soundBtn.label;
            UpdateSoundLabel();

            // Control mode toggle button
            var controlBtn = UIFactory.CreateButton(inner.transform, "", new Vector2(0, -20),
                UIStyles.SETTINGS_BUTTON_SIZE, UIStyles.BTN_SETTINGS_TOGGLE,
                UIStyles.SETTINGS_BUTTON_TEXT_SIZE, OnControlToggleClicked);
            _controlLabel = controlBtn.label;
            UpdateControlLabel();

            // Close button
            UIFactory.CreateButton(inner.transform, "Close", new Vector2(0, -110),
                UIStyles.CLOSE_BUTTON_SIZE, UIStyles.BTN_CLOSE,
                UIStyles.SETTINGS_BUTTON_TEXT_SIZE, Hide);
        }

        private void OnSoundToggleClicked()
        {
            if (SaveDataManager.Instance == null) return;

            bool newState = !SaveDataManager.Instance.SoundOn;
            SaveDataManager.Instance.SetSoundOn(newState);
            AudioListener.volume = newState ? 1f : 0f;
            MusicManager.Instance?.ApplySoundSetting();
            UpdateSoundLabel();
        }

        private void UpdateSoundLabel()
        {
            if (_soundLabel == null) return;
            bool soundOn = SaveDataManager.Instance != null ? SaveDataManager.Instance.SoundOn : true;
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
                : ControlMode.Drag;
            _controlLabel.text = mode == ControlMode.Drag ? "Controls: Drag" : "Controls: Tap";
        }
    }
}
