using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace DogtorBurguer
{
    public class BurgerChallenge : MonoBehaviour
    {
        public static BurgerChallenge Instance { get; private set; }

        [Header("Panel Position")]
        [SerializeField] private Vector2 _panelCenter = new Vector2(1.35f, 2.4f);

        [Header("Burger Display")]
        [SerializeField] private float _ingredientSpacing = 0.18f;
        [SerializeField] private float _ingredientScale = 1.0f;
        [SerializeField] private int _sortingOrder = Constants.SORT_CHALLENGE_BASE;

        [Header("Placeholder")]
        [SerializeField] private Sprite _spritePlaceholder;

        [Header("Meter")]
        [SerializeField] private float _meterWidth = 0.2f;
        [SerializeField] private float _meterHeight = 1.6f;
        [SerializeField] private Color _meterBgColor = UIStyles.CHALLENGE_METER_BG;
        [SerializeField] private Color _meterFillColor = UIStyles.CHALLENGE_METER_FILL;

        private OrderType _orderType;
        private int _requiredSize;
        private List<IngredientType> _targetIngredients = new List<IngredientType>();
        private int _challengeLevel = 1;
        private int _challengeProgress;
        private string _challengeName;

        // Visual elements
        private List<GameObject> _burgerVisuals = new List<GameObject>();
        private TMPro.TextMeshPro _titleText;
        private TMPro.TextMeshPro _nameText;
        private TMPro.TextMeshPro _levelText;
        private SpriteRenderer _meterBg;
        private SpriteRenderer _meterFill;
        private GameObject _displayRoot;

        // Cached meter positions
        private float _meterX = 0.9f;
        private float _meterY = -0.2f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            CreateUI();
            GenerateNewChallenge();
            SubscribeEvents();
        }

        private void OnDestroy()
        {
            if (GridManager.Instance != null)
                GridManager.Instance.OnBurgerWithIngredients -= HandleBurgerCompleted;
        }

        private void SubscribeEvents()
        {
            if (GridManager.Instance != null)
                GridManager.Instance.OnBurgerWithIngredients += HandleBurgerCompleted;
        }

        private void CreateUI()
        {
            _displayRoot = new GameObject("ChallengeDisplay");
            _displayRoot.transform.SetParent(transform, false);
            _displayRoot.transform.position = new Vector3(_panelCenter.x, _panelCenter.y, 0f);

            // Title text ("Special Order!")
            GameObject titleObj = new GameObject("ChallengeTitle");
            titleObj.transform.SetParent(_displayRoot.transform, false);
            titleObj.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            _titleText = titleObj.AddComponent<TMPro.TextMeshPro>();
            _titleText.text = "Special Order!";
            _titleText.fontSize = UIStyles.WORLD_CHALLENGE_NAME_SIZE;
            _titleText.color = UIStyles.GOLD;
            _titleText.alignment = TMPro.TextAlignmentOptions.Center;
            _titleText.fontStyle = TMPro.FontStyles.Bold;
            _titleText.sortingOrder = _sortingOrder + 1;
            _titleText.outlineWidth = UIStyles.OUTLINE_WIDTH_UI;
            _titleText.outlineColor = UIStyles.OUTLINE_COLOR;
            RectTransform titleRect = _titleText.GetComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(2.2f, 0.5f);

            // Name text (requirement description, below title)
            GameObject nameObj = new GameObject("ChallengeName");
            nameObj.transform.SetParent(_displayRoot.transform, false);
            nameObj.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            _nameText = nameObj.AddComponent<TMPro.TextMeshPro>();
            _nameText.fontSize = UIStyles.WORLD_CHALLENGE_NAME_SIZE * 0.8f;
            _nameText.color = UIStyles.TEXT_UI;
            _nameText.alignment = TMPro.TextAlignmentOptions.Center;
            _nameText.sortingOrder = _sortingOrder + 1;
            _nameText.outlineWidth = UIStyles.OUTLINE_WIDTH_UI;
            _nameText.outlineColor = UIStyles.OUTLINE_COLOR;
            RectTransform nameRect = _nameText.GetComponent<RectTransform>();
            nameRect.sizeDelta = new Vector2(2.2f, 0.5f);

            // Meter background
            GameObject meterBgObj = new GameObject("MeterBg");
            meterBgObj.transform.SetParent(_displayRoot.transform, false);
            meterBgObj.transform.localPosition = new Vector3(_meterX, _meterY, 0f);
            _meterBg = meterBgObj.AddComponent<SpriteRenderer>();
            _meterBg.sprite = GenerateRectSprite();
            _meterBg.color = _meterBgColor;
            _meterBg.sortingOrder = _sortingOrder;
            meterBgObj.transform.localScale = new Vector3(_meterWidth, _meterHeight, 1f);

            // Meter fill
            GameObject meterFillObj = new GameObject("MeterFill");
            meterFillObj.transform.SetParent(_displayRoot.transform, false);
            meterFillObj.transform.localPosition = new Vector3(_meterX, _meterY - _meterHeight * 0.5f, 0f);
            _meterFill = meterFillObj.AddComponent<SpriteRenderer>();
            _meterFill.sprite = GenerateRectSprite();
            _meterFill.color = _meterFillColor;
            _meterFill.sortingOrder = _sortingOrder + 1;
            meterFillObj.transform.localScale = new Vector3(_meterWidth, 0f, 1f);
            _meterFill.transform.localPosition = new Vector3(_meterX, _meterY - _meterHeight * 0.5f, 0f);

            // Level text
            GameObject levelObj = new GameObject("ChallengeLevel");
            levelObj.transform.SetParent(_displayRoot.transform, false);
            levelObj.transform.localPosition = new Vector3(_meterX, _meterY - _meterHeight * 0.5f - 0.2f, 0f);
            _levelText = levelObj.AddComponent<TMPro.TextMeshPro>();
            _levelText.fontSize = UIStyles.WORLD_CHALLENGE_LEVEL_SIZE;
            _levelText.color = UIStyles.TEXT_UI;
            _levelText.alignment = TMPro.TextAlignmentOptions.Center;
            _levelText.sortingOrder = _sortingOrder + 1;
            _levelText.outlineWidth = UIStyles.OUTLINE_WIDTH_UI;
            _levelText.outlineColor = UIStyles.OUTLINE_COLOR;
            RectTransform levelRect = _levelText.GetComponent<RectTransform>();
            levelRect.sizeDelta = new Vector2(1f, 0.4f);
        }

        public void GenerateNewChallenge()
        {
            _targetIngredients.Clear();
            ClearBurgerVisuals();

            _orderType = Rng.Range(0, 2) == 0 ? OrderType.Size : OrderType.Contains;

            if (_orderType == OrderType.Size)
            {
                _requiredSize = Mathf.Clamp(_challengeLevel + 1, GameplayConfig.CHALLENGE_MIN_SIZE, GameplayConfig.CHALLENGE_MAX_SIZE);
                _challengeName = $"{_requiredSize}+ Ingredients";
                CreateSizeVisual();
            }
            else
            {
                int count = Mathf.Clamp(_challengeLevel, 1, GameplayConfig.CHALLENGE_MAX_CONTAINS);
                int activeCount = GetActiveIngredientCount();
                GenerateContainsIngredients(count, activeCount);
                _challengeName = BuildContainsName();
                CreateContainsVisual();
            }

            _nameText.text = _challengeName;
            UpdateMeter();
            UpdateLevelText();
        }

        private void GenerateContainsIngredients(int count, int activeCount)
        {
            // Pick unique random ingredients from the active pool
            List<int> available = new List<int>();
            for (int i = 0; i < activeCount; i++)
                available.Add(i);

            for (int i = 0; i < count && available.Count > 0; i++)
            {
                int idx = Rng.Range(0, available.Count);
                _targetIngredients.Add(GameplayConfig.REGULAR_INGREDIENTS[available[idx]]);
                available.RemoveAt(idx);
            }
        }

        private string BuildContainsName()
        {
            if (_targetIngredients.Count == 1)
                return $"Has: {_targetIngredients[0]}";

            System.Text.StringBuilder sb = new System.Text.StringBuilder("Has: ");
            for (int i = 0; i < _targetIngredients.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(_targetIngredients[i]);
            }
            return sb.ToString();
        }

        private int GetActiveIngredientCount()
        {
            IngredientSpawner spawner = FindAnyObjectByType<IngredientSpawner>();
            if (spawner != null)
                return spawner.ActiveIngredientCount;
            return GameplayConfig.STARTING_INGREDIENT_COUNT;
        }

        private void CreateSizeVisual()
        {
            IngredientSpawner spawner = FindAnyObjectByType<IngredientSpawner>();
            if (spawner == null) return;

            float totalHeight = 2 * _ingredientSpacing;
            float startY = -totalHeight * 0.5f;

            // Bottom bun
            CreateIngredientVisual(IngredientType.BunBottom, startY, spawner, 0);

            // Silhouette placeholder with "+N" label
            CreatePlaceholderVisual($"+{_requiredSize}", startY + _ingredientSpacing, 1);

            // Top bun
            CreateIngredientVisual(IngredientType.BunTop, startY + 2 * _ingredientSpacing, spawner, 2);
        }

        private void CreateContainsVisual()
        {
            IngredientSpawner spawner = FindAnyObjectByType<IngredientSpawner>();
            if (spawner == null) return;

            // Layout: bun + "?" + ingredients... + "?" + bun
            int totalSlots = _targetIngredients.Count + 2; // +2 for the two "?" placeholders
            float startY = -(totalSlots + 1) * _ingredientSpacing * 0.5f;
            int order = 0;

            // Bottom bun
            CreateIngredientVisual(IngredientType.BunBottom, startY, spawner, order++);

            // Opening "?" placeholder
            float y = startY + order * _ingredientSpacing;
            CreatePlaceholderVisual("?", y, order);
            order++;

            // Required ingredients
            for (int i = 0; i < _targetIngredients.Count; i++)
            {
                y = startY + order * _ingredientSpacing;
                CreateIngredientVisual(_targetIngredients[i], y, spawner, order);
                order++;
            }

            // Closing "?" placeholder
            y = startY + order * _ingredientSpacing;
            CreatePlaceholderVisual("?", y, order);
            order++;

            // Top bun
            float topY = startY + order * _ingredientSpacing;
            CreateIngredientVisual(IngredientType.BunTop, topY, spawner, order);
        }

        private void CreatePlaceholderVisual(string label, float localY, int orderIndex)
        {
            if (_spritePlaceholder == null) return;

            // Silhouette sprite
            GameObject obj = new GameObject($"Placeholder_{label}");
            obj.transform.SetParent(_displayRoot.transform, false);
            obj.transform.localPosition = new Vector3(0f, localY, 0f);
            obj.transform.localScale = Vector3.one * _ingredientScale;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _spritePlaceholder;
            sr.sortingOrder = _sortingOrder + 2 + orderIndex;
            _burgerVisuals.Add(obj);

            // Text overlay as child
            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(obj.transform, false);
            textObj.transform.localPosition = Vector3.zero;

            TMPro.TextMeshPro tmp = textObj.AddComponent<TMPro.TextMeshPro>();
            tmp.text = label;
            tmp.fontSize = UIStyles.WORLD_CHALLENGE_NAME_SIZE;
            tmp.color = UIStyles.TEXT_UI;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.fontStyle = TMPro.FontStyles.Bold;
            tmp.sortingOrder = sr.sortingOrder + 1;
            tmp.outlineWidth = UIStyles.OUTLINE_WIDTH_WORLD;
            tmp.outlineColor = UIStyles.OUTLINE_COLOR;
            RectTransform textRect = tmp.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(1f, 0.5f);
            _burgerVisuals.Add(textObj);
        }

        private void CreateIngredientVisual(IngredientType type, float localY, IngredientSpawner spawner, int orderIndex)
        {
            Sprite sprite = spawner.GetSpriteForType(type);
            if (sprite == null) return;

            GameObject obj = new GameObject($"Challenge_{type}");
            obj.transform.SetParent(_displayRoot.transform, false);
            obj.transform.localPosition = new Vector3(0f, localY, 0f);
            obj.transform.localScale = Vector3.one * _ingredientScale;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = _sortingOrder + 2 + orderIndex;

            _burgerVisuals.Add(obj);
        }

        private void ClearBurgerVisuals()
        {
            foreach (var obj in _burgerVisuals)
            {
                if (obj != null)
                {
                    obj.transform.DOKill();
                    SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.DOKill();
                    Destroy(obj);
                }
            }
            _burgerVisuals.Clear();
        }

        /// <summary>
        /// Pure match check with no side effects. Used by GridManager
        /// to override burger display name to "Order Complete!".
        /// </summary>
        public bool IsOrderMatch(List<IngredientType> ingredients, int ingredientCount)
        {
            if (ingredientCount == 0) return false;

            if (_orderType == OrderType.Size)
                return ingredientCount >= _requiredSize;

            // Contains: all required ingredients must be present
            foreach (var required in _targetIngredients)
            {
                if (!ingredients.Contains(required))
                    return false;
            }
            return true;
        }

        private void HandleBurgerCompleted(Vector3 pos, int basePoints, string name, int ingredientCount, List<IngredientType> ingredients)
        {
            if (ingredientCount == 0) return;

            bool isMatch = IsOrderMatch(ingredients, ingredientCount);
            int globalMult = GetGlobalMultiplier();
            int challengeMult = isMatch ? GameplayConfig.CHALLENGE_MATCH_MULTIPLIER : 1;
            int finalPoints = basePoints * globalMult * challengeMult;

            // Award the extra score (beyond base already given)
            int extraPoints = finalPoints - basePoints;
            if (extraPoints > 0)
                GameManager.Instance?.AddExtraScore(extraPoints);

            // Show multiplier text
            if (globalMult > 1 || isMatch)
            {
                string multText = $"x{globalMult * challengeMult}";
                Color textColor = isMatch ? UIStyles.GOLD : UIStyles.TEXT_UI;
                FloatingText.Spawn(pos + Vector3.up * 0.5f, multText, textColor, UIStyles.WORLD_FLOATING_TEXT_SIZE);
            }

            if (isMatch)
            {
                _challengeProgress++;
                AudioManager.Instance?.PlayChallengeMatch();
                FlashPanel();

                if (_challengeProgress >= _challengeLevel + 1)
                {
                    LevelUp();
                }
                else
                {
                    GenerateNewChallenge();
                }
            }
        }

        public int GetGlobalMultiplier()
        {
            return 1 + (_challengeLevel - 1) * GameplayConfig.CHALLENGE_GLOBAL_MULT_PER_LEVEL;
        }

        private void LevelUp()
        {
            _challengeLevel++;
            _challengeProgress = 0;

            StartCoroutine(LevelUpEffect());

            Debug.Log($"[BurgerChallenge] Level up! Now level {_challengeLevel}");
        }

        private System.Collections.IEnumerator LevelUpEffect()
        {
            if (_meterFill == null) yield break;

            // Fill meter to 100%
            float fullBottom = _meterY - _meterHeight * 0.5f + _meterHeight * 0.5f;
            _meterFill.transform.DOLocalMove(new Vector3(_meterX, fullBottom, 0f), AnimConfig.LEVELUP_FILL_DURATION).SetEase(Ease.OutQuad);
            _meterFill.transform.DOScale(new Vector3(_meterWidth, _meterHeight, 1f), AnimConfig.LEVELUP_FILL_DURATION).SetEase(Ease.OutQuad);
            yield return new WaitForSeconds(AnimConfig.LEVELUP_FILL_DURATION);

            // Flash gold and punch scale
            Color originalColor = _meterFillColor;
            _meterFill.color = UIStyles.GOLD;
            _meterFill.transform.DOPunchScale(Vector3.one * AnimConfig.LEVELUP_PUNCH_SCALE, AnimConfig.LEVELUP_PUNCH_DURATION, 6);
            _meterBg.transform.DOPunchScale(Vector3.one * AnimConfig.LEVELUP_BG_PUNCH_SCALE, AnimConfig.LEVELUP_PUNCH_DURATION, 6);
            _levelText.transform.DOPunchScale(Vector3.one * AnimConfig.LEVELUP_TEXT_PUNCH_SCALE, AnimConfig.LEVELUP_TEXT_PUNCH_DURATION);

            yield return new WaitForSeconds(AnimConfig.LEVELUP_HOLD);

            // Fade back to normal color and shrink to 0
            _meterFill.DOColor(originalColor, AnimConfig.LEVELUP_FADE_COLOR_DURATION);
            float emptyBottom = _meterY - _meterHeight * 0.5f;
            _meterFill.transform.DOLocalMove(new Vector3(_meterX, emptyBottom, 0f), AnimConfig.LEVELUP_SHRINK_DURATION).SetEase(Ease.InQuad);
            _meterFill.transform.DOScale(new Vector3(_meterWidth, 0f, 1f), AnimConfig.LEVELUP_SHRINK_DURATION).SetEase(Ease.InQuad);

            yield return new WaitForSeconds(AnimConfig.LEVELUP_WAIT);

            GenerateNewChallenge();
        }

        private void UpdateMeter()
        {
            float fill = (float)_challengeProgress / (_challengeLevel + 1);
            float fillHeight = _meterHeight * fill;

            if (_meterFill != null)
            {
                float bottomY = _meterY - _meterHeight * 0.5f + fillHeight * 0.5f;
                _meterFill.transform.localPosition = new Vector3(_meterX, bottomY, 0f);
                _meterFill.transform.localScale = new Vector3(_meterWidth, fillHeight, 1f);
            }
        }

        private void UpdateLevelText()
        {
            if (_levelText != null)
                _levelText.text = $"\u2605 {_challengeLevel}";
        }

        private void FlashPanel()
        {
            foreach (var obj in _burgerVisuals)
            {
                if (obj == null) continue;
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color original = sr.color;
                    sr.color = UIStyles.GOLD;
                    sr.DOColor(original, AnimConfig.LEVELUP_COLOR_RESTORE_DURATION);
                }
            }
        }

        private Sprite GenerateRectSprite()
        {
            Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[16];
            for (int i = 0; i < 16; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }
    }
}
