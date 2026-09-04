using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Periodically spawns a <see cref="BurgerFairy"/> while the game is playing. Each fairy rolls
    /// a payload: a consumable (~60%, weighted by GameplayConfig) or currency (~40%, split gems vs
    /// stars by MonetizationConfig.FAIRY_STAR_SHARE). Replaces GemPackSpawner.
    /// </summary>
    public class BurgerFairySpawner : MonoBehaviour
    {
        private float _spawnTimer = MonetizationConfig.FAIRY_SPAWN_INTERVAL;

        private void Update()
        {
            // Read game state each frame (order-independent — F-51). Resetting the timer while not
            // playing means the first eligible interval after resuming starts fresh.
            bool isPlaying = GameManager.Instance != null &&
                             GameManager.Instance.CurrentState == GameState.Playing;
            if (!isPlaying)
            {
                _spawnTimer = MonetizationConfig.FAIRY_SPAWN_INTERVAL;
                return;
            }

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                _spawnTimer = MonetizationConfig.FAIRY_SPAWN_INTERVAL;
                TrySpawnFairy();
            }
        }

        private void TrySpawnFairy()
        {
            if (Rng.Value > MonetizationConfig.FAIRY_SPAWN_CHANCE) return;
            SpawnFairy();
        }

#if UNITY_EDITOR
        /// <summary>Debug (editor-only): immediate spawn, bound to F in TouchInputHandler.</summary>
        public void DebugSpawn() => SpawnFairy();
#endif

        private void SpawnFairy()
        {
            bool fromLeft = Rng.Value > 0.5f;

            float screenEdge = Constants.GEM_SPAWN_EDGE_X;
            float yPos = Rng.Range(Constants.GEM_SPAWN_Y_MIN, Constants.GEM_SPAWN_Y_MAX);

            Vector3 startPos = new Vector3(fromLeft ? -screenEdge : screenEdge, yPos, 0f);
            Vector3 endPos = new Vector3(fromLeft ? screenEdge : -screenEdge,
                yPos + Rng.Range(-AnimConfig.FAIRY_WOBBLE, AnimConfig.FAIRY_WOBBLE), 0f);

            float duration = Rng.Range(AnimConfig.GEM_FLY_DURATION_MIN, AnimConfig.GEM_FLY_DURATION_MAX);

            GameObject obj = new GameObject("BurgerFairy");
            BurgerFairy fairy = obj.AddComponent<BurgerFairy>();
            fairy.Initialize(RollPayload(), startPos, endPos, duration);
        }

        private FairyPayload RollPayload()
        {
            if (Rng.Value >= GameplayConfig.FAIRY_CONSUMABLE_CHANCE)
            {
                // Currency fairy — split between the two currencies.
                return Rng.Value < MonetizationConfig.FAIRY_STAR_SHARE
                    ? FairyPayload.Stars()
                    : FairyPayload.Gems();
            }
            return FairyPayload.Of(RollConsumable());
        }

        private ConsumableType RollConsumable()
        {
            float[] weights = GameplayConfig.CONSUMABLE_SPAWN_WEIGHTS;

            float total = 0f;
            foreach (float w in weights) total += w;

            float roll = Rng.Range(0f, total);
            float acc = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                acc += weights[i];
                if (roll < acc) return (ConsumableType)i;
            }
            return (ConsumableType)(weights.Length - 1);
        }
    }
}
