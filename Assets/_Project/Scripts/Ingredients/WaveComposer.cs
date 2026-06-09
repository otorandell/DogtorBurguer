using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Decides what each spawned slot contains — ingredient-type rolls and bun pacing/type rules.
    /// Owns the bun-pacing counter and reads grid state via GridManager. Column selection lives in
    /// IngredientSpawner (the preview queue). Extracted so the spawner only orchestrates + spawns (F-38).
    /// </summary>
    public class WaveComposer
    {
        private readonly bool _enableForcedBunSpawn;
        private readonly float _forceBunMultiplier;
        private int _spawnsSinceLastBun;

        public WaveComposer(bool enableForcedBunSpawn, float forceBunMultiplier)
        {
            _enableForcedBunSpawn = enableForcedBunSpawn;
            _forceBunMultiplier = forceBunMultiplier;
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
            if (_enableForcedBunSpawn)
            {
                int threshold = (int)(activeIngredientCount * _forceBunMultiplier);
                if (_spawnsSinceLastBun >= threshold)
                {
                    _spawnsSinceLastBun = 0;
                    return GetBunType();
                }
            }

            int roll = Rng.Range(0, activeIngredientCount + 1);
            if (roll < activeIngredientCount)
            {
                _spawnsSinceLastBun++;
                return GameplayConfig.REGULAR_INGREDIENTS[roll];
            }

            _spawnsSinceLastBun = 0;
            return GetBunType();
        }

        private IngredientType GetBunType()
        {
            if (!GridHasBottomBun())
                return IngredientType.BunBottom;

            int bottomCount = CountBottomBunsOnGrid();
            float topChance = Mathf.Min(GameplayConfig.BUN_TOP_BASE_CHANCE + bottomCount * GameplayConfig.BUN_TOP_CHANCE_PER_BOTTOM, GameplayConfig.BUN_TOP_CHANCE_CAP);
            return Rng.Value < topChance ? IngredientType.BunTop : IngredientType.BunBottom;
        }

        private int CountBottomBunsOnGrid()
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

        private bool GridHasBottomBun()
        {
            if (GridManager.Instance == null) return false;

            for (int c = 0; c < Constants.COLUMN_COUNT; c++)
            {
                Column col = GridManager.Instance.GetColumn(c);
                if (col == null) continue;

                foreach (var ing in col.GetAllIngredients())
                {
                    if (ing.Type == IngredientType.BunBottom)
                        return true;
                }
            }
            return false;
        }
    }
}
