using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Decides what each spawned slot contains. Regular ingredients come from an even-spread
    /// <see cref="IngredientBag"/>; buns are a decoupled, grid-aware economy (flat bottom rate +
    /// a top rate that scales with the number of open bottoms, so the board self-balances rather
    /// than passively accumulating unclosed burgers). Column selection lives in IngredientSpawner
    /// (the preview queue); this only chooses types. Extracted so the spawner just orchestrates (F-38).
    /// </summary>
    public class WaveComposer
    {
        private readonly IngredientBag _bag = new();
        private readonly IngredientRoster _roster; // this run's random unlock order
        private int _piecesSinceBun;

        public WaveComposer(IngredientRoster roster)
        {
            _roster = roster;
        }

        /// <summary>Rolls a single slot for a specific column (used to build/refill the preview queue).</summary>
        public WaveSlot RollSlot(int activeIngredientCount, int column)
        {
            return new WaveSlot(GetSpawnType(activeIngredientCount), column);
        }

        /// <summary>How many ingredients a wave contains (3 on a triple roll, else 2).</summary>
        public int RollWaveSize(float tripleWaveChance)
        {
            return Rng.Value < tripleWaveChance ? 3 : 2;
        }

        private IngredientType GetSpawnType(int activeIngredientCount)
        {
            if (TryRollBun(out IngredientType bun))
                return bun;

            _piecesSinceBun++;
            return _bag.Next(_roster, activeIngredientCount);
        }

        /// <summary>
        /// Decides whether this slot is a bun, and which. Bottom is a flat chance (the "start a
        /// burger" resource); top only when there's an open bottom to close and scales up with the
        /// backlog (the "close a burger" resource), so open bottoms hover near the balance point.
        /// A drought guard forces a bottom if buns have been absent too long.
        /// </summary>
        private bool TryRollBun(out IngredientType bunType)
        {
            // Drought guard: never let burger-building stall for lack of a bottom to start on.
            if (_piecesSinceBun >= GameplayConfig.BUN_DROUGHT_LIMIT)
            {
                _piecesSinceBun = 0;
                bunType = IngredientType.BunBottom;
                return true;
            }

            // Flat bottom chance — safe to flow freely; surplus bottoms cancel each other on the grid.
            if (Rng.Value < GameplayConfig.BOTTOM_BUN_CHANCE)
            {
                _piecesSinceBun = 0;
                bunType = IngredientType.BunBottom;
                return true;
            }

            // Top only matters when there's an unclosed bottom to land on (a lone top self-destructs).
            int openBottoms = CountOpenBottoms();
            if (openBottoms >= 1)
            {
                float topChance = Mathf.Min(
                    GameplayConfig.TOP_BUN_BASE_CHANCE + GameplayConfig.TOP_BUN_CHANCE_PER_EXTRA_BOTTOM * (openBottoms - 1),
                    GameplayConfig.TOP_BUN_CHANCE_CAP);
                if (Rng.Value < topChance)
                {
                    _piecesSinceBun = 0;
                    bunType = IngredientType.BunTop;
                    return true;
                }
            }

            bunType = default;
            return false;
        }

        /// <summary>Counts unclosed bottom buns on the grid (every on-grid BunBottom is still open).</summary>
        private int CountOpenBottoms()
        {
            if (GridManager.Instance == null) return 0;

            int count = 0;
            for (int c = 0; c < Constants.COLUMN_COUNT; c++)
            {
                Column col = GridManager.Instance.GetColumn(c);
                if (col == null) continue;

                foreach (var ing in col.GetAllIngredients())
                {
                    if (ing.Type == IngredientType.BunBottom)
                        count++;
                }
            }
            return count;
        }
    }
}
