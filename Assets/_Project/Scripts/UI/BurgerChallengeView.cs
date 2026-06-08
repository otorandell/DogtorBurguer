using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

namespace DogtorBurguer
{
    /// <summary>
    /// The view half of the Special Orders challenge (F-56). Builds and animates all of
    /// the on-screen elements; owns no challenge logic. Created at runtime by
    /// BurgerChallenge (the model) and driven entirely by its events + read-only state.
    /// </summary>
    public class BurgerChallengeView : MonoBehaviour
    {
        private BurgerChallenge _model;

        private readonly List<GameObject> _burgerVisuals = new List<GameObject>();
        private GameObject _displayRoot;
        private TextMeshPro _titleText;
        private TextMeshPro _nameText;
        private TextMeshPro _levelText;
        private SpriteRenderer _meterBg;
        private SpriteRenderer _meterFill;

        // Meter anchor (local to the display root)
        private const float MeterX = 0.9f;
        private const float MeterY = -0.2f;

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
            int order = _model.SortingOrder;

            _displayRoot = new GameObject("ChallengeDisplay");
            _displayRoot.transform.SetParent(transform, false);
            _displayRoot.transform.position = new Vector3(_model.PanelCenter.x, _model.PanelCenter.y, 0f);

            // Title ("Special Order!")
            GameObject titleObj = new GameObject("ChallengeTitle");
            titleObj.transform.SetParent(_displayRoot.transform, false);
            titleObj.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            _titleText = WorldTextFactory.Create(titleObj, "Special Order!",
                UIStyles.WORLD_CHALLENGE_NAME_SIZE, UIStyles.GOLD, order + 1,
                new Vector2(2.2f, 0.5f), FontStyles.Bold, UIStyles.OUTLINE_WIDTH_UI);

            // Requirement description (below title)
            GameObject nameObj = new GameObject("ChallengeName");
            nameObj.transform.SetParent(_displayRoot.transform, false);
            nameObj.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            _nameText = WorldTextFactory.Create(nameObj, string.Empty,
                UIStyles.WORLD_CHALLENGE_NAME_SIZE * 0.8f, UIStyles.TEXT_UI, order + 1,
                new Vector2(2.2f, 0.5f), FontStyles.Normal, UIStyles.OUTLINE_WIDTH_UI);

            // Meter background
            GameObject meterBgObj = new GameObject("MeterBg");
            meterBgObj.transform.SetParent(_displayRoot.transform, false);
            meterBgObj.transform.localPosition = new Vector3(MeterX, MeterY, 0f);
            _meterBg = meterBgObj.AddComponent<SpriteRenderer>();
            _meterBg.sprite = SpriteFactory.White();
            _meterBg.color = _model.MeterBgColor;
            _meterBg.sortingOrder = order;
            meterBgObj.transform.localScale = new Vector3(_model.MeterWidth, _model.MeterHeight, 1f);

            // Meter fill
            GameObject meterFillObj = new GameObject("MeterFill");
            meterFillObj.transform.SetParent(_displayRoot.transform, false);
            meterFillObj.transform.localPosition = new Vector3(MeterX, MeterY - _model.MeterHeight * 0.5f, 0f);
            _meterFill = meterFillObj.AddComponent<SpriteRenderer>();
            _meterFill.sprite = SpriteFactory.White();
            _meterFill.color = _model.MeterFillColor;
            _meterFill.sortingOrder = order + 1;
            meterFillObj.transform.localScale = new Vector3(_model.MeterWidth, 0f, 1f);

            // Level text
            GameObject levelObj = new GameObject("ChallengeLevel");
            levelObj.transform.SetParent(_displayRoot.transform, false);
            levelObj.transform.localPosition = new Vector3(MeterX, MeterY - _model.MeterHeight * 0.5f - 0.2f, 0f);
            _levelText = WorldTextFactory.Create(levelObj, string.Empty,
                UIStyles.WORLD_CHALLENGE_LEVEL_SIZE, UIStyles.TEXT_UI, order + 1,
                new Vector2(1f, 0.4f), FontStyles.Normal, UIStyles.OUTLINE_WIDTH_UI);
        }

        private void HandleChallengeChanged()
        {
            ClearBurgerVisuals();

            if (_model.CurrentOrderType == OrderType.Size)
                CreateSizeVisual();
            else
                CreateContainsVisual();

            _nameText.text = _model.ChallengeName;
            UpdateMeter();
            UpdateLevelText();
        }

        private void HandleLevelUp()
        {
            StartCoroutine(LevelUpEffect());
        }

        private void CreateSizeVisual()
        {
            float startY = -(2 * _model.IngredientSpacing) * 0.5f;

            CreateIngredientVisual(IngredientType.BunBottom, startY, 0);
            CreatePlaceholderVisual($"+{_model.RequiredSize}", startY + _model.IngredientSpacing, 1);
            CreateIngredientVisual(IngredientType.BunTop, startY + 2 * _model.IngredientSpacing, 2);
        }

        private void CreateContainsVisual()
        {
            IReadOnlyList<IngredientType> targets = _model.TargetIngredients;
            int totalSlots = targets.Count + 2; // two "?" placeholders
            float startY = -(totalSlots + 1) * _model.IngredientSpacing * 0.5f;
            int order = 0;

            CreateIngredientVisual(IngredientType.BunBottom, startY, order++);

            CreatePlaceholderVisual("?", startY + order * _model.IngredientSpacing, order);
            order++;

            for (int i = 0; i < targets.Count; i++)
            {
                CreateIngredientVisual(targets[i], startY + order * _model.IngredientSpacing, order);
                order++;
            }

            CreatePlaceholderVisual("?", startY + order * _model.IngredientSpacing, order);
            order++;

            CreateIngredientVisual(IngredientType.BunTop, startY + order * _model.IngredientSpacing, order);
        }

