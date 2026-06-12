using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace DogtorBurguer
{
    /// <summary>
    /// A fairy that flies across the screen carrying a payload (gems or a consumable) and can be
    /// tapped to collect. Replaces the old GemPack — the gem is now just one possible payload.
    /// Taps are routed from TouchInputHandler (New Input System → OnMouseDown never fires).
    /// </summary>
    public class BurgerFairy : MonoBehaviour
    {
        // Active, uncollected fairies. Taps are matched against this registry.
        private static readonly List<BurgerFairy> _active = new List<BurgerFairy>();

        // World-space tap radius — generous, since the pulsing visual is smaller.
        private const float TapRadius = 0.8f;

        private FairyPayload _payload;
        private bool _collected;
        private Tween _moveTween;

        public void Initialize(FairyPayload payload, Vector3 startPos, Vector3 endPos, float duration)
        {
            _payload = payload;
            transform.position = startPos;
            BuildVisual();
            PlayFlyIn(startPos, endPos, duration);
            _active.Add(this);
        }

        private void BuildVisual()
        {
            // Body + badge on children so the root transform stays free for the fly path + pulse,
            // and each sprite is normalized to a world height (reward sprites import large).
            GameObject bodyObj = new GameObject("Body");
            bodyObj.transform.SetParent(transform, false);
            SpriteRenderer body = bodyObj.AddComponent<SpriteRenderer>();
            body.sprite = RewardArt.Fairy;
            body.sortingOrder = Constants.SORT_GEM_PACK;
            SpriteFit.Height(body, UIStyles.FAIRY_BODY_HEIGHT);

            // Payload badge rides on the fairy (gem or consumable icon), drawn just above it.
            GameObject badgeObj = new GameObject("Badge");
            badgeObj.transform.SetParent(transform, false);
            SpriteRenderer badge = badgeObj.AddComponent<SpriteRenderer>();
            badge.sprite = RewardArt.Badge(_payload);
            badge.sortingOrder = Constants.SORT_FAIRY_BADGE;
            SpriteFit.Height(badge, UIStyles.FAIRY_BADGE_HEIGHT);
        }

        private void PlayFlyIn(Vector3 startPos, Vector3 endPos, float duration)
        {
            // Fly across with a sine wobble (Catmull-Rom through a wandering midpoint).
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

            // Gentle breathing pulse so the fairy reads as alive (no spin — it's not a coin).
            transform.DOScale(Vector3.one * AnimConfig.FAIRY_PULSE_SCALE, AnimConfig.GEM_PULSE_DURATION)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        /// <summary>
        /// Collects the active fairy nearest <paramref name="worldPos"/> within tap range.
        /// Returns true if one was collected (so the tap is consumed).
        /// </summary>
        public static bool TryTapAt(Vector3 worldPos)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                BurgerFairy fairy = _active[i];
                if (fairy == null)
                {
                    _active.RemoveAt(i);
                    continue;
                }

                if (Vector2.Distance(worldPos, fairy.transform.position) <= TapRadius)
                {
                    fairy.Collect();
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

            Award();
            PlayCollect();
        }

        private void Award()
        {
            if (_payload.Kind == FairyPayloadKind.Gems)
            {
                if (SaveDataManager.Instance != null)
                    SaveDataManager.Instance.AddGems(MonetizationConfig.GEM_PACK_VALUE);
                Debug.Log($"[BurgerFairy] Collected gems! +{MonetizationConfig.GEM_PACK_VALUE}");
            }
            else
            {
                ConsumableInventory.Instance?.Add(_payload.Consumable);
                AudioManager.Instance?.PlayConsumableCollect();
                Debug.Log($"[BurgerFairy] Collected consumable: {_payload.Consumable}");
            }
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

#if UNITY_EDITOR
        // Tap hit-zone for collecting this fairy (matches TryTapAt). Visible in the Scene view
        // during play, since fairies are runtime-spawned. Toggle via Unity's Gizmos menu.
        private void OnDrawGizmos()
        {
            Gizmos.color = GizmoStyles.FairyTap;
            Gizmos.DrawWireSphere(transform.position, TapRadius);
        }
#endif
    }
}
