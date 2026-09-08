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
        private int _requiredSize;
        private readonly List<IngredientType> _targetIngredients = new List<IngredientType>();
        private int _challengeLevel = 1;
        private int _challengeProgress;
        private string _challengeName;

        private IngredientSpawner _spawner;
        private BurgerChallengeView _view;
        private bool _tutorialArmed; // tutorial: matches count only once the scripted order is set

        // --- events the view renders ---
        public event Action OnChallengeChanged; // new order rolled → rebuild visuals + name + meter
        public event Action OnMatched;           // a matching burger landed → flash the order
        public event Action OnLevelUp;           // progress filled → play level-up effect

        // --- state exposed to the view ---
        public int RequiredSize => _requiredSize;
        public IReadOnlyList<IngredientType> TargetIngredients => _targetIngredients;
        public string ChallengeName => _challengeName;
        public int Level => _challengeLevel;

        /// <summary>Progress toward the next challenge level (0..1) — drives the Mult meter.</summary>
        public float ChallengeFill => ProgressTarget > 0 ? (float)_challengeProgress / ProgressTarget : 0f;

        // Orders needed to level up: 2, 2, 3, 3, 3 … (grows every second level, capped) so the
        // meter never drags late-run (2026-09-05 — the 0.25-step multiplier pays too little for
        // an ever-longer climb).
        private int ProgressTarget =>
            Mathf.Min((_challengeLevel + 3) / 2, GameplayConfig.CHALLENGE_ORDERS_TO_LEVEL_CAP);

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

            if (TutorialMode.IsActive || TutorialMode.ShouldRun)
            {
                // Tutorial: no auto order, and hide the panel OURSELVES — same-frame Start order
                // vs TutorialManager is unspecified, so its SetPanelVisible call can arrive
                // before the view exists (the broken empty card, found 2026-09-07). ShouldRun
                // covers the case where this Start wins the race.
                _view.SetVisible(false);
            }
            else
            {
                GenerateNewChallenge();
            }
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

        /// <summary>Rolls a new order from the ladder and notifies the view.</summary>
        public void GenerateNewChallenge()
        {
            // The tutorial drives its one scripted order; no auto rolls (incl. post-level-up).
            if (TutorialMode.IsActive) return;

            _targetIngredients.Clear();

            // An exact-count recipe — named ingredients + free slots — read from the per-level
            // ladder tables (GameplayConfig.ORDER_*_BY_LEVEL, clamped at the last entry).
            // Ingredient ORDER never matters; extras don't fit (the count is exact).
            int i = Mathf.Min(_challengeLevel - 1, GameplayConfig.ORDER_SIZE_BY_LEVEL.Length - 1);
            _requiredSize = Mathf.Min(GameplayConfig.ORDER_SIZE_BY_LEVEL[i], GameplayConfig.ORDER_MAX_SIZE);
            GenerateContainsIngredients(Mathf.Min(GameplayConfig.ORDER_NAMED_BY_LEVEL[i], _requiredSize));
            _challengeName = BuildContainsName();

            OnChallengeChanged?.Invoke();
        }

        private void GenerateContainsIngredients(int count)
        {
            // Unique picks from the active pool while possible; duplicates once an order needs
            // more named ingredients than there are active types (late levels — a real burger
            // repeats patties anyway; the multiset match handles copies).
            int activeCount = ActiveIngredientCount;
            List<int> available = new List<int>();
            for (int i = 0; i < activeCount; i++)
                available.Add(i);

            for (int i = 0; i < count; i++)
            {
                if (available.Count == 0)
                {
                    _targetIngredients.Add(ActiveType(Rng.Range(0, activeCount)));
                    continue;
                }
                int idx = Rng.Range(0, available.Count);
                _targetIngredients.Add(ActiveType(available[idx]));
                available.RemoveAt(idx);
            }
        }

        // This run's type at an unlock position — the per-run random roster (2026-09-08).
        private IngredientType ActiveType(int index) =>
            _spawner != null ? _spawner.ActiveTypeAt(index) : GameplayConfig.REGULAR_INGREDIENTS[index];

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

            // The recipe (2026-09-05): the total count is EXACT, every named ingredient must
            // be present (as a multiset — a duplicated name needs that many copies), the free
            // slots take anything, and ordering never matters.
            if (ingredientCount != _requiredSize) return false;

            List<IngredientType> pool = new List<IngredientType>(ingredients);
            foreach (var required in _targetIngredients)
            {
                if (!pool.Remove(required))
                    return false;
            }
            return true;
        }

        /// <summary>The global score multiplier: 1 + 0.25·(level−1) — 1, 1.25, 1.5 … Applies to
        /// ALL gameplay score (matches and burgers; consumable removals stay flat).</summary>
        public float Multiplier => 1f + (_challengeLevel - 1) * GameplayConfig.CHALLENGE_MULT_STEP;

        /// <summary>Base gameplay points scaled by the live global multiplier (rounded).</summary>
        public static int Scaled(int basePoints) =>
            Instance != null ? Mathf.RoundToInt(basePoints * Instance.Multiplier) : basePoints;

        /// <summary>Tutorial: shows/hides the whole order panel (hidden until the Order step).</summary>
        public void SetPanelVisible(bool visible) => _view?.SetVisible(visible);

        /// <summary>Tutorial: installs one scripted exact-count order and pre-fills the meter so
        /// completing it levels the multiplier up (the showcase). Arms match handling.</summary>
        public void SetScriptedOrder(IngredientType target, int exactCount, int progress)
        {
            _tutorialArmed = true;
            _requiredSize = exactCount;
            _targetIngredients.Clear();
            _targetIngredients.Add(target);
            _challengeProgress = progress;
            _challengeName = BuildContainsName();
            OnChallengeChanged?.Invoke();
        }

        // The burger's SCORE is fully handled upstream (GridManager computes the final
        // multiplied points, GameManager adds them, the popup shows them — no x-badge popup
        // since the 2026-09-05 redesign). This handler owns only order progression + stars.
        private void HandleBurgerCompleted(Vector3 pos, int finalPoints, string name, int ingredientCount, List<IngredientType> ingredients)
        {
            if (ingredientCount == 0) return;
            if (TutorialMode.IsActive && !_tutorialArmed) return; // no order on the card yet
            if (!IsOrderMatch(ingredients, ingredientCount)) return;

            _challengeProgress++;

            // Completed orders are the star faucet: award scales with the challenge level.
            int stars = MonetizationConfig.STARS_PER_ORDER_BASE
                + MonetizationConfig.STARS_PER_ORDER_PER_LEVEL * (_challengeLevel - 1);
            GameManager.Instance?.AwardStars(stars);
            FloatingText.Spawn(pos + Vector3.up * 1.1f, $"{stars}!", UIStyles.HUD_TEXT_FILL,
                UIStyles.WORLD_STAR_POPUP_SIZE, "ui_popup_plate_mult");

            OnMatched?.Invoke();

            // The leveling match plays the distinct mult-level-up jingle INSTEAD of the match
            // chord (they were overlapping/reused — Oscar, 2026-09-05).
            if (_challengeProgress >= ProgressTarget)
            {
                AudioManager.Instance?.PlayChallengeLevelUp();
                LevelUp();
            }
            else
            {
                AudioManager.Instance?.PlayChallengeMatch();
                GenerateNewChallenge();
            }
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
