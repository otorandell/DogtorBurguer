using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DogtorBurguer
{
    /// <summary>
    /// The HOW TO PLAY panel on the shared ModalPanel chrome, opened from the top bar "?" button
    /// — in-game AND on the main menu, same display everywhere. Paginated: short brown rule
    /// lines per page, rotated yellow preview arrows to flip, and a "1/3" pager (its slash
    /// renders through the LiberationSans sticker material — the trial font slivers it).
    /// Layout knobs: UIStyles.HOWTO_*.
    /// </summary>
    public class HowToPlayPanel : MonoBehaviour
    {
        // One string[] per page. Trial-font safe: letters, digits and . , ; : - ! ? only (no
        // apostrophes or symbols — see CLAUDE.md trial-font note).
        private static readonly string[][] Pages =
        {
            new[]
            {
                "Move the chef left and right; tap him to rotate the plates.",
                "Combine two ingredients of the same type to pop them and score points.",
                "Tap a falling ingredient to drop it faster; tap a preview arrow to call it down now.",
            },
            new[]
            {
                "Make burgers: open with a bottom bun, stack ingredients on it and close with a top bun. Bigger burgers score more!",
                "Complete Special Orders to earn stars and raise your point multiplier.",
            },
            new[]
            {
                "Catch the Burger Fairy for gems, stars and power-ups!",
                "Drag a power-up from its slot onto a column to use it:",
                "Ketchup clears its column. Mustard clears the top type from the whole board. Skewer slams its column down onto the bottom bun.",
            },
        };

        private const string PagerSlash =
            "<font=\"LiberationSans SDF\" material=\"LiberationSans SDF - Sticker\">/</font>";

        private Canvas _canvas;
        private ModalPanel _modal;
        private Transform _linesRoot;
        private TextMeshProUGUI _pager;
        private GameObject _prevArrow;
        private GameObject _nextArrow;
        private int _page;

        /// <summary>Fired when the panel closes — the in-game opener resumes the run on this.</summary>
        public event System.Action OnClosed;

        public void Initialize(Canvas canvas) => _canvas = canvas;

        public void Show()
        {
            if (_modal == null)
                CreatePanel();

            SetPage(0);
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

            GameObject linesObj = new GameObject("Lines");
            linesObj.transform.SetParent(_modal.Panel, false);
            RectTransform linesRect = linesObj.AddComponent<RectTransform>();
            linesRect.anchorMin = linesRect.anchorMax = new Vector2(0.5f, 0.5f);
            linesRect.sizeDelta = Vector2.zero;
            _linesRoot = linesObj.transform;

            // Pager row: [<] 1/3 [>] — the yellow preview arrow art, rotated sideways.
            _pager = UIFactory.CreateText(_modal.Panel, "", new Vector2(0f, UIStyles.HOWTO_PAGER_Y),
                new Vector2(120f, 40f), UIStyles.HOWTO_PAGER_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(_pager);

            _prevArrow = BuildArrow("Prev", -UIStyles.HOWTO_ARROW_X, UIStyles.HOWTO_ARROW_ROT_LEFT, -1);
            _nextArrow = BuildArrow("Next", UIStyles.HOWTO_ARROW_X, UIStyles.HOWTO_ARROW_ROT_RIGHT, 1);
        }

        private GameObject BuildArrow(string name, float x, float zRotation, int step)
        {
            Sprite arrow = UiArt.Load("ui_arrow_yellow");
            Button btn = UIFactory.CreateSpriteButton(_modal.Panel, name, arrow,
                new Vector2(0.5f, 0.5f), new Vector2(x, UIStyles.HOWTO_PAGER_Y),
                UIFactory.SizeByHeight(arrow, UIStyles.HOWTO_ARROW_H), () => SetPage(_page + step));
            btn.transform.localEulerAngles = new Vector3(0f, 0f, zRotation);
            return btn.gameObject;
        }

        private void SetPage(int page)
        {
            _page = Mathf.Clamp(page, 0, Pages.Length - 1);

            for (int i = _linesRoot.childCount - 1; i >= 0; i--)
                Destroy(_linesRoot.GetChild(i).gameObject);

            string[] lines = Pages[_page];
            for (int i = 0; i < lines.Length; i++)
            {
                TextMeshProUGUI line = UIFactory.CreateText(_linesRoot, lines[i],
                    new Vector2(0f, UIStyles.HOWTO_TOP_Y - i * UIStyles.HOWTO_PITCH),
                    new Vector2(UIStyles.HOWTO_LINE_W, UIStyles.HOWTO_PITCH),
                    UIStyles.HOWTO_TEXT_SIZE, FontStyles.Bold, UIStyles.TOPBAR_NUMBER_COLOR,
                    TextAlignmentOptions.Left, wrap: true);
                line.gameObject.name = "Rule" + i;
            }

            _pager.text = (_page + 1) + PagerSlash + Pages.Length;
            _prevArrow.SetActive(_page > 0);
            _nextArrow.SetActive(_page < Pages.Length - 1);
        }

        private void OnDestroy() => _modal?.Kill();
    }
}
