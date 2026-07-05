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
        private TextMeshProUGUI _multText;
        private Image _meterFill;

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
            _canvas = UIFactory.CreateCanvas(transform, "ChallengeCanvas", Constants.SORT_CHALLENGE_BASE);

            Image card = UIFactory.CreateImage(_canvas.transform, "SpecialCard", UiArt.Load("ui_special_card"),
                new Vector2(1f, 1f), UIStyles.SPECIAL_CARD_POS, UIStyles.SPECIAL_CARD_SIZE);
            _card = card.rectTransform;

            // SPECIAL ORDER banner (blank art), sized by height (aspect), overhanging the card's
            // top-left, with the word as TMP — like the Level/Score tabs.
            Sprite banner = UiArt.Load("ui_special_title");
            float bannerAspect = banner != null ? banner.rect.width / banner.rect.height : 1f;
            Vector2 bannerSize = new(UIStyles.SPECIAL_BANNER_H * bannerAspect, UIStyles.SPECIAL_BANNER_H);
            Image bannerImg = UIFactory.CreateImage(_card, "Banner", banner, new Vector2(0.5f, 0.5f),
                UIStyles.SPECIAL_BANNER_OFFSET, bannerSize);

            TextMeshProUGUI bannerLabel = UIFactory.CreateText(bannerImg.transform, "SPECIAL ORDER",
                UIStyles.SPECIAL_BANNER_LABEL_OFFSET, bannerSize, UIStyles.SPECIAL_BANNER_LABEL_SIZE,
                FontStyles.Bold);
            UIFactory.StyleHudText(bannerLabel);
            bannerLabel.textWrappingMode = TextWrappingModes.NoWrap;
            bannerLabel.enableAutoSizing = true;
            bannerLabel.fontSizeMin = UIStyles.SPECIAL_BANNER_LABEL_SIZE_MIN;
            bannerLabel.fontSizeMax = UIStyles.SPECIAL_BANNER_LABEL_SIZE;

            // Burger stack container (centred a touch below the card middle).
            GameObject stackObj = new GameObject("Stack");
            stackObj.transform.SetParent(_card, false);
            _stackRoot = stackObj.AddComponent<RectTransform>();
            _stackRoot.anchorMin = _stackRoot.anchorMax = new Vector2(0.5f, 0.5f);
            _stackRoot.anchoredPosition = new Vector2(0f, UIStyles.SPECIAL_STACK_Y);
            _stackRoot.sizeDelta = Vector2.zero;

            // Mult meter (built before the badge so the badge renders on top of it).
            BuildMultMeter();

            // Multiplier badge (bottom-right) — reuses the red num box sprite.
            Image badge = UIFactory.CreateImage(_card, "MultBadge", UiArt.Load("ui_consumable_num"),
                new Vector2(0.5f, 0.5f), UIStyles.SPECIAL_MULT_BADGE_OFFSET,
                new Vector2(UIStyles.SPECIAL_MULT_BADGE_H, UIStyles.SPECIAL_MULT_BADGE_H));
            _multText = UIFactory.CreateText(badge.transform, "x1", Vector2.zero,
                new Vector2(UIStyles.SPECIAL_MULT_BADGE_H, UIStyles.SPECIAL_MULT_BADGE_H),
                UIStyles.SPECIAL_MULT_TEXT_SIZE, FontStyles.Bold);
            UIFactory.StyleHudText(_multText);
            _multText.textWrappingMode = TextWrappingModes.NoWrap;
        }

        // The mult meter: a vertical capsule from three stacked layers at one rect — brown well (back) →
        // green fill (middle, an Image.Filled driven bottom-up) → frame (front). Parented to the card and
        // built before the badge, so the multiplier badge renders on top of it (sharing its x).
        private void BuildMultMeter()
        {
            Sprite back = UiArt.Load("ui_mult_meter_back");
            Sprite fill = UiArt.Load("ui_mult_meter_fill");
            Sprite frame = UiArt.Load("ui_mult_meter_front");

            float aspect = back != null ? back.rect.width / back.rect.height : 0.3f;
            Vector2 size = new(UIStyles.MULT_METER_H * aspect, UIStyles.MULT_METER_H);
            Vector2 anchor = new(0.5f, 0.5f); // centred in the card, like the mult badge
            Vector2 pos = UIStyles.MULT_METER_OFFSET;

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

            // bun bottom → (size: a "+N" mystery placeholder | contains: each required ingredient) → bun top
            List<IngredientType?> rows = new List<IngredientType?> { IngredientType.BunBottom };
            string placeholder = null;
            if (_model.CurrentOrderType == OrderType.Size)
            {
                rows.Add(null); // mystery placeholder
                placeholder = $"+{_model.RequiredSize}";
            }
            else
            {
                foreach (IngredientType t in _model.TargetIngredients)
                    rows.Add(t);
            }
            rows.Add(IngredientType.BunTop);

            float spacing = UIStyles.SPECIAL_INGREDIENT_SPACING;
            float startY = -(rows.Count - 1) * spacing * 0.5f;

            // Plate under the bottom bun (added first → renders behind the stack).
            Sprite plate = Theme.Plate;
            if (plate != null)
            {
                float plateAspect = plate.rect.width / plate.rect.height;
                Image plateImg = UIFactory.CreateImage(_stackRoot, "Plate", plate, new Vector2(0.5f, 0.5f),
                    new Vector2(0f, startY - UIStyles.SPECIAL_PLATE_Y_OFFSET),
                    new Vector2(UIStyles.SPECIAL_PLATE_H * plateAspect, UIStyles.SPECIAL_PLATE_H));
                _stackImages.Add(plateImg);
            }

            for (int i = 0; i < rows.Count; i++)
            {
                float y = startY + i * spacing;
                if (rows[i].HasValue)
                    AddSprite(_model.GetIngredientSprite(rows[i].Value), $"Ing_{rows[i].Value}", y, null);
                else
                    AddSprite(UiArt.Load("ui_mystery"), "Placeholder", y, placeholder);
            }

            _multText.text = $"x{_model.GetGlobalMultiplier()}";
            UpdateMeter(animate: true);
        }

        // A matching burger landed: flash the order and climb the meter toward the next level.
        private void HandleMatched()
        {
            FlashOrder();
            UpdateMeter(animate: true);
        }

        // Adds one stacked image (ingredient or placeholder), sized by height preserving aspect, with
        // an optional centred label (the "+N" on the mystery silhouette).
        private void AddSprite(Sprite sprite, string name, float y, string label)
        {
            if (sprite == null) return;
            float aspect = sprite.rect.width / sprite.rect.height;
            Image img = UIFactory.CreateImage(_stackRoot, name, sprite, new Vector2(0.5f, 0.5f),
                new Vector2(0f, y), new Vector2(UIStyles.SPECIAL_INGREDIENT_H * aspect, UIStyles.SPECIAL_INGREDIENT_H));
            _stackImages.Add(img);

            if (!string.IsNullOrEmpty(label))
            {
                TextMeshProUGUI t = UIFactory.CreateText(img.transform, label, Vector2.zero,
                    img.rectTransform.sizeDelta, UIStyles.SPECIAL_PLACEHOLDER_LABEL_SIZE, FontStyles.Bold, UIStyles.TEXT_UI);
                t.textWrappingMode = TextWrappingModes.NoWrap;
            }
        }

        private void ClearStack()
        {
            foreach (Image img in _stackImages)
            {
                if (img == null) continue;
                img.transform.DOKill();
                Destroy(img.gameObject);
            }
            _stackImages.Clear();
        }

        private void FlashOrder()
        {
            foreach (Image img in _stackImages)
            {
                if (img == null) continue;
                img.DOKill();
                img.color = UIStyles.GOLD;
                img.DOColor(Color.white, AnimConfig.LEVELUP_COLOR_RESTORE_DURATION);
            }
        }

        private void HandleLevelUp() => StartCoroutine(LevelUpEffect());

        private IEnumerator LevelUpEffect()
        {
            FlashOrder();
            _card.DOPunchScale(Vector3.one * AnimConfig.LEVELUP_PUNCH_SCALE, AnimConfig.LEVELUP_PUNCH_DURATION, 6);
            yield return new WaitForSeconds(AnimConfig.LEVELUP_HOLD);
            _model.GenerateNewChallenge();
        }
    }
}
