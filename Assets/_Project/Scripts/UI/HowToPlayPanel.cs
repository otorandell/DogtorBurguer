using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DogtorBurguer
{
    /// <summary>
    /// The HOW TO PLAY panel on the shared ModalPanel chrome, opened from the top bar "?" button
    /// — in-game AND on the main menu, same display everywhere. Paginated: a lime page header
    /// over a vertical LAYOUT of dash bullets — each bullet auto-sizes to its wrapped height
    /// (VerticalLayoutGroup + ContentSizeFitter), so multi-line bullets can never overlap and
    /// the gap between bullets is constant. Bullets are REGULAR weight (synthetic bold on the
    /// ExtraBold trial font smears). Pager "1/6" (fallback-material slash) + rotated yellow
    /// arrows flip pages. Layout knobs: UIStyles.HOWTO_*.
    /// </summary>
    public class HowToPlayPanel : MonoBehaviour
    {
        // One (header, bullets) per page. Trial-font safe: letters, digits and . , ; : - ! ?
        // only (no apostrophes or symbols — see CLAUDE.md trial-font note).
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
        private TextMeshProUGUI _header;
        private RectTransform _bulletList;
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

            _header = UIFactory.CreateText(_modal.Panel, "", new Vector2(0f, UIStyles.HOWTO_HEADER_Y),
                new Vector2(UIStyles.HOWTO_LINE_W, 40f), UIStyles.HOWTO_HEADER_SIZE, FontStyles.Bold);
            ShopWidgets.StyleAccent(_header);

            _bulletList = BuildBulletList();

            // Pager row: [<] 1/6 [>] — the yellow preview arrow art, rotated sideways.
            _pager = UIFactory.CreateText(_modal.Panel, "", new Vector2(0f, UIStyles.HOWTO_PAGER_Y),
                new Vector2(120f, 40f), UIStyles.HOWTO_PAGER_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(_pager);

            _prevArrow = BuildArrow("Prev", -UIStyles.HOWTO_ARROW_X, UIStyles.HOWTO_ARROW_ROT_LEFT, -1);
            _nextArrow = BuildArrow("Next", UIStyles.HOWTO_ARROW_X, UIStyles.HOWTO_ARROW_ROT_RIGHT, 1);
        }

        // The bullet list: a top-anchored vertical layout that measures each bullet's wrapped
        // height (TMP is an ILayoutElement) and stacks them with one constant gap — Unity's own
        // text-flow mechanism, replacing the fixed-pitch rows that overlapped on long bullets.
        private RectTransform BuildBulletList()
        {
            GameObject obj = new GameObject("Bullets");
            obj.transform.SetParent(_modal.Panel, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 1f); // grows downward from a fixed top edge
            rect.anchoredPosition = new Vector2(0f, UIStyles.HOWTO_BULLETS_TOP_Y);
            rect.sizeDelta = new Vector2(UIStyles.HOWTO_LINE_W, 0f);

            VerticalLayoutGroup layout = obj.AddComponent<VerticalLayoutGroup>();
            layout.spacing = UIStyles.HOWTO_BULLET_GAP;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            obj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rect;
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

            // Deactivate before the deferred Destroy so the layout ignores the old bullets
            // immediately (a destroyed-this-frame child still counts in the layout pass).
            for (int i = _bulletList.childCount - 1; i >= 0; i--)
            {
                GameObject old = _bulletList.GetChild(i).gameObject;
                old.SetActive(false);
                Destroy(old);
            }

            (string header, string[] bullets) = Pages[_page];
            _header.text = header;

            for (int i = 0; i < bullets.Length; i++)
            {
                TextMeshProUGUI line = UIFactory.CreateText(_bulletList, bullets[i],
                    Vector2.zero, Vector2.zero, UIStyles.HOWTO_TEXT_SIZE, FontStyles.Normal,
                    UIStyles.TOPBAR_NUMBER_COLOR, TextAlignmentOptions.TopLeft, wrap: true);
                line.gameObject.name = "Bullet" + i;
            }

            _pager.text = (_page + 1) + PagerSlash + Pages.Length;
            _prevArrow.SetActive(_page > 0);
            _nextArrow.SetActive(_page < Pages.Length - 1);
        }

        private void OnDestroy() => _modal?.Kill();
    }
}
