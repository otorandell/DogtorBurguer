using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Decides what a wave contains — column selection, ingredient-type rolls, and bun
    /// pacing/type rules. Owns the bun-pacing counter and reads grid state via GridManager.
    /// Extracted from IngredientSpawner so the spawner only orchestrates + spawns (F-38).
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

        /// <summary>Rolls the slots for one wave, given the active pool size and triple-wave chance.</summary>
        public List<WaveSlot> RollWave(int activeIngredientCount, float tripleWaveChance)
        {
            int waveSize = Rng.Value < tripleWaveChance ? 3 : 2;
            var data = new List<WaveSlot>();
            var usedColumns = new List<int>();

            for (int i = 0; i < waveSize; i++)
            {
                int col = GetUnusedColumn(usedColumns);
                if (col < 0) break;
                usedColumns.Add(col);
                data.Add(new WaveSlot(GetSpawnType(activeIngredientCount), col));
            }
            return data;
        }

        private int GetUnusedColumn(List<int> usedColumns)
        {
            List<int> available = new List<int>();
            for (int i = 0; i < Constants.COLUMN_COUNT; i++)
            {
                if (!usedColumns.Contains(i))
                    available.Add(i);
            }
            if (available.Count == 0) return -1;
            return available[Rng.Range(0, available.Count)];
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
