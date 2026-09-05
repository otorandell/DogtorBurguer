using UnityEngine;
using TMPro;

namespace DogtorBurguer
{
    /// <summary>
    /// The HOW TO PLAY panel on the shared ModalPanel chrome: the core rules as short brown
    /// text lines down the body. Opened from the in-game top bar "?" button (which replaced the
    /// shop button 2026-09-05 — the shop stays reachable from the menu and the consumable plus
    /// box). Layout knobs: UIStyles.HOWTO_*.
    /// </summary>
    public class HowToPlayPanel : MonoBehaviour
    {
        // One rule per line. Trial-font safe: letters, digits and . , ; : - ! ? only (no
        // apostrophes or symbols — see CLAUDE.md trial-font note).
        private static readonly string[] Lines =
        {
            "Move the chef left and right; tap him to rotate the plates.",
            "Combine two ingredients of the same type to pop them and score points.",
            "Make burgers: open with a bottom bun, stack ingredients on it, close with a top bun. Bigger burgers score more!",
            "Complete Special Orders to earn stars and raise your point multiplier.",
            "Catch the Burger Fairy for gems, stars and power-ups!",
        };

        private Canvas _canvas;
        private ModalPanel _modal;

        /// <summary>Fired when the panel closes — the in-game opener resumes the run on this.</summary>
        public event System.Action OnClosed;

        public void Initialize(Canvas canvas) => _canvas = canvas;

        public void Show()
        {
            if (_modal == null)
                CreatePanel();

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
            _modal = ModalPanel.Build(_canvas, "HOW TO PLAY", "ui_modal_panel", Vector2.zero, Vector2.zero, Hide);

            for (int i = 0; i < Lines.Length; i++)
            {
                TextMeshProUGUI line = UIFactory.CreateText(_modal.Panel, Lines[i],
                    new Vector2(0f, UIStyles.HOWTO_TOP_Y - i * UIStyles.HOWTO_PITCH),
                    new Vector2(UIStyles.HOWTO_LINE_W, UIStyles.HOWTO_PITCH),
                    UIStyles.HOWTO_TEXT_SIZE, FontStyles.Bold, UIStyles.TOPBAR_NUMBER_COLOR,
                    TextAlignmentOptions.Left, wrap: true);
                line.gameObject.name = "Rule" + i;
            }
        }

        private void OnDestroy() => _modal?.Kill();
    }
}
