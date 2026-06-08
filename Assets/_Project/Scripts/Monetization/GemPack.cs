using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace DogtorBurguer
{
    public class GemPack : MonoBehaviour
    {
        // Active, uncollected packs. Taps are routed here from TouchInputHandler:
        // the project uses the New Input System, so OnMouseDown never fires (F-48).
        private static readonly List<GemPack> _active = new List<GemPack>();
        private static Sprite _gemSprite;

        // World-space tap radius — generous, since the pulsing visual is smaller.
        private const float TapRadius = 0.8f;

        private SpriteRenderer _spriteRenderer;
        private bool _collected;
        private Tween _moveTween;

        public void Initialize(Vector3 startPos, Vector3 endPos, float duration)
        {
            transform.position = startPos;
            BuildVisual();
            PlayFlyIn(startPos, endPos, duration);
            _active.Add(this);
        }

        private void BuildVisual()
        {
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            _spriteRenderer.sprite = GetGemSprite();
            _spriteRenderer.sortingOrder = Constants.SORT_GEM_PACK;
            _spriteRenderer.color = UIStyles.GEM_PACK;
            transform.localScale = Vector3.one * AnimConfig.GEM_START_SCALE;
        }

        private void PlayFlyIn(Vector3 startPos, Vector3 endPos, float duration)
        {
            // Fly across with a sine wobble.
            float midY = (startPos.y + endPos.y) * 0.5f + Rng.Range(-AnimConfig.GEM_WOBBLE, AnimConfig.GEM_WOBBLE);
            Vector3 midPos = new Vector3((startPos.x + endPos.x) * 0.5f, midY, 0f);
            Vector3[] path = { startPos, midPos, endPos };

            _moveTween = transform.DOPath(path, duration, PathType.CatmullRom)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    if (!_collected)
                        Destroy(gameObject);
                });

            transform.DORotate(new Vector3(0, 0, 360f), duration * 0.5f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);

            transform.DOScale(Vector3.one * AnimConfig.GEM_PULSE_MAX_SCALE, AnimConfig.GEM_PULSE_DURATION)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        /// <summary>
        /// Collects the active pack nearest <paramref name="worldPos"/> within tap range.
        /// Returns true if a pack was collected (so the tap is consumed).
        /// </summary>
        public static bool TryTapAt(Vector3 worldPos)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                GemPack pack = _active[i];
                if (pack == null)
                {
                    _active.RemoveAt(i);
                    continue;
                }

                if (Vector2.Distance(worldPos, pack.transform.position) <= TapRadius)
                {
                    pack.Collect();
                    return true;
                }
            }
            return false;
        }

        private void Collect()
        {
            if (_collected) return;
            _collected = true;
            _active.Remove(this);

            AwardGems();
            PlayCollect();
        }

        private void AwardGems()
        {
            if (SaveDataManager.Instance != null)
                SaveDataManager.Instance.AddGems(MonetizationConfig.GEM_PACK_VALUE);

            Debug.Log($"[GemPack] Collected! +{MonetizationConfig.GEM_PACK_VALUE} gems");
        }

        private void PlayCollect()
        {
            _moveTween?.Kill();
            DOTween.Kill(transform);

            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOScale(Vector3.one * AnimConfig.GEM_COLLECT_SCALE_UP, AnimConfig.GEM_COLLECT_SCALE_UP_DURATION).SetEase(Ease.OutBack));
            seq.Append(transform.DOScale(Vector3.zero, AnimConfig.GEM_COLLECT_SCALE_DOWN_DURATION).SetEase(Ease.InBack));
            seq.OnComplete(() => Destroy(gameObject));
        }

        private void OnDestroy()
        {
            _active.Remove(this);
            _moveTween?.Kill();
            DOTween.Kill(transform);
        }

        /// <summary>Builds the diamond sprite once and reuses it for every pack.</summary>
        private static Sprite GetGemSprite()
        {
            if (_gemSprite != null) return _gemSprite;

            const int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = (size - 1) / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Rhombus: inside when Manhattan distance from center is within the radius
                    bool inside = Mathf.Abs(x - center) + Mathf.Abs(y - center) <= center;
                    tex.SetPixel(x, y, inside ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            _gemSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _gemSprite;
        }
    }
}
