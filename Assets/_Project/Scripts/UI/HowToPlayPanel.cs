using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DogtorBurguer
{
    /// <summary>
    /// The HOW TO PLAY panel on the shared ModalPanel chrome, opened from the top bar "?" button
    /// — in-game AND on the main menu, same display everywhere. Laid out to the artist's reference
    /// (2026-09-26): a darker rounded subpanel in the cream body carries the page — a lime header
    /// centred on its top edge, a vertical LAYOUT of bullets (each auto-sizes to its wrapped height,
    /// constant gap; a long page steps its text size down to fit), and the "1/6" pager with the
    /// green arrows inside its bottom — while PLAY TUTORIAL straddles the sheet's bottom edge.
    /// Layout knobs: UIStyles.HOWTO_*.
    /// </summary>
    public class HowToPlayPanel : MonoBehaviour
    {
        // One (header, bullets) per page — resolved through Loc on every access, so a
        // language change is live on the next open (a cached static array would go stale).
        private static (string Header, string[] Bullets)[] Pages => new[]
        {
            (Loc.Get(LocKey.HowToControls), new[] { Loc.Get(LocKey.HowToControls1),
                Loc.Get(LocKey.HowToControls2), Loc.Get(LocKey.HowToControls3), Loc.Get(LocKey.HowToControls4) }),
            (Loc.Get(LocKey.HowToMatching), new[] { Loc.Get(LocKey.HowToMatching1), Loc.Get(LocKey.HowToMatching2) }),
            (Loc.Get(LocKey.HowToBurgers), new[] { Loc.Get(LocKey.HowToBurgers1),
                Loc.Get(LocKey.HowToBurgers2), Loc.Get(LocKey.HowToBurgers3), Loc.Get(LocKey.HowToBurgers4) }),
            (Loc.Get(LocKey.HowToOrders), new[] { Loc.Get(LocKey.HowToOrders1), Loc.Get(LocKey.HowToOrders2) }),
            (Loc.Get(LocKey.PowerUps), new[] { Loc.Get(LocKey.HowToPowerUpsA1), Loc.Get(LocKey.HowToPowerUpsA2) }),
            (Loc.Get(LocKey.PowerUps), new[] { Loc.Get(LocKey.HowToPowerUpsB1),
                Loc.Get(LocKey.HowToPowerUpsB2), Loc.Get(LocKey.HowToPowerUpsB3) }),
        };

        private const string PagerSlash = "/"; // plain since the Baloo swap (the trial slivered it)

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

            // Show first: SetPage measures the bullet layout to fit it, and an inactive panel
            // skips layout (the fit would silently do nothing).
            _modal.Show();
            SetPage(0);
        }

        public void Hide()
        {
            if (_modal == null) return;

            _modal.Hide();
            OnClosed?.Invoke();
        }

        private void CreatePanel()
        {
            // The taller 4-row sheet (2026-09-08): translated bullet pages outgrew the short
            // modal body; the extra height is claimed by the HOWTO_* Y knobs.
            _modal = ModalPanel.Build(_canvas, Loc.Get(LocKey.HowToTitle), "ui_settings_panel", Vector2.zero, Vector2.zero, Hide);

            // Layout to the artist's reference (2026-09-26, Fixes/HowToReference.png): a darker
            // rounded SUBPANEL inset in the cream body holds the page — header centred on its top
            // edge, the bullets, then the pager inside its bottom — and PLAY TUTORIAL straddles the
            // sheet's bottom edge as the big green button.
            BuildSubpanel();

            _header = UIFactory.CreateText(_modal.Panel, "", new Vector2(0f, UIStyles.HOWTO_SUBPANEL_TOP),
                new Vector2(UIStyles.HOWTO_SUBPANEL_W - 2f * UIStyles.HOWTO_SUBPANEL_PAD, UIStyles.HOWTO_HEADER_SIZE * 2f),
                UIStyles.HOWTO_HEADER_SIZE, FontStyles.Bold, alignment: TextAlignmentOptions.Capline);
            ShopWidgets.StyleAccent(_header);
            UIFactory.AutoFit(_header, UIStyles.HOWTO_HEADER_SIZE_MIN, UIStyles.HOWTO_HEADER_SIZE);

            _bulletList = BuildBulletList();

            // Pager row inside the subpanel's bottom: [<] 1/6 [>] — the artist's green arrow, drawn
            // pointing right; the left one is its mirror.
            _pager = UIFactory.CreateText(_modal.Panel, "", new Vector2(0f, UIStyles.HOWTO_PAGER_Y),
                new Vector2(120f, 40f), UIStyles.HOWTO_PAGER_SIZE, FontStyles.Bold, alignment: TextAlignmentOptions.Capline);
            UIFactory.StyleHudText(_pager);

            _prevArrow = BuildArrow("Prev", -UIStyles.HOWTO_ARROW_X, -1);
            _nextArrow = BuildArrow("Next", UIStyles.HOWTO_ARROW_X, 1);

            // PLAY TUTORIAL — the big green blank (the menu PLAY's), its face centred ON the sheet's
            // bottom edge. Loads the game scene in tutorial mode; from an in-game opener this
            // forfeits the paused run (like Quit).
            Sprite blank = UiArt.Load("ui_play_button");
            Vector2 size = UIFactory.SizeByWidth(blank, UIStyles.HOWTO_TUTORIAL_W);
            Rect face = ButtonFace.Of(blank, size);
            Button tut = UIFactory.CreateSpriteButton(_modal.Panel, "PlayTutorial", blank, new Vector2(0.5f, 0.5f),
                new Vector2(-face.center.x, UIStyles.HOWTO_TUTORIAL_Y - face.center.y), size, () =>
                {
                    TutorialMode.Pending = true;
                    SceneLoader.LoadGame();
                });
            UIFactory.CreateFaceLabel(tut.transform, blank, size, Loc.Get(LocKey.HowToPlayTutorial),
                UIStyles.HOWTO_TUTORIAL_LABEL_SIZE, UIStyles.HOWTO_TUTORIAL_LABEL_SIZE_MIN);
        }

        // The darker rounded panel the page sits in (procedural 9-sliced rounded rect, tinted).
        private void BuildSubpanel()
        {
            float h = UIStyles.HOWTO_SUBPANEL_TOP - UIStyles.HOWTO_SUBPANEL_BOTTOM;
            Image sub = UIFactory.CreateImage(_modal.Panel, "Subpanel", SpriteFactory.RoundedRect(),
                new Vector2(0.5f, 0.5f),
                new Vector2(0f, (UIStyles.HOWTO_SUBPANEL_TOP + UIStyles.HOWTO_SUBPANEL_BOTTOM) * 0.5f),
                new Vector2(UIStyles.HOWTO_SUBPANEL_W, h));
            sub.type = Image.Type.Sliced;
            sub.pixelsPerUnitMultiplier = SpriteFactory.ROUNDED_RECT_RADIUS / UIStyles.HOWTO_SUBPANEL_RADIUS;
            sub.color = UIStyles.HOWTO_SUBPANEL_COLOR;
            sub.raycastTarget = false;
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
            rect.sizeDelta = new Vector2(UIStyles.HOWTO_SUBPANEL_W - 2f * UIStyles.HOWTO_SUBPANEL_PAD, 0f);

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

        // step -1 = the mirrored (left) arrow: flipped by scale, not rotated, so its dot shading
        // and the drop shadow under it stay the right way up.
        private GameObject BuildArrow(string name, float x, int step)
        {
            Sprite arrow = UiArt.Load("ui_arrow_pager");
            Button btn = UIFactory.CreateSpriteButton(_modal.Panel, name, arrow,
                new Vector2(0.5f, 0.5f), new Vector2(x, UIStyles.HOWTO_PAGER_Y),
                UIFactory.SizeByHeight(arrow, UIStyles.HOWTO_ARROW_H), () => SetPage(_page + step));
            btn.transform.localScale = new Vector3(step < 0 ? -1f : 1f, 1f, 1f);
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

            TextMeshProUGUI[] lines = new TextMeshProUGUI[bullets.Length];
            for (int i = 0; i < bullets.Length; i++)
            {
                lines[i] = UIFactory.CreateText(_bulletList, bullets[i],
                    Vector2.zero, Vector2.zero, UIStyles.HOWTO_TEXT_SIZE, FontStyles.Normal,
                    UIStyles.HOWTO_TEXT_COLOR, TextAlignmentOptions.TopLeft, wrap: true);
                lines[i].gameObject.name = "Bullet" + i;
            }
            FitBullets(lines);

            _pager.text = (_page + 1) + PagerSlash + Pages.Length;
            _prevArrow.SetActive(_page > 0);
            _nextArrow.SetActive(_page < Pages.Length - 1);
        }

        // Big text by default, but a long page (German, four wrapped bullets) must never run into
        // the pager: step the whole page's size down together until the list fits its space.
        private void FitBullets(TextMeshProUGUI[] lines)
        {
            float room = UIStyles.HOWTO_BULLETS_TOP_Y - UIStyles.HOWTO_BULLETS_BOTTOM_Y;
            for (float size = UIStyles.HOWTO_TEXT_SIZE; size >= UIStyles.HOWTO_TEXT_SIZE_MIN; size -= 1f)
            {
                foreach (TextMeshProUGUI line in lines) line.fontSize = size;
                LayoutRebuilder.ForceRebuildLayoutImmediate(_bulletList);
                if (_bulletList.rect.height <= room) return;
            }
        }

        private void OnDestroy() => _modal?.Kill();
    }
}
