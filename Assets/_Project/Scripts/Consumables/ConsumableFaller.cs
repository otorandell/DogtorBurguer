using UnityEngine;
using DG.Tweening;

namespace DogtorBurguer
{
    /// <summary>
    /// The shared consumable "faller": spawns at the top of the target column, drops fast, and
    /// resolves its effect on impact (fizzles harmlessly if there's no target). Spawned by the drag
    /// controller when a consumable is released over a column.
    /// </summary>
    public class ConsumableFaller : MonoBehaviour
    {
        public static void Spawn(ConsumableType type, Column column)
        {
            if (column == null) return;
            GameObject obj = new GameObject("ConsumableFaller");
            obj.AddComponent<ConsumableFaller>().Begin(type, column);
        }

        private void Begin(ConsumableType type, Column column)
        {
            ConsumableEffect effect = ConsumableEffects.For(type);

            SpriteRenderer sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = effect.FallerSprite;
            sr.sortingOrder = Constants.SORT_CONSUMABLE_FALLER;
            SpriteFit.Height(sr, effect.FallerHeight);

            int impactRow = effect.ImpactRow(column);
            transform.position = column.GetWorldPositionForRow(Constants.MAX_ROWS);
            Vector3 end = column.GetWorldPositionForRow(impactRow) + Vector3.up * effect.FallerImpactLift;

            float cells = Mathf.Max(1f, Constants.MAX_ROWS - impactRow);
            float duration = cells * GameplayConfig.CONSUMABLE_FALL_STEP_DURATION;

            transform.DOMove(end, duration).SetEase(Ease.InQuad)
                .OnComplete(() => Resolve(effect, column));
        }

        private void Resolve(ConsumableEffect effect, Column column)
        {
            // Resolve against live state at impact (robust to a piece landing mid-drop).
            if (effect.CanApply(column))
            {
                effect.PlayVfx(column); // cosmetic overlay; the grid resolves underneath it
                effect.Apply(column);
                AudioManager.Instance?.PlayConsumableUse(effect.Type);
            }
            else
            {
                AudioManager.Instance?.PlayConsumableFizzle();
            }

            transform.DOScale(Vector3.zero, AnimConfig.FLASH_SCALE_OUT_DURATION).SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    if (this != null && gameObject != null)
                        Destroy(gameObject);
                });
        }

        private void OnDestroy() => transform.DOKill();
    }
}
