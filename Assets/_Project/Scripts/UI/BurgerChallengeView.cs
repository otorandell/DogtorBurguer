using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace DogtorBurguer
{
    /// <summary>
    /// The view half of Special Orders — a screen-space UGUI panel (top-right): a dotted card + the
    /// SPECIAL ORDER banner, the required-burger ingredient stack, the requirement line, and a
    /// multiplier badge. Built at runtime by <see cref="BurgerChallenge"/> (the model) and driven by
    /// its events; owns no challenge logic.
    /// </summary>
    public class BurgerChallengeView : MonoBehaviour
    {
        private BurgerChallenge _model;
        private Canvas _canvas;
        private RectTransform _card;
        private RectTransform _stackRoot;
        private readonly List<Image> _stackImages = new List<Image>();
        private readonly List<Color> _stackBaseColors = new List<Color>(); // per-image rest color (the ghost isn't white)
        private TextMeshProUGUI _multText;
        private Image _meterFill;
        // The card area left free for the burger: left of the meter tube, under the banner (card-local px).
        private Rect _stackArea;
        // Fit factor of the current order (<= 1), applied to every stack sprite by AddSprite.
        private float _stackScale = 1f;

        /// <summary>Tutorial: hides/shows the whole panel canvas (state persists on it).</summary>
        public void SetVisible(bool visible)
        {
            if (_canvas != null) _canvas.gameObject.SetActive(visible);
        }

        public void Initialize(BurgerChallenge model)
        {
            _model = model;
            BuildPanel();

            _model.OnChallengeChanged += HandleChallengeChanged;
            _model.OnMatched += HandleMatched;
            _model.OnLevelUp += HandleLevelUp;
        }

        private void OnDestroy()
        {
            if (_meterFill != null) _meterFill.DOKill();
            if (_model == null) return;
            _model.OnChallengeChanged -= HandleChallengeChanged;
            _model.OnMatched -= HandleMatched;
            _model.OnLevelUp -= HandleLevelUp;
        }

        private void BuildPanel()
        {
            // Screen Space - Camera (like the HUD) so world sprites above SORT_CHALLENGE_BASE —
            // fairies (100), popups — fly OVER the panel instead of vanishing behind it.
            _canvas = UIFactory.CreateCanvas(transform, "ChallengeCanvas", Constants.SORT_CHALLENGE_BASE, Camera.main);

            Image card = UIFactory.CreateImage(UIFactory.SafeRoot(_canvas), "SpecialCard", UiArt.Load("ui_special_card"),
                new Vector2(1f, 1f), UIStyles.SPECIAL_CARD_POS, UIStyles.SPECIAL_CARD_SIZE);
            _card = card.rectTransform;

            // SPECIAL ORDER banner (blank art), sized by height (aspect, then stretched wider by
            // SPECIAL_BANNER_STRETCH_X — deliberate), overhanging the card's top-left, with the
            // word as TMP — like the Level/Score tabs.
            Sprite banner = UiArt.Load("ui_special_title");
            float bannerAspect = banner != null ? banner.rect.width / banner.rect.height : 1f;
            Vector2 bannerSize = new(UIStyles.SPECIAL_BANNER_H * bannerAspect * UIStyles.SPECIAL_BANNER_STRETCH_X,
                UIStyles.SPECIAL_BANNER_H);
            Image bannerImg = UIFactory.CreateImage(_card, "Banner", banner, new Vector2(0.5f, 0.5f),
                UIStyles.SPECIAL_BANNER_OFFSET, bannerSize);

            // The label rect is the VISIBLE red band, not the art canvas (transparent margins) —
            // long translations (SPEZIALBESTELLUNG) auto-shrink against the band width.
            Vector2 bannerLabelRect = new(bannerSize.x * UIStyles.SPECIAL_BANNER_LABEL_W_FRAC, bannerSize.y);
            TextMeshProUGUI bannerLabel = UIFactory.CreateText(bannerImg.transform, Loc.Get(LocKey.SpecialOrder),
                UIStyles.SPECIAL_BANNER_LABEL_OFFSET, bannerLabelRect, UIStyles.SPECIAL_BANNER_LABEL_SIZE,
                FontStyles.Bold, alignment: TextAlignmentOptions.Capline);
            UIFactory.StyleHudText(bannerLabel);
            bannerLabel.textWrappingMode = TextWrappingModes.NoWrap;
            bannerLabel.enableAutoSizing = true;
            bannerLabel.fontSizeMin = UIStyles.SPECIAL_BANNER_LABEL_SIZE_MIN;
            bannerLabel.fontSizeMax = UIStyles.SPECIAL_BANNER_LABEL_SIZE;
            // Capline centres the CAP HEIGHT, not the font's line box or the glyph bounds, so the
            // word sits at the same height whatever size auto-size settles on and whatever
            // accents the translation carries (Center drifted per language; Midline sank them).
            float bannerPad = bannerLabelRect.x * UIStyles.SPECIAL_BANNER_LABEL_SIDE_PAD_FRAC;
            bannerLabel.margin = new Vector4(bannerPad, 0f, bannerPad, 0f);

            // Burger stack container — created before the meter so the stack renders under it;
            // positioned by HandleChallengeChanged, centred in the area the meter leaves free.
            GameObject stackObj = new GameObject("Stack");
            stackObj.transform.SetParent(_card, false);
            _stackRoot = stackObj.AddComponent<RectTransform>();
            _stackRoot.anchorMin = _stackRoot.anchorMax = new Vector2(0.5f, 0.5f);
            _stackRoot.sizeDelta = Vector2.zero;

            // Mult meter (built before the badge so the badge renders on top of it). Returns the
            // tube's rect so the badge can sit on its bottom-left corner and the stack can centre
            // in what's left of the card.
            Rect tube = BuildMultMeter();

            float cardHalfW = UIStyles.SPECIAL_CARD_SIZE.x * 0.5f;
            float cardHalfH = UIStyles.SPECIAL_CARD_SIZE.y * 0.5f;
            float bannerBottom = UIStyles.SPECIAL_BANNER_OFFSET.y - UIStyles.SPECIAL_BANNER_H * 0.5f;
            float left = -cardHalfW + UIStyles.SPECIAL_CARD_INNER_PAD;
            float bottom = -cardHalfH + UIStyles.SPECIAL_CARD_INNER_PAD;
            _stackArea = Rect.MinMaxRect(left, bottom, tube.xMin - UIStyles.SPECIAL_STACK_METER_GAP, bannerBottom);

            // Multiplier badge on the tube's bottom cap, sharing its x — reuses the red num box sprite.
            Vector2 badgePos = new Vector2(tube.center.x, tube.yMin) + UIStyles.SPECIAL_MULT_BADGE_OFFSET;
            Vector2 badgeSize = new(UIStyles.SPECIAL_MULT_BADGE_H, UIStyles.SPECIAL_MULT_BADGE_H);
            Image badge = UIFactory.CreateImage(_card, "MultBadge", UiArt.Load("ui_consumable_num"),
                new Vector2(0.5f, 0.5f), badgePos, badgeSize);
            _multText = UIFactory.CreateText(badge.transform, "x1", Vector2.zero, badgeSize,
                UIStyles.SPECIAL_MULT_TEXT_SIZE, FontStyles.Bold, alignment: TextAlignmentOptions.Capline);
            UIFactory.StyleHudText(_multText);
            UIFactory.AutoFit(_multText, UIStyles.SPECIAL_MULT_TEXT_SIZE_MIN, UIStyles.SPECIAL_MULT_TEXT_SIZE);
            float badgePad = badgeSize.x * UIStyles.HUD_RED_LABEL_SIDE_PAD_FRAC;
            _multText.margin = new Vector4(badgePad, 0f, badgePad, 0f); // the box is round: keep "x1.25" off its curve
            // (The red CLASSIC/RELAX mode tab that straddled the card's bottom edge went with
            // the mode toggle on 2026-09-07 — one ruleset, nothing to label.)
        }

        // The mult meter, to the artist's reference: the tube — a vertical capsule from three
        // stacked layers at one rect: brown well (back) → green fill (middle, an Image.Filled
        // driven bottom-up) → frame (front) — running from under the MULT box down past the card's
        // bottom corner, and the red MULT box (the kit's blank Mult_Box) drawn LAST so it caps the
        // tube's top. The box's top edge is the layout's reference line: it sits on the SPECIAL
        // ORDER banner's top edge. The tube's height is DERIVED (box → card bottom) so it always
        // spans the card; width follows the art's aspect. Parented to the card; returns the tube
        // rect (card-local px).
        private Rect BuildMultMeter()
        {
            Vector2 anchor = new(0.5f, 0.5f); // centred in the card, like the mult badge
            float x = UIStyles.MULT_METER_X;

            Sprite boxArt = UiArt.Load("ui_mult_box");
            Vector2 boxSize = UIFactory.SizeByHeight(boxArt, UIStyles.MULT_TAB_H);
            float bannerTop = UIStyles.SPECIAL_BANNER_OFFSET.y + UIStyles.SPECIAL_BANNER_H * 0.5f;
            Vector2 boxPos = new(x, bannerTop + UIStyles.MULT_TAB_Y_NUDGE - boxSize.y * 0.5f);

            Sprite back = UiArt.Load("ui_mult_meter_back");
            Sprite fill = UiArt.Load("ui_mult_meter_fill");
            Sprite frame = UiArt.Load("ui_mult_meter_front");

            float top = boxPos.y - boxSize.y * 0.5f + UIStyles.MULT_METER_TAB_OVERLAP;
            float bottom = -UIStyles.SPECIAL_CARD_SIZE.y * 0.5f + UIStyles.MULT_METER_BOTTOM_INSET;
            float aspect = back != null ? back.rect.width / back.rect.height : 0.3f;
            Vector2 size = new((top - bottom) * aspect, top - bottom);
            Vector2 pos = new(x, (top + bottom) * 0.5f);

            UIFactory.CreateImage(_card, "MultMeterBack", back, anchor, pos, size);

            // The green capsule is inset above the well bottom; extend its rect downward (top fixed) so the
            // fill seats on the well bottom and there's no gap below it.
            float extend = UIStyles.MULT_METER_FILL_BOTTOM_EXTEND;
            Vector2 fillSize = new(size.x, size.y + extend);
            Vector2 fillPos = pos + new Vector2(0f, -extend * 0.5f);
            _meterFill = UIFactory.CreateImage(_card, "MultMeterFill", fill, anchor, fillPos, fillSize);
            _meterFill.type = Image.Type.Filled;
            _meterFill.fillMethod = Image.FillMethod.Vertical;
            _meterFill.fillOrigin = (int)Image.OriginVertical.Bottom;
            _meterFill.fillAmount = 0f;

            UIFactory.CreateImage(_card, "MultMeterFront", frame, anchor, pos, size);

            // The MULT box over the tube's top cap.
            Image box = UIFactory.CreateImage(_card, "MultBox", boxArt, anchor, boxPos, boxSize);
            TextMeshProUGUI boxLabel = UIFactory.CreateText(box.transform, Loc.Get(LocKey.MultTab), Vector2.zero,
                boxSize, UIStyles.MULT_TAB_LABEL_SIZE, FontStyles.Bold, alignment: TextAlignmentOptions.Capline);
            UIFactory.StyleHudText(boxLabel);
            UIFactory.AutoFit(boxLabel, UIStyles.MULT_TAB_LABEL_SIZE_MIN, UIStyles.MULT_TAB_LABEL_SIZE);
            float boxPad = boxSize.x * UIStyles.MULT_TAB_LABEL_SIDE_PAD_FRAC;
            boxLabel.margin = new Vector4(boxPad, 0f, boxPad, 0f);

            return new Rect(pos - size * 0.5f, size);
        }

        // Drives the green fill to the current progress-to-next-level (0..1). Animated on a match /
        // order roll; the leveling match fills to full, then the post-level new order drains it to empty.
        private void UpdateMeter(bool animate)
        {
            if (_meterFill == null) return;
            float target = _model.ChallengeFill;
            _meterFill.DOKill();
            if (animate)
                _meterFill.DOFillAmount(target, AnimConfig.MULT_METER_FILL_DURATION);
            else
                _meterFill.fillAmount = target;
        }

        private void HandleChallengeChanged()
        {
            ClearStack();

            // bun bottom → each required ingredient → one "?" mystery slot PER free ingredient → bun top.
            // The order's total is exact, so the card shows the whole recipe: named art +
            // anything-goes slots (one per instance since 2026-09-06 — the old size-only orders
            // and their single "N" placeholder are gone).
            List<IngredientType?> rows = new List<IngredientType?> { IngredientType.BunBottom };
            foreach (IngredientType t in _model.TargetIngredients)
                rows.Add(t);
            for (int i = _model.TargetIngredients.Count; i < _model.RequiredSize; i++)
                rows.Add(null);
            rows.Add(IngredientType.BunTop);
            const string placeholder = "?";

            // Fit-scale (2026-09-17): the stack draws at SPECIAL_STACK_PX_PER_UNIT and only shrinks —
            // uniformly, spacing included — when a tall order (bun + 3 + bun + plate) would not fit
            // the free area under the banner. Small orders, most of a run, fill the card like the
            // artist's reference; the max order always fits. Extents: top-bun top edge → plate bottom.
            Sprite plate = Theme.Plate;
            Sprite topBun = _model.GetIngredientSprite(IngredientType.BunTop);
            Sprite pin = UiArt.Load("ui_burger_pin");
            float spacing = UIStyles.SPECIAL_INGREDIENT_SPACING;
            // The bone pin stands on the top bun with its stick sunk in by SPECIAL_PIN_EMBED, so
            // the block's top edge is the pin's tip, not the bun's.
            float bunHalf = topBun != null ? WorldScaled(topBun).y * 0.5f : 0f;
            Vector2 pinSize = pin != null ? UIFactory.SizeByHeight(pin, UIStyles.SPECIAL_PIN_H) : Vector2.zero;
            float aboveTop = pin != null ? bunHalf - UIStyles.SPECIAL_PIN_EMBED + pinSize.y : bunHalf;
            float belowBottom = plate != null ? UIStyles.SPECIAL_PLATE_Y_OFFSET + PlateSize(plate).y * 0.5f : 0f;
            float visual = (rows.Count - 1) * spacing + aboveTop + belowBottom;
            float k = Mathf.Min(1f, _stackArea.height / visual);
            _stackScale = k;
            spacing *= k; aboveTop *= k; belowBottom *= k;
            float startY = -(rows.Count - 1) * spacing * 0.5f;

            // Centre the VISUAL block (not the row span) in the free area: the plate hangs below
            // the bottom bun, so the root sits above the area's centre by half that asymmetry.
            float extentCenter = (aboveTop - belowBottom) * 0.5f;
            _stackRoot.anchoredPosition = _stackArea.center - new Vector2(0f, extentCenter) + UIStyles.SPECIAL_STACK_NUDGE;

            // Plate under the bottom bun (added first → renders behind the stack).
            if (plate != null)
            {
                Image plateImg = UIFactory.CreateImage(_stackRoot, "Plate", plate, new Vector2(0.5f, 0.5f),
                    new Vector2(0f, startY - UIStyles.SPECIAL_PLATE_Y_OFFSET * k), PlateSize(plate) * k);
                _stackImages.Add(plateImg);
                _stackBaseColors.Add(Color.white);
            }

            for (int i = 0; i < rows.Count; i++)
            {
                float y = startY + i * spacing;
                if (rows[i].HasValue)
                    AddSprite(_model.GetIngredientSprite(rows[i].Value), $"Ing_{rows[i].Value}", y, null);
                else
                    AddSprite(UiArt.Load("ui_mystery"), "Placeholder", y, placeholder);
            }

            // The bone pin, last so it draws over the top bun (its stick reads as stuck in).
            if (pin != null)
            {
                float bunTopY = startY + (rows.Count - 1) * spacing + bunHalf * k;
                Vector2 pinPos = new(UIStyles.SPECIAL_PIN_X * k, bunTopY - UIStyles.SPECIAL_PIN_EMBED * k + pinSize.y * k * 0.5f);
                Image pinImg = UIFactory.CreateImage(_stackRoot, "Pin", pin, new Vector2(0.5f, 0.5f), pinPos, pinSize * k);
                _stackImages.Add(pinImg);
                _stackBaseColors.Add(Color.white);
            }

            _multText.text = $"x{_model.Multiplier:0.##}"; // 1, 1.25, 1.5 … (the gauge badge shows the LIVE value)
            UpdateMeter(animate: true);
        }

        // A matching burger landed: flash the order and climb the meter toward the next level.
        private void HandleMatched()
        {
            FlashOrder();
            UpdateMeter(animate: true);
        }

        // Adds one stacked image (ingredient or placeholder) with an optional centred label (the
        // "?" on the mystery silhouette, which is also ghosted by SPECIAL_GHOST_ALPHA). Gameplay
        // sprites are sized from their world dimensions (see WorldScaled); the mystery placeholder
        // is UI art with no tuned PPU, sized by height.
        private void AddSprite(Sprite sprite, string name, float y, string label)
        {
            if (sprite == null) return;
            bool isMystery = !string.IsNullOrEmpty(label);
            bool ghosted = isMystery;
            Vector2 size = (isMystery
                ? new Vector2(UIStyles.SPECIAL_MYSTERY_H * sprite.rect.width / sprite.rect.height, UIStyles.SPECIAL_MYSTERY_H)
                : WorldScaled(sprite)) * _stackScale;
            Image img = UIFactory.CreateImage(_stackRoot, name, sprite, new Vector2(0.5f, 0.5f),
                new Vector2(0f, y), size);
            Color baseColor = ghosted ? new Color(1f, 1f, 1f, UIStyles.SPECIAL_GHOST_ALPHA) : Color.white;
            img.color = baseColor;
            _stackImages.Add(img);
            _stackBaseColors.Add(baseColor);

            if (!string.IsNullOrEmpty(label))
            {
                Color labelColor = UIStyles.TEXT_UI;
                if (ghosted) labelColor.a = UIStyles.SPECIAL_GHOST_ALPHA;
                TextMeshProUGUI t = UIFactory.CreateText(img.transform, label, Vector2.zero,
                    img.rectTransform.sizeDelta, UIStyles.SPECIAL_PLACEHOLDER_LABEL_SIZE, FontStyles.Bold, labelColor);
                t.textWrappingMode = TextWrappingModes.NoWrap;
            }
        }

        // Screen size of a gameplay sprite from its world dimensions (pixel rect / PPU) — the same
        // per-file normalization the playfield uses, so the stack's proportions match the game.
        private static Vector2 WorldScaled(Sprite sprite) =>
            new Vector2(sprite.rect.width, sprite.rect.height) / sprite.pixelsPerUnit * UIStyles.SPECIAL_STACK_PX_PER_UNIT;

        // The plate is drawn larger than its playfield proportion — the reference shows a wide
        // dish under the burger, not the under-column saucer.
        private static Vector2 PlateSize(Sprite plate) => WorldScaled(plate) * UIStyles.SPECIAL_PLATE_SCALE;

        private void ClearStack()
        {
            foreach (Image img in _stackImages)
            {
                if (img == null) continue;
                img.transform.DOKill();
                Destroy(img.gameObject);
            }
            _stackImages.Clear();
            _stackBaseColors.Clear();
        }

        private void FlashOrder()
        {
            for (int i = 0; i < _stackImages.Count; i++)
            {
                Image img = _stackImages[i];
                if (img == null) continue;
                img.DOKill();
                img.color = UIStyles.GOLD;
                // Restore each image's REST color — the ghost mystery layer isn't opaque white.
                img.DOColor(_stackBaseColors[i], AnimConfig.LEVELUP_COLOR_RESTORE_DURATION);
            }
        }

        private void HandleLevelUp() => StartCoroutine(LevelUpEffect());

        private IEnumerator LevelUpEffect()
        {
            FlashOrder();
            _card.DOPunchScale(Vector3.one * AnimConfig.LEVELUP_PUNCH_SCALE, AnimConfig.LEVELUP_PUNCH_DURATION, 6);
            yield return new WaitForSeconds(AnimConfig.LEVELUP_HOLD);
            _model.GenerateNewChallenge();
            // The badge/meter normally refresh via the new order's OnChallengeChanged — but the
            // tutorial suppresses the roll, which left the badge stuck at x1 after the showcase
            // level-up (2026-09-07). Refresh them explicitly; harmless when the roll happened.
            _multText.text = $"x{_model.Multiplier:0.##}";
            UpdateMeter(animate: true);
        }
    }
}
