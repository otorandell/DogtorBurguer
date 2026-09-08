using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace DogtorBurguer
{
    /// <summary>
    /// A fairy that flies across the screen carrying a payload (gems, stars, or a consumable) and
    /// can be tapped to collect. Each payload has its own full-body fairy illustration.
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
            // One full-body illustration per payload (the cargo is drawn into the art — the old
            // body + badge overlay is gone). On a child so the root transform stays free for the
            // fly path + pulse; normalized to a world height (the art imports large).
            GameObject bodyObj = new GameObject("Body");
            bodyObj.transform.SetParent(transform, false);
            SpriteRenderer body = bodyObj.AddComponent<SpriteRenderer>();
            body.sprite = RewardArt.Fairy(_payload);
            body.sortingOrder = Constants.SORT_GEM_PACK;
            SpriteFit.Height(body, UIStyles.FAIRY_BODY_HEIGHT);
        }

        private void PlayFlyIn(Vector3 startPos, Vector3 endPos, float duration)
        {
            // An erratic flit: Catmull-Rom through several scattered waypoints — each one jittered
            // vertically (FAIRY_WOBBLE) and horizontally (FAIRY_WOBBLE_X, so the pacing surges and
            // stalls) instead of the old single wandering midpoint.
            int waypoints = AnimConfig.FAIRY_PATH_WAYPOINTS;
            Vector3[] path = new Vector3[waypoints + 2];
            path[0] = startPos;
            for (int i = 1; i <= waypoints; i++)
            {
                float t = (float)i / (waypoints + 1);
                path[i] = new Vector3(
                    Mathf.Lerp(startPos.x, endPos.x, t) + Rng.Range(-AnimConfig.FAIRY_WOBBLE_X, AnimConfig.FAIRY_WOBBLE_X),
                    Mathf.Lerp(startPos.y, endPos.y, t) + Rng.Range(-AnimConfig.FAIRY_WOBBLE, AnimConfig.FAIRY_WOBBLE),
                    0f);
            }
            path[waypoints + 1] = endPos;

            _moveTween = transform.DOPath(path, duration, PathType.CatmullRom)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    if (!_collected)
                        Destroy(gameObject);
                });

            // Gentle breathing pulse so the fairy reads as alive (no spin — it's not a coin).
            transform.DOScale(Vector3.one * AnimConfig.FAIRY_PULSE_SCALE, AnimConfig.FAIRY_PULSE_DURATION)
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

            AudioManager.Instance?.PlayConsumableCollect(); // the collect chime — EVERY payload (2026-09-08)
            Award();
            PlayCollect();
        }

        private void Award()
        {
            // Every payload pops its loot — amount + inline icon on the yellow reward plate
            // (the fairy silently vanishing read as "did I get anything?" — 2026-09-08).
            Vector3 popupPos = transform.position + Vector3.up * 0.4f;
            switch (_payload.Kind)
            {
                case FairyPayloadKind.Gems:
                    SaveDataManager.Instance?.AddGems(MonetizationConfig.GEM_PACK_VALUE);
                    FloatingText.Spawn(popupPos, MonetizationConfig.GEM_PACK_VALUE.ToString(),
                        UIStyles.HUD_TEXT_FILL, UIStyles.WORLD_FLOATING_TEXT_SIZE,
                        "ui_popup_plate_mult", "ui_gem");
                    break;
                case FairyPayloadKind.Stars:
                    // Via GameManager so the stars count toward the run total on the game-over panel.
                    GameManager.Instance?.AwardStars(MonetizationConfig.STAR_PACK_VALUE);
                    FloatingText.Spawn(popupPos, MonetizationConfig.STAR_PACK_VALUE.ToString(),
                        UIStyles.HUD_TEXT_FILL, UIStyles.WORLD_FLOATING_TEXT_SIZE,
                        "ui_popup_plate_mult", "ui_star");
                    break;
                default:
                    ConsumableInventory.Instance?.Add(_payload.Consumable);
                    FloatingText.Spawn(popupPos, "1", UIStyles.HUD_TEXT_FILL,
                        UIStyles.WORLD_FLOATING_TEXT_SIZE, "ui_popup_plate_mult",
                        "ui_consumable_" + _payload.Consumable.ToString().ToLowerInvariant());
                    break;
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
