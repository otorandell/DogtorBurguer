using UnityEngine;
using DG.Tweening;

namespace DogtorBurguer
{
    public class GemPack : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider;
        private bool _collected;
        private Tween _moveTween;

        public void Initialize(Vector3 startPos, Vector3 endPos, float duration)
        {
            transform.position = startPos;

            // Create sprite
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            _spriteRenderer.sortingOrder = 100;
            _spriteRenderer.color = UIStyles.BTN_GEM_PACK;

            // Create a simple diamond shape via scale
            transform.localScale = Vector3.one * AnimConfig.GEM_START_SCALE;

            // Add collider for tap detection
            CircleCollider2D col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.8f;
            _collider = col;

            // Fly across with sine wobble
            float midY = (startPos.y + endPos.y) * 0.5f + Rng.Range(-1f, 1f);
            Vector3 midPos = new Vector3(
                (startPos.x + endPos.x) * 0.5f,
                midY,
                0f
            );

            Vector3[] path = { startPos, midPos, endPos };
            _moveTween = transform.DOPath(path, duration, PathType.CatmullRom)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    if (!_collected)
                        Destroy(gameObject);
                });

            // Add a gentle rotation
            transform.DORotate(new Vector3(0, 0, 360f), duration * 0.5f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);

            // Pulse scale
            transform.DOScale(Vector3.one * AnimConfig.GEM_PULSE_MAX_SCALE, AnimConfig.GEM_PULSE_DURATION)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private void OnMouseDown()
        {
            Collect();
        }

        private void Collect()
        {
            if (_collected) return;
            _collected = true;

            // Award gems
            if (SaveDataManager.Instance != null)
                SaveDataManager.Instance.AddGems(Constants.GEM_PACK_VALUE);

            Debug.Log($"[GemPack] Collected! +{Constants.GEM_PACK_VALUE} gems");

            // Collect animation
            _moveTween?.Kill();
            DOTween.Kill(transform);

            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOScale(Vector3.one * AnimConfig.GEM_COLLECT_SCALE_UP, AnimConfig.GEM_COLLECT_SCALE_UP_DURATION).SetEase(Ease.OutBack));
            seq.Append(transform.DOScale(Vector3.zero, AnimConfig.GEM_COLLECT_SCALE_DOWN_DURATION).SetEase(Ease.InBack));
            seq.OnComplete(() => Destroy(gameObject));
        }

        private void OnDestroy()
        {
            _moveTween?.Kill();
            DOTween.Kill(transform);
        }
    }
}
