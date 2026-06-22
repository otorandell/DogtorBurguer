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

        public void Initialize(BurgerChallenge model)
        {
            _model = model;
            BuildPanel();

            _model.OnChallengeChanged += HandleChallengeChanged;
            _model.OnMatched += FlashOrder;
            _model.OnLevelUp += HandleLevelUp;
        }

        private void OnDestroy()
        {
            if (_model == null) return;
            _model.OnChallengeChanged -= HandleChallengeChanged;
            _model.OnMatched -= FlashOrder;
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
                FontStyles.Bold, UIStyles.HUD_TITLE_LABEL_COLOR);
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

            // Multiplier badge (bottom-right) — reuses the red num box sprite.
            Image badge = UIFactory.CreateImage(_card, "MultBadge", UiArt.Load("ui_consumable_num"),
                new Vector2(0.5f, 0.5f), UIStyles.SPECIAL_MULT_BADGE_OFFSET,
                new Vector2(UIStyles.SPECIAL_MULT_BADGE_H, UIStyles.SPECIAL_MULT_BADGE_H));
            _multText = UIFactory.CreateText(badge.transform, "x1", Vector2.zero,
                new Vector2(UIStyles.SPECIAL_MULT_BADGE_H, UIStyles.SPECIAL_MULT_BADGE_H),
                UIStyles.SPECIAL_MULT_TEXT_SIZE, FontStyles.Bold, UIStyles.CONSUMABLE_COUNT_COLOR);
            _multText.textWrappingMode = TextWrappingModes.NoWrap;
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
