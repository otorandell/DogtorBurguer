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
        [SerializeField] private float _ingredientSpacing = 0.08f;
        [SerializeField] private float _ingredientScale = 0.5f;
        [SerializeField] private int _sortingOrder = 60;

        [Header("Meter")]
        [SerializeField] private float _meterWidth = 0.2f;
        [SerializeField] private float _meterHeight = 1.6f;
        [SerializeField] private Color _meterBgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color _meterFillColor = new Color(0.2f, 0.9f, 0.3f, 1f);

        private List<IngredientType> _targetIngredients = new List<IngredientType>();
        private int _challengeLevel = 1;
        private int _challengeProgress;
        private string _challengeName;

        // Visual elements
        private List<GameObject> _burgerVisuals = new List<GameObject>();
        private TMPro.TextMeshPro _nameText;
        private TMPro.TextMeshPro _levelText;
        private SpriteRenderer _meterBg;
        private SpriteRenderer _meterFill;
        private GameObject _displayRoot;

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

            // Name text (top of panel)
            GameObject nameObj = new GameObject("ChallengeName");
            nameObj.transform.SetParent(_displayRoot.transform, false);
            nameObj.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            _nameText = nameObj.AddComponent<TMPro.TextMeshPro>();
            _nameText.fontSize = 2.2f;
            _nameText.color = Color.white;
            _nameText.alignment = TMPro.TextAlignmentOptions.Center;
            _nameText.sortingOrder = _sortingOrder + 1;
            RectTransform nameRect = _nameText.GetComponent<RectTransform>();
            nameRect.sizeDelta = new Vector2(2.2f, 0.5f);

            // Meter background
            float meterX = 0.9f;
            float meterY = -0.2f;

            GameObject meterBgObj = new GameObject("MeterBg");
            meterBgObj.transform.SetParent(_displayRoot.transform, false);
            meterBgObj.transform.localPosition = new Vector3(meterX, meterY, 0f);
            _meterBg = meterBgObj.AddComponent<SpriteRenderer>();
            _meterBg.sprite = GenerateRectSprite();
            _meterBg.color = _meterBgColor;
            _meterBg.sortingOrder = _sortingOrder;
            meterBgObj.transform.localScale = new Vector3(_meterWidth, _meterHeight, 1f);

            // Meter fill
            GameObject meterFillObj = new GameObject("MeterFill");
            meterFillObj.transform.SetParent(_displayRoot.transform, false);
            meterFillObj.transform.localPosition = new Vector3(meterX, meterY - _meterHeight * 0.5f, 0f);
            _meterFill = meterFillObj.AddComponent<SpriteRenderer>();
            _meterFill.sprite = GenerateRectSprite();
            _meterFill.color = _meterFillColor;
            _meterFill.sortingOrder = _sortingOrder + 1;
            meterFillObj.transform.localScale = new Vector3(_meterWidth, 0f, 1f);
            // Pivot at bottom: offset the sprite renderer
            _meterFill.transform.localPosition = new Vector3(meterX, meterY - _meterHeight * 0.5f, 0f);

            // Level text
            GameObject levelObj = new GameObject("ChallengeLevel");
            levelObj.transform.SetParent(_displayRoot.transform, false);
            levelObj.transform.localPosition = new Vector3(meterX, meterY - _meterHeight * 0.5f - 0.2f, 0f);
            _levelText = levelObj.AddComponent<TMPro.TextMeshPro>();
            _levelText.fontSize = 2f;
            _levelText.color = Color.white;
            _levelText.alignment = TMPro.TextAlignmentOptions.Center;
            _levelText.sortingOrder = _sortingOrder + 1;
            RectTransform levelRect = _levelText.GetComponent<RectTransform>();
            levelRect.sizeDelta = new Vector2(1f, 0.4f);
        }

        public void GenerateNewChallenge()
        {
            _targetIngredients.Clear();
            ClearBurgerVisuals();

            int targetCount = GetTargetIngredientCount();
            int activeCount = GetActiveIngredientCount();

            for (int i = 0; i < targetCount; i++)
            {
                int typeIndex = Random.Range(0, activeCount);
                _targetIngredients.Add((IngredientType)typeIndex);
            }

            _challengeName = GenerateChallengeName(targetCount);
            _nameText.text = _challengeName;

            CreateBurgerVisual();
            UpdateMeter();
            UpdateLevelText();
        }

        private int GetTargetIngredientCount()
        {
            return Mathf.Min(5, (_challengeLevel + 1) / 2);
        }

        private int GetActiveIngredientCount()
        {
            IngredientSpawner spawner = FindAnyObjectByType<IngredientSpawner>();
            if (spawner != null)
                return spawner.ActiveIngredientCount;
            return Constants.STARTING_INGREDIENT_COUNT;
        }

        private string GenerateChallengeName(int ingredientCount)
        {
            string[] prefixes = { "The", "El", "Dr.", "Chef's", "Super", "Mega", "Ultra", "Lil'" };
            string[] adjectives = { "Spicy", "Wild", "Crazy", "Hot", "Cool", "Epic", "Tasty", "Zesty" };
            string[] nouns = { "Bite", "Stack", "Tower", "Combo", "Special", "Delight", "Dream", "Feast" };

            string prefix = prefixes[Random.Range(0, prefixes.Length)];
            string adj = adjectives[Random.Range(0, adjectives.Length)];
            string noun = nouns[Random.Range(0, nouns.Length)];

            return $"{prefix} {adj} {noun}";
        }

        private void CreateBurgerVisual()
        {
            IngredientSpawner spawner = FindAnyObjectByType<IngredientSpawner>();
            if (spawner == null) return;

            float startY = -(_targetIngredients.Count + 1) * _ingredientSpacing * 0.5f;
            int order = 0;

            // Bottom bun
            CreateIngredientVisual(IngredientType.BunBottom, startY, spawner, order++);

            // Ingredients
            for (int i = 0; i < _targetIngredients.Count; i++)
            {
                float y = startY + (i + 1) * _ingredientSpacing;
                CreateIngredientVisual(_targetIngredients[i], y, spawner, order++);
            }

            // Top bun
            float topY = startY + (_targetIngredients.Count + 1) * _ingredientSpacing;
            CreateIngredientVisual(IngredientType.BunTop, topY, spawner, order++);
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

        private void HandleBurgerCompleted(Vector3 pos, int basePoints, string name, int ingredientCount, List<IngredientType> ingredients)
        {
            if (ingredientCount == 0) return; // Poor burgers don't count

            bool isMatch = CheckMatch(ingredients);
            int globalMult = GetGlobalMultiplier();
            int challengeMult = isMatch ? 3 : 1;
            int finalPoints = basePoints * globalMult * challengeMult;

            // Award the extra score (beyond base already given)
            int extraPoints = finalPoints - basePoints;
            if (extraPoints > 0)
                GameManager.Instance?.AddExtraScore(extraPoints);

            // Show multiplier text
            if (globalMult > 1 || isMatch)
            {
                string multText = $"x{globalMult * challengeMult}";
                Color textColor = isMatch ? new Color(1f, 0.85f, 0f) : Color.white;
                FloatingText.Spawn(pos + Vector3.up * 0.5f, multText, textColor, 4f);
            }

            if (isMatch)
            {
                _challengeProgress++;
                UpdateMeter();
                AudioManager.Instance?.PlayChallengeMatch();
                FlashPanel();

                if (_challengeProgress >= _challengeLevel)
                {
                    LevelUp();
                }
            }
        }

        private bool CheckMatch(List<IngredientType> completedIngredients)
        {
            if (completedIngredients == null || completedIngredients.Count != _targetIngredients.Count)
                return false;

            // Sort both lists and compare (order doesn't matter)
            List<IngredientType> target = new List<IngredientType>(_targetIngredients);
            List<IngredientType> completed = new List<IngredientType>(completedIngredients);

            target.Sort();
            completed.Sort();

            for (int i = 0; i < target.Count; i++)
            {
                if (target[i] != completed[i])
                    return false;
            }
            return true;
        }

        public int GetGlobalMultiplier()
        {
            return 1 + (_challengeLevel - 1) * 5;
        }

        private void LevelUp()
        {
            _challengeLevel++;
            _challengeProgress = 0;

            // Animate level text
            _levelText.transform.DOPunchScale(Vector3.one * 0.3f, 0.4f);

            GenerateNewChallenge();

            Debug.Log($"[BurgerChallenge] Level up! Now level {_challengeLevel}");
        }

        private void UpdateMeter()
        {
            float fill = (float)_challengeProgress / _challengeLevel;
            float fillHeight = _meterHeight * fill;

            if (_meterFill != null)
            {
                // Scale Y to fill amount, position at bottom of meter
                float meterX = 0.9f;
                float meterY = -0.2f;
                float bottomY = meterY - _meterHeight * 0.5f + fillHeight * 0.5f;

                _meterFill.transform.localPosition = new Vector3(meterX, bottomY, 0f);
                _meterFill.transform.localScale = new Vector3(_meterWidth, fillHeight, 1f);
            }
        }

        private void UpdateLevelText()
        {
            if (_levelText != null)
                _levelText.text = $"Lv.{_challengeLevel}";
        }

        private void FlashPanel()
        {
            // Brief gold flash on all burger visuals
            foreach (var obj in _burgerVisuals)
            {
                if (obj == null) continue;
                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Color original = sr.color;
                    sr.color = new Color(1f, 0.85f, 0f);
                    sr.DOColor(original, 0.4f);
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