        private void CreatePlaceholderVisual(string label, float localY, int orderIndex)
        {
            Sprite placeholder = _model.PlaceholderSprite;
            if (placeholder == null) return;

            GameObject obj = new GameObject($"Placeholder_{label}");
            obj.transform.SetParent(_displayRoot.transform, false);
            obj.transform.localPosition = new Vector3(0f, localY, 0f);
            obj.transform.localScale = Vector3.one * _model.IngredientScale;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = placeholder;
            sr.sortingOrder = _model.SortingOrder + 2 + orderIndex;
            _burgerVisuals.Add(obj);

            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(obj.transform, false);
            textObj.transform.localPosition = Vector3.zero;
            WorldTextFactory.Create(textObj, label,
                UIStyles.WORLD_CHALLENGE_NAME_SIZE, UIStyles.TEXT_UI, sr.sortingOrder + 1,
                new Vector2(1f, 0.5f), FontStyles.Bold, UIStyles.OUTLINE_WIDTH_WORLD);
            _burgerVisuals.Add(textObj);
        }

        private void CreateIngredientVisual(IngredientType type, float localY, int orderIndex)
        {
            Sprite sprite = _model.GetIngredientSprite(type);
            if (sprite == null) return;

            GameObject obj = new GameObject($"Challenge_{type}");
            obj.transform.SetParent(_displayRoot.transform, false);
            obj.transform.localPosition = new Vector3(0f, localY, 0f);
            obj.transform.localScale = Vector3.one * _model.IngredientScale;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = _model.SortingOrder + 2 + orderIndex;
            _burgerVisuals.Add(obj);
        }

        private void ClearBurgerVisuals()
        {
            foreach (var obj in _burgerVisuals)
            {
                if (obj == null) continue;
                obj.transform.DOKill();
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null) sr.DOKill();
                Destroy(obj);
            }
            _burgerVisuals.Clear();
        }

        private void UpdateMeter()
        {
            if (_meterFill == null) return;

            float fill = (float)_model.Progress / _model.ProgressTarget;
            float fillHeight = _model.MeterHeight * fill;
            float bottomY = MeterY - _model.MeterHeight * 0.5f + fillHeight * 0.5f;
            _meterFill.transform.localPosition = new Vector3(MeterX, bottomY, 0f);
            _meterFill.transform.localScale = new Vector3(_model.MeterWidth, fillHeight, 1f);
        }

        private void UpdateLevelText()
        {
            if (_levelText != null)
                _levelText.text = $"★ {_model.Level}";
        }

        private void FlashOrder()
        {
            foreach (var obj in _burgerVisuals)
            {
                if (obj == null) continue;
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr == null) continue;

                Color original = sr.color;
                sr.color = UIStyles.GOLD;
                sr.DOColor(original, AnimConfig.LEVELUP_COLOR_RESTORE_DURATION);
            }
        }

        private IEnumerator LevelUpEffect()
        {
            if (_meterFill == null) yield break;

            float width = _model.MeterWidth;
            float height = _model.MeterHeight;

            // Fill the meter to 100%
            float fullCenter = MeterY;
            _meterFill.transform.DOLocalMove(new Vector3(MeterX, fullCenter, 0f), AnimConfig.LEVELUP_FILL_DURATION).SetEase(Ease.OutQuad);
            _meterFill.transform.DOScale(new Vector3(width, height, 1f), AnimConfig.LEVELUP_FILL_DURATION).SetEase(Ease.OutQuad);
            yield return new WaitForSeconds(AnimConfig.LEVELUP_FILL_DURATION);

            // Flash gold and punch
            Color originalColor = _model.MeterFillColor;
            _meterFill.color = UIStyles.GOLD;
            _meterFill.transform.DOPunchScale(Vector3.one * AnimConfig.LEVELUP_PUNCH_SCALE, AnimConfig.LEVELUP_PUNCH_DURATION, 6);
            _meterBg.transform.DOPunchScale(Vector3.one * AnimConfig.LEVELUP_BG_PUNCH_SCALE, AnimConfig.LEVELUP_PUNCH_DURATION, 6);
            _levelText.transform.DOPunchScale(Vector3.one * AnimConfig.LEVELUP_TEXT_PUNCH_SCALE, AnimConfig.LEVELUP_TEXT_PUNCH_DURATION);
            yield return new WaitForSeconds(AnimConfig.LEVELUP_HOLD);

            // Fade back and shrink to empty
            _meterFill.DOColor(originalColor, AnimConfig.LEVELUP_FADE_COLOR_DURATION);
            float emptyBottom = MeterY - height * 0.5f;
            _meterFill.transform.DOLocalMove(new Vector3(MeterX, emptyBottom, 0f), AnimConfig.LEVELUP_SHRINK_DURATION).SetEase(Ease.InQuad);
            _meterFill.transform.DOScale(new Vector3(width, 0f, 1f), AnimConfig.LEVELUP_SHRINK_DURATION).SetEase(Ease.InQuad);
            yield return new WaitForSeconds(AnimConfig.LEVELUP_WAIT);

            _model.GenerateNewChallenge();
        }
    }
}
