using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Special Orders challenge — the logic/model half (F-56). Owns the current order,
    /// match checking, progression, and score award; raises events the view renders.
    /// All on-screen construction + animation lives in <see cref="BurgerChallengeView"/>, a
    /// screen-space UGUI panel this creates at runtime. Layout is owned by the view (UIStyles), so the
    /// model holds only logic + the read-only state the view renders.
    /// </summary>
    public class BurgerChallenge : Singleton<BurgerChallenge>
    {
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

        /// <summary>Progress toward the next challenge level (0..1) — drives the Mult meter.</summary>
        public float ChallengeFill => ProgressTarget > 0 ? (float)_challengeProgress / ProgressTarget : 0f;

        private int ProgressTarget => _challengeLevel + 1; // matches needed to level up

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

            // Contains: every required ingredient must be present — extras OK. (Tried exact-match
            // on 2026-09-04, reverted the same day: the panel now carries a "Contains" word instead.)
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
                FloatingText.Spawn(pos + Vector3.up * 0.5f, multText, UIStyles.HUD_TEXT_FILL,
                    UIStyles.WORLD_FLOATING_TEXT_SIZE, "ui_popup_plate_mult");
            }

            if (!isMatch) return;

            _challengeProgress++;

            // Completed orders are the star faucet: award scales with the challenge level.
            int stars = MonetizationConfig.STARS_PER_ORDER_BASE
                + MonetizationConfig.STARS_PER_ORDER_PER_LEVEL * (_challengeLevel - 1);
            GameManager.Instance?.AwardStars(stars);
            FloatingText.Spawn(pos + Vector3.up * 1.1f, $"{stars}!", UIStyles.HUD_TEXT_FILL,
                UIStyles.WORLD_STAR_POPUP_SIZE, "ui_popup_plate_mult");

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
