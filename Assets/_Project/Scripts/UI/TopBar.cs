using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DogtorBurguer
{
    /// <summary>
    /// The shared top status bar: high-score trophy + star + gem currency pills at fixed
    /// positions (UIStyles.TOPBAR_*), plus optional help ("?")/settings icon buttons. One recipe
    /// for every screen (game HUD, main menu, shop header) so the bar looks identical and
    /// stays put when screens change. Binds itself to the SaveDataManager currency events
    /// and punches a pill on change (unscaled time — the shop header sits on a paused run).
    /// </summary>
    public class TopBar : MonoBehaviour
    {
        private TextMeshProUGUI _highScoreNumber;
        private TextMeshProUGUI _starNumber;
        private TextMeshProUGUI _gemNumber;

        /// <summary>The gem pill transform — deny-shake target for failed gem spends.</summary>
        public Transform GemPill => _gemNumber.transform.parent;

        /// <summary>Builds the bar under a canvas. Null callbacks omit their icon button; the
        /// "?" + gear pair sits at the same spot and size on EVERY screen (menu = in-game — the
        /// menu's bigger-gear override was dropped 2026-09-05 for consistency).</summary>
        public static TopBar Build(Transform canvas, Action onHelp = null, Action onSettings = null)
        {
            GameObject obj = new GameObject("TopBar");
            obj.transform.SetParent(canvas, false);
            RectTransform rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            TopBar bar = obj.AddComponent<TopBar>();
            bar.BuildContents(onHelp, onSettings);
            return bar;
        }

        // Transparent margins of the bar's authored blanks, as fractions of the canvas width
        // (left, right) — alpha bbox, measured 2026-09-26. The layout works on VISIBLE edges, so
        // "same space left and right" is true of what the eye sees, not of the canvases.
        private static readonly Vector2 BoxMargins = new(0.0959f, 0.0817f);      // ui_currency_box
        private static readonly Vector2 HelpMargins = new(0.1346f, 0.1268f);     // ui_btn_square_green
        private static readonly Vector2 ConfigMargins = new(0.1076f, 0.1286f);  // ui_config_button

        private static readonly (string Art, float IconH)[] Pills =
        {
            ("ui_score_trophy", UIStyles.TOPBAR_SCORE_ICON_H),
            ("ui_star", UIStyles.TOPBAR_STAR_ICON_H),
            ("ui_gem", UIStyles.TOPBAR_GEM_ICON_H),
        };

        // Layout (artist note 2026-09-26): the row runs trophy · star · gem · ? · gear between two
        // EQUAL side margins (TOPBAR_SIDE_MARGIN, visible edges), items spaced evenly. A bar
        // without the buttons (the shop header) keeps that same spacing and centres the pills.
        private void BuildContents(Action onHelp, Action onSettings)
        {
            float[] pillLeft = new float[Pills.Length];
            float[] pillRight = new float[Pills.Length];
            float pillsW = 0f;
            for (int i = 0; i < Pills.Length; i++)
            {
                (pillLeft[i], pillRight[i]) = PillExtent(UiArt.Load(Pills[i].Art), Pills[i].IconH);
                pillsW += pillRight[i] - pillLeft[i];
            }
            float s = UIStyles.TOPBAR_BUTTON_SIZE.x;
            float helpW = s * (1f - HelpMargins.x - HelpMargins.y);
            float configW = s * (1f - ConfigMargins.x - ConfigMargins.y);

            float rowW = UIStyles.REFERENCE_RESOLUTION.x - 2f * UIStyles.TOPBAR_SIDE_MARGIN;
            float gap = (rowW - pillsW - helpW - configW) / (Pills.Length + 1);
            bool buttons = onHelp != null || onSettings != null;
            float cursor = buttons ? -rowW * 0.5f : -(pillsW + gap * (Pills.Length - 1)) * 0.5f;

            TextMeshProUGUI[] numbers = new TextMeshProUGUI[Pills.Length];
            for (int i = 0; i < Pills.Length; i++)
            {
                numbers[i] = BuildCurrencyWidget(Pills[i].Art, Pills[i].Art, cursor - pillLeft[i], Pills[i].IconH);
                cursor += pillRight[i] - pillLeft[i] + gap;
            }
            _highScoreNumber = numbers[0];
            _starNumber = numbers[1];
            _gemNumber = numbers[2];

            // The trophy pill doubles as the leaderboard button (2026-09-06) — tap opens the
            // Play Games board (the editor mock just logs). Zero extra layout.
            Image trophyBox = _highScoreNumber.transform.parent.GetComponent<Image>();
            trophyBox.raycastTarget = true;
            trophyBox.gameObject.AddComponent<Button>().onClick.AddListener(
                () => LeaderboardManager.Instance?.ShowLeaderboard());

            if (onHelp != null)
            {
                // The "?" help button (replaced the in-game shop button 2026-09-05): the kit's
                // blank green square with a HUD-palette question mark on it.
                float x = cursor + s * (0.5f - HelpMargins.x);
                Button help = UIFactory.CreateSpriteButton(transform, "HelpButton", UiArt.Load("ui_btn_square_green"),
                    TopCenter, new Vector2(x, UIStyles.TOPBAR_Y), UIStyles.TOPBAR_BUTTON_SIZE, () => onHelp());
                TextMeshProUGUI mark = UIFactory.CreateText(help.transform, "?", Vector2.zero,
                    UIStyles.TOPBAR_BUTTON_SIZE, UIStyles.HOWTO_BTN_TEXT_SIZE, FontStyles.Bold);
                UIFactory.StyleHudText(mark);
            }
            cursor += helpW + gap;
            if (onSettings != null)
            {
                float x = cursor + s * (0.5f - ConfigMargins.x);
                UIFactory.CreateSpriteButton(transform, "ConfigButton", UiArt.Load("ui_config_button"),
                    TopCenter, new Vector2(x, UIStyles.TOPBAR_Y), UIStyles.TOPBAR_BUTTON_SIZE, () => onSettings());
            }

            SaveDataManager save = SaveDataManager.Instance;
            // High score only changes at game over, so a one-time seed is enough (no live event).
            _highScoreNumber.text = NumberFormat.Abbreviate(save != null ? save.HighScore : 0);
            _starNumber.text = NumberFormat.Abbreviate(save != null ? save.Stars : 0);
            _gemNumber.text = NumberFormat.Abbreviate(save != null ? save.Gems : 0);
            if (save != null)
            {
                save.OnStarsChanged += HandleStarsChanged;
                save.OnGemsChanged += HandleGemsChanged;
            }
        }

        private static readonly Vector2 TopCenter = new(0.5f, 1f);

        // A pill's visible extent relative to the pill's centre: from the icon's (or the box's)
        // left edge to the box's visible right edge.
        private static (float Left, float Right) PillExtent(Sprite icon, float iconH)
        {
            float boxW = UIStyles.TOPBAR_BOX_SIZE.x;
            float iconW = IconWidth(icon, iconH);
            float left = Mathf.Min(UIStyles.TOPBAR_ICON_X - iconW * 0.5f, -boxW * 0.5f + boxW * BoxMargins.x);
            return (left, boxW * 0.5f - boxW * BoxMargins.y);
        }

        // Icons are sized by height, width following native aspect — never force a square (distorts).
        private static float IconWidth(Sprite icon, float iconH) =>
            icon != null ? iconH * icon.rect.width / icon.rect.height : iconH;

        // A currency pill (baked box) with an overhanging icon (bigger than the pill) and a number
        // centred in the free zone between the icon and the box's right edge; returns the number.
        private TextMeshProUGUI BuildCurrencyWidget(string name, string iconArt, float x, float iconHeight)
        {
            Vector2 boxSize = UIStyles.TOPBAR_BOX_SIZE;
            Image box = UIFactory.CreateImage(transform, name, UiArt.Load("ui_currency_box"),
                TopCenter, new Vector2(x, UIStyles.TOPBAR_Y), boxSize);

            Sprite iconSprite = UiArt.Load(iconArt);
            float iconW = IconWidth(iconSprite, iconHeight);
            UIFactory.CreateImage(box.transform, "Icon", iconSprite,
                new Vector2(0.5f, 0.5f), new Vector2(UIStyles.TOPBAR_ICON_X, UIStyles.TOPBAR_ICON_Y),
                new Vector2(iconW, iconHeight));

            float zoneLeft = UIStyles.TOPBAR_ICON_X + iconW * 0.5f + UIStyles.TOPBAR_NUMBER_SIDE_PAD;
            float zoneRight = boxSize.x * 0.5f - boxSize.x * BoxMargins.y - UIStyles.TOPBAR_NUMBER_SIDE_PAD;
            // Solid brown, no sticker lettering (artist note 2026-09-26 — the cream HUD palette
            // used here since 2026-09-03 read as busy on the pill).
            TextMeshProUGUI number = UIFactory.CreateText(box.transform, "0",
                new Vector2((zoneLeft + zoneRight) * 0.5f, UIStyles.TOPBAR_NUMBER_Y),
                new Vector2(zoneRight - zoneLeft, boxSize.y),
                UIStyles.TOPBAR_NUMBER_SIZE, FontStyles.Bold, UIStyles.TOPBAR_NUMBER_COLOR,
                TextAlignmentOptions.Capline); // caps centred in the free zone regardless of digit count
            // Shrink-to-fit: the box stays fixed and the text scales down to stay inside it.
            number.textWrappingMode = TextWrappingModes.NoWrap;
            number.enableAutoSizing = true;
            number.fontSizeMin = UIStyles.TOPBAR_NUMBER_SIZE_MIN;
            number.fontSizeMax = UIStyles.TOPBAR_NUMBER_SIZE;
            return number;
        }

        private void HandleStarsChanged(int stars)
        {
            _starNumber.text = NumberFormat.Abbreviate(stars);
            Punch(_starNumber.transform.parent);
        }

        private void HandleGemsChanged(int gems)
        {
            _gemNumber.text = NumberFormat.Abbreviate(gems);
            Punch(_gemNumber.transform.parent);
        }

        // Runs on unscaled time — the shop header's bar sits on a paused (timeScale 0) run.
        private static void Punch(Transform target)
        {
            target.DOKill(true);
            target.DOPunchScale(Vector3.one * AnimConfig.SHOP_PILL_PUNCH_SCALE,
                    AnimConfig.SHOP_PILL_PUNCH_DURATION, 6, 0.7f)
                .SetUpdate(true).SetLink(target.gameObject);
        }

        private void OnDestroy()
        {
            SaveDataManager save = SaveDataManager.Instance;
            if (save != null)
            {
                save.OnStarsChanged -= HandleStarsChanged;
                save.OnGemsChanged -= HandleGemsChanged;
            }
        }
    }
}
