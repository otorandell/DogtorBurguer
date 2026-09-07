using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

namespace DogtorBurguer
{
    /// <summary>
    /// The Settings panel, on the shared ModalPanel chrome (full-canvas panel art, title, round X):
    /// wide blue rows — the Sound and Controls toggles, then the menu-only START level row (the
    /// blue blank with a yellow arrow inside each end, "START: LVL N" between — steps the persisted
    /// StartingLevel, 1..SETTINGS_LEVEL_CAP; replaced the mode toggle 2026-09-07) or the
    /// full-width Quit to Menu in-game (the level applies to the NEXT run, so in-game it would
    /// only mislead). Opened by the menu gear and the in-game top-bar gear (that one pauses the
    /// run and resumes on close). Layout knobs: UIStyles.SETTINGS_*.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        private static readonly Vector2 Center = new(0.5f, 0.5f);

        private Canvas _canvas;
        private ModalPanel _modal;
        private TextMeshProUGUI _soundLabel;
        private TextMeshProUGUI _controlLabel;
        private TextMeshProUGUI _levelLabel;
        private GameObject _levelDown;
        private GameObject _levelUp;
        private bool _showRunButtons;

        /// <summary>Fired when the panel closes — the in-game opener resumes the run on this.</summary>
        public event System.Action OnClosed;

        /// <summary>Injects the canvas to build into (F-77), instead of scanning the scene.
        /// Pass <paramref name="showRunButtons"/> from the in-game opener to get the
        /// Quit-to-menu row in place of the START level row.</summary>
        public void Initialize(Canvas canvas, bool showRunButtons = false)
        {
            _canvas = canvas;
            _showRunButtons = showRunButtons;
        }

        public void Show()
        {
            if (_modal == null)
                CreatePanel();

            UpdateSoundLabel();
            UpdateControlLabel();
            UpdateLevelRow();
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

            // Rows down the body. The label strings are set by the Update* refreshers.
            _soundLabel = CreateRowButton("Sound", new Vector2(0f, RowY(0)), UIStyles.SETTINGS_ROW_W, OnSoundToggleClicked);
            _controlLabel = CreateRowButton("Controls", new Vector2(0f, RowY(1)), UIStyles.SETTINGS_ROW_W, OnControlToggleClicked);

            // Third row: in-game a full-width Quit to Menu (the Restart half was dropped
            // 2026-09-05 — game over already offers Retry; scene loads reset timeScale, so
            // leaving from the paused panel is safe). In the menu, the START level row.
            if (_showRunButtons)
                CreateRowButton("Quit to Menu", new Vector2(0f, RowY(2)), UIStyles.SETTINGS_ROW_W, OnQuitClicked);
            else
                BuildLevelRow(new Vector2(0f, RowY(2)), UIStyles.SETTINGS_ROW_W);
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

        // The START level row: the same blue blank (not itself a button), the label in the middle
        // and a yellow arrow button inside each end. Arrows hide at the ends of the range.
        private void BuildLevelRow(Vector2 pos, float width)
        {
            Sprite blank = UiArt.Load("ui_btn_blue_wide");
            Vector2 size = UIFactory.SizeByWidth(blank, width);
            Image row = UIFactory.CreateImage(_modal.Panel, "StartLevel", blank, Center, pos, size);
            _levelLabel = CreateRowLabel(row.transform, "START: LVL 1", size);

            _levelDown = BuildLevelArrow(row.transform, "Down", -UIStyles.SETTINGS_LEVEL_ARROW_X,
                UIStyles.ARROW_YELLOW_ROT_LEFT, -1);
            _levelUp = BuildLevelArrow(row.transform, "Up", UIStyles.SETTINGS_LEVEL_ARROW_X,
                UIStyles.ARROW_YELLOW_ROT_RIGHT, 1);
        }

        private GameObject BuildLevelArrow(Transform row, string name, float x, float zRotation, int step)
        {
            Sprite arrow = UiArt.Load("ui_arrow_yellow");
            Button btn = UIFactory.CreateSpriteButton(row, name, arrow, Center, new Vector2(x, 0f),
                UIFactory.SizeByHeight(arrow, UIStyles.SETTINGS_LEVEL_ARROW_H), () => OnLevelStep(step));
            btn.transform.localEulerAngles = new Vector3(0f, 0f, zRotation);
            return btn.gameObject;
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

        private void OnLevelStep(int delta)
        {
            if (SaveDataManager.Instance == null) return;

            SaveDataManager.Instance.SetStartingLevel(SaveDataManager.Instance.StartingLevel + delta);
            UpdateLevelRow();
        }

        private void UpdateLevelRow()
        {
            if (_levelLabel == null) return;
            int level = SaveDataManager.Instance != null
                ? SaveDataManager.Instance.StartingLevel
                : SaveDataManager.DEFAULT_STARTING_LEVEL;
            _levelLabel.text = $"START: LVL {level}";
            _levelDown.SetActive(level > 1);
            _levelUp.SetActive(level < GameplayConfig.SETTINGS_LEVEL_CAP);
        }

        private void OnDestroy()
        {
            _modal?.Kill();
        }
    }
}
