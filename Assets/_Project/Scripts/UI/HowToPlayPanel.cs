using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DogtorBurguer
{
    /// <summary>
    /// The HOW TO PLAY panel on the shared ModalPanel chrome, opened from the top bar "?" button
    /// — in-game AND on the main menu, same display everywhere. Paginated: a lime page header +
    /// short dash bullets (regular weight — synthetic bold on the ExtraBold trial font smears),
    /// rotated yellow preview arrows to flip, and a "1/6" pager (its slash renders through the
    /// LiberationSans sticker material — the trial font slivers it). Layout: UIStyles.HOWTO_*.
    /// </summary>
    public class HowToPlayPanel : MonoBehaviour
    {
        // One (header, bullets) per page, max ~4 bullets. Trial-font safe: letters, digits and
        // . , ; : - ! ? only (no apostrophes or symbols — see CLAUDE.md trial-font note).
        private static readonly (string Header, string[] Bullets)[] Pages =
        {
            ("CONTROLS", new[]
            {
                "- Swipe left or right to move the chef.",
                "- Tap the chef to rotate his plates.",
                "- Tap a falling ingredient to drop it faster.",
                "- Tap a preview arrow to spawn it right away.",
            }),
            ("MATCHING", new[]
            {
                "- Two stacked ingredients of the same type pop and score points.",
                "- Keep the columns low: if one overflows, the run ends!",
            }),
            ("BURGERS", new[]
            {
                "- Open with a bottom bun.",
                "- Stack any ingredients on top.",
                "- Close with a top bun to serve the burger.",
                "- Bigger burgers score more!",
            }),
            ("SPECIAL ORDERS", new[]
            {
                "- Serve a burger that fits the shown order.",
                "- Orders pay stars and raise your multiplier.",
            }),
            ("POWER-UPS", new[]
            {
                "- The Burger Fairy brings gems, stars and power-ups. Tap her before she leaves!",
                "- Drag a power-up from its slot onto a column to use it.",
            }),
            ("POWER-UPS", new[]
            {
                "- Ketchup clears its column.",
                "- Mustard sweeps the top two ingredient types of its column from the whole board.",
                "- Skewer slams its column down onto the bottom bun.",
            }),
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

            // Pager row: [<] 1/6 [>] — the yellow preview arrow art, rotated sideways.
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

            (string header, string[] bullets) = Pages[_page];

            TextMeshProUGUI headerText = UIFactory.CreateText(_linesRoot, header,
                new Vector2(0f, UIStyles.HOWTO_HEADER_Y), new Vector2(UIStyles.HOWTO_LINE_W, 40f),
                UIStyles.HOWTO_HEADER_SIZE, FontStyles.Bold);
            ShopWidgets.StyleAccent(headerText);

            // TopLeft-aligned: every bullet's FIRST text line starts at its rect top, so the
            // header gap is identical on every page (midline centering made wrapping bullets
            // float up toward the header on some pages).
            for (int i = 0; i < bullets.Length; i++)
            {
                TextMeshProUGUI line = UIFactory.CreateText(_linesRoot, bullets[i],
                    new Vector2(0f, UIStyles.HOWTO_BULLET_TOP_Y - i * UIStyles.HOWTO_BULLET_PITCH),
                    new Vector2(UIStyles.HOWTO_LINE_W, UIStyles.HOWTO_BULLET_PITCH),
                    UIStyles.HOWTO_TEXT_SIZE, FontStyles.Normal, UIStyles.TOPBAR_NUMBER_COLOR,
                    TextAlignmentOptions.TopLeft, wrap: true);
                line.gameObject.name = "Bullet" + i;
            }

            _pager.text = (_page + 1) + PagerSlash + Pages.Length;
            _prevArrow.SetActive(_page > 0);
            _nextArrow.SetActive(_page < Pages.Length - 1);
        }

        private void OnDestroy() => _modal?.Kill();
    }
}
