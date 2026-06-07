using UnityEngine;

namespace DogtorBurguer
{
    public class GemPackSpawner : MonoBehaviour
    {
        private float _spawnTimer = MonetizationConfig.GEM_PACK_SPAWN_INTERVAL;

        private void Update()
        {
            // Read game state directly each frame. The old approach subscribed once in
            // Start and only if GameManager already existed — if it didn't, spawning was
            // silently disabled forever. Reading here is order-independent (F-51).
            bool isPlaying = GameManager.Instance != null &&
                             GameManager.Instance.CurrentState == GameState.Playing;
            if (!isPlaying)
            {
                _spawnTimer = MonetizationConfig.GEM_PACK_SPAWN_INTERVAL;
                return;
            }

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                _spawnTimer = MonetizationConfig.GEM_PACK_SPAWN_INTERVAL;
                TrySpawnGemPack();
            }
        }

        private void TrySpawnGemPack()
        {
            if (Rng.Value > MonetizationConfig.GEM_PACK_SPAWN_CHANCE) return;

            SpawnGemPack();
        }

        private void SpawnGemPack()
        {
            // Determine direction (left-to-right or right-to-left)
            bool fromLeft = Rng.Value > 0.5f;

            float screenEdge = 5f; // Off-screen X position
            float yPos = Rng.Range(0f, 3f); // Upper area of screen

            Vector3 startPos = new Vector3(fromLeft ? -screenEdge : screenEdge, yPos, 0f);
            Vector3 endPos = new Vector3(fromLeft ? screenEdge : -screenEdge, yPos + Rng.Range(-1f, 1f), 0f);

            float duration = Rng.Range(3f, 5f);

            GameObject packObj = new GameObject("GemPack");
            GemPack pack = packObj.AddComponent<GemPack>();
            pack.Initialize(startPos, endPos, duration);
        }
    }
}
