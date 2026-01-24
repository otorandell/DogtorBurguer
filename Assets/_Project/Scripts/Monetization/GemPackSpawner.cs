using UnityEngine;

namespace DogtorBurguer
{
    public class GemPackSpawner : MonoBehaviour
    {
        private float _spawnTimer;
        private bool _isActive;

        private void Start()
        {
            _spawnTimer = Constants.GEM_PACK_SPAWN_INTERVAL;

            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += HandleStateChanged;

            _isActive = GameManager.Instance != null &&
                        GameManager.Instance.CurrentState == GameState.Playing;
        }

        private void Update()
        {
            if (!_isActive) return;

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer <= 0f)
            {
                _spawnTimer = Constants.GEM_PACK_SPAWN_INTERVAL;
                TrySpawnGemPack();
            }
        }

        private void TrySpawnGemPack()
        {
            if (Rng.Value > Constants.GEM_PACK_SPAWN_CHANCE) return;

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

        private void HandleStateChanged(GameState state)
        {
            _isActive = state == GameState.Playing;

            if (!_isActive)
                _spawnTimer = Constants.GEM_PACK_SPAWN_INTERVAL;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
        }
    }
}
