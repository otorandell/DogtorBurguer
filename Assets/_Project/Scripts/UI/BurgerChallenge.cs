using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Special Orders challenge — the logic/model half (F-56). Owns the current order,
    /// match checking, progression, and score award; raises events the view renders.
    /// All on-screen construction + animation lives in <see cref="BurgerChallengeView"/>,
    /// which this creates at runtime.
    ///
    /// Visual config stays here as SerializeFields (kept on the scene component so the
    /// inspector-assigned placeholder sprite survives) and is exposed read-only to the view.
    /// </summary>
    public class BurgerChallenge : Singleton<BurgerChallenge>
    {
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

        // --- challenge state ---
        private OrderType _orderType;
        private int _requiredSize;
        private readonly List<IngredientType> _targetIngredients = new List<IngredientType>();
        private int _challengeLevel = 1;
        private int _challengeProgress;
        private string _challengeName;

        private IngredientSpawner _spawner;
        private BurgerChallengeView _view;

        // --- events the view renders ---
        public event Action OnChallengeChanged; // new order rolled → rebuild visuals + name + meter
        public event Action OnMatched;           // a matching burger landed → flash the order
        public event Action OnLevelUp;           // progress filled → play level-up effect

        // --- state exposed to the view ---
        public OrderType CurrentOrderType => _orderType;
        public int RequiredSize => _requiredSize;
        public IReadOnlyList<IngredientType> TargetIngredients => _targetIngredients;
        public string ChallengeName => _challengeName;
        public int Level => _challengeLevel;
        public int Progress => _challengeProgress;
        public int ProgressTarget => _challengeLevel + 1;

        // --- visual config exposed to the view ---
        public Vector2 PanelCenter => _panelCenter;
        public float IngredientSpacing => _ingredientSpacing;
        public float IngredientScale => _ingredientScale;
        public int SortingOrder => _sortingOrder;
        public Sprite PlaceholderSprite => _spritePlaceholder;
        public float MeterWidth => _meterWidth;
        public float MeterHeight => _meterHeight;
        public Color MeterBgColor => _meterBgColor;
        public Color MeterFillColor => _meterFillColor;

        // --- spawner-backed lookups (resolved once, F-59) ---
        public int ActiveIngredientCount =>
            _spawner != null ? _spawner.ActiveIngredientCount : GameplayConfig.STARTING_INGREDIENT_COUNT;

        public Sprite GetIngredientSprite(IngredientType type) =>
            _spawner != null ? _spawner.GetSpriteForType(type) : null;

        private void Start()
        {
            _spawner = FindAnyObjectByType<IngredientSpawner>();

            _view = gameObject.AddComponent<BurgerChallengeView>();
            _view.Initialize(this);

            GenerateNewChallenge();
            SubscribeEvents();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (GridManager.Instance != null)
                GridManager.Instance.OnBurgerWithIngredients -= HandleBurgerCompleted;
        }

        private void SubscribeEvents()
        {
            if (GridManager.Instance != null)
                GridManager.Instance.OnBurgerWithIngredients += HandleBurgerCompleted;
        }

        /// <summary>Rolls a new order and notifies the view to rebuild.</summary>
        public void GenerateNewChallenge()
        {
            _targetIngredients.Clear();
            _orderType = Rng.Range(0, 2) == 0 ? OrderType.Size : OrderType.Contains;

            if (_orderType == OrderType.Size)
            {
                _requiredSize = Mathf.Clamp(_challengeLevel + 1, GameplayConfig.CHALLENGE_MIN_SIZE, GameplayConfig.CHALLENGE_MAX_SIZE);
                _challengeName = $"{_requiredSize}+ Ingredients";
            }
            else
            {
                int count = Mathf.Clamp(_challengeLevel, 1, GameplayConfig.CHALLENGE_MAX_CONTAINS);
                GenerateContainsIngredients(count);
                _challengeName = BuildContainsName();
            }

            OnChallengeChanged?.Invoke();
        }

        private void GenerateContainsIngredients(int count)
        {
            // Pick unique random ingredients from the active pool.
            int activeCount = ActiveIngredientCount;
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

            StringBuilder sb = new StringBuilder("Has: ");
            for (int i = 0; i < _targetIngredients.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(_targetIngredients[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Pure match check with no side effects. Used by GridManager to override the
        /// burger display name to "Order Complete!".
        /// </summary>
        public bool IsOrderMatch(List<IngredientType> ingredients, int ingredientCount)
        {
            if (ingredientCount == 0) return false;

            if (_orderType == OrderType.Size)
                return ingredientCount >= _requiredSize;

            // Contains: every required ingredient must be present (extras OK).
            foreach (var required in _targetIngredients)
            {
                if (!ingredients.Contains(required))
                    return false;
            }
            return true;
        }

        public int GetGlobalMultiplier()
        {
            return 1 + (_challengeLevel - 1) * GameplayConfig.CHALLENGE_GLOBAL_MULT_PER_LEVEL;
        }

        private void HandleBurgerCompleted(Vector3 pos, int basePoints, string name, int ingredientCount, List<IngredientType> ingredients)
        {
            if (ingredientCount == 0) return;

            bool isMatch = IsOrderMatch(ingredients, ingredientCount);
            int globalMult = GetGlobalMultiplier();
            int challengeMult = isMatch ? GameplayConfig.CHALLENGE_MATCH_MULTIPLIER : 1;
            int finalPoints = basePoints * globalMult * challengeMult;

            // Award the extra score beyond the base already granted.
            int extraPoints = finalPoints - basePoints;
            if (extraPoints > 0)
                GameManager.Instance?.AddExtraScore(extraPoints);

            // Multiplier feedback popup at the burger position.
            if (globalMult > 1 || isMatch)
            {
                string multText = $"x{globalMult * challengeMult}";
                Color textColor = isMatch ? UIStyles.GOLD : UIStyles.TEXT_UI;
                FloatingText.Spawn(pos + Vector3.up * 0.5f, multText, textColor, UIStyles.WORLD_FLOATING_TEXT_SIZE);
            }

            if (!isMatch) return;

            _challengeProgress++;
            AudioManager.Instance?.PlayChallengeMatch();
            OnMatched?.Invoke();

            if (_challengeProgress >= ProgressTarget)
                LevelUp();
            else
                GenerateNewChallenge();
        }

        private void LevelUp()
        {
            _challengeLevel++;
            _challengeProgress = 0;

            // The view animates the level-up and calls GenerateNewChallenge when it finishes.
            OnLevelUp?.Invoke();

            Debug.Log($"[BurgerChallenge] Level up! Now level {_challengeLevel}");
        }
    }
}
