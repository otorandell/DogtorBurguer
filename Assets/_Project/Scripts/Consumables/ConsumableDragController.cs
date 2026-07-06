using DG.Tweening;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// The carry interaction: a press that starts on an inventory slot lifts that consumable, which
    /// then follows the finger with a translucent ghost clamped to the nearest column. Releasing over
    /// the playfield drops the faller (and consumes the slot); releasing elsewhere cancels. Driven by
    /// TouchInputHandler (the single raw-input reader), which suppresses chef gestures while carrying.
    /// The world keeps moving throughout — deliberately no pause.
    /// </summary>
    public class ConsumableDragController : Singleton<ConsumableDragController>
    {
        private bool _carrying;
        private ConsumableType _type;

        private SpriteRenderer _carryIcon;
        private SpriteRenderer _ghost;

        public bool IsCarrying => _carrying;

        /// <summary>Starts a carry if the press landed on a stocked slot. Returns true if it did. Takes a
        /// SCREEN position (slots are screen-space UGUI); the carry/drop then work in world space.</summary>
        public bool TryBegin(Vector2 screenPos)
        {
            if (_carrying) return false;

            ConsumableInventoryView view = ConsumableInventoryView.Instance;
            if (view == null || !view.TryGetSlotTypeAt(screenPos, out ConsumableType type))
                return false;

            StartCarry(type);
            return true;
        }

        public void UpdateCarry(Vector3 worldPos)
        {
            if (!_carrying) return;

            _carryIcon.transform.position = worldPos;

            Column col = GridManager.Instance?.GetColumn(NearestColumn(worldPos.x));
            if (col != null)
            {
                _ghost.enabled = true;
                _ghost.transform.position = col.GetWorldPositionForRow(Constants.MAX_ROWS)
                    + Vector3.up * UIStyles.CONSUMABLE_GHOST_Y_OFFSET;
            }
        }

        public void Release(Vector3 worldPos)
        {
            if (!_carrying) return;

            if (IsOverPlayfield(worldPos))
            {
                Column col = GridManager.Instance?.GetColumn(NearestColumn(worldPos.x));

                // For linger effects the ghost survives the release as the "locked on" nozzle:
                // it holds over the column while the effect plays, then fades out.
                if (_ghost != null && ConsumableEffects.For(_type).GhostLingers)
                {
                    SpriteRenderer ghost = _ghost;
                    _ghost = null; // EndCarry leaves it alone
                    DOTween.Sequence().SetLink(ghost.gameObject)
                        .AppendInterval(AnimConfig.GHOST_LINGER_DURATION)
                        .Append(ghost.DOFade(0f, AnimConfig.FX_FADE_DURATION))
                        .OnComplete(() => { if (ghost != null) Destroy(ghost.gameObject); });
                }

                ConsumableFaller.Spawn(_type, col);
                ConsumableInventory.Instance?.TryConsume(_type); // OnChanged → view refresh
            }
            else
            {
                // Cancel: restore the source slot icon (no OnChanged fires — nothing was consumed).
                ConsumableInventoryView.Instance?.SetTypeHidden(_type, false);
            }

            EndCarry();
        }

        public void Cancel()
        {
            if (!_carrying) return;
            ConsumableInventoryView.Instance?.SetTypeHidden(_type, false);
            EndCarry();
        }

        private void StartCarry(ConsumableType type)
        {
            _carrying = true;
            _type = type;

            ConsumableInventoryView.Instance?.SetTypeHidden(type, true);

            // The held item stays the badge; the column ghost is the effect's targeting art
            // (ketchup/mustard nozzles — the visual that "locks onto" the column).
            _carryIcon = CreateRenderer("CarryIcon", RewardArt.Badge(type),
                Constants.SORT_CONSUMABLE_CARRY, 1f, UIStyles.CONSUMABLE_CARRY_HEIGHT);
            _ghost = CreateRenderer("ColumnGhost", ConsumableEffects.For(type).GhostSprite,
                Constants.SORT_CONSUMABLE_GHOST, UIStyles.CONSUMABLE_GHOST_ALPHA, UIStyles.CONSUMABLE_GHOST_HEIGHT);
            _ghost.enabled = false; // revealed once a column is targeted
        }

        private static SpriteRenderer CreateRenderer(string name, Sprite sprite, int sortingOrder, float alpha, float height)
        {
            GameObject obj = new GameObject(name);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = sortingOrder;
            sr.color = new Color(1f, 1f, 1f, alpha);
            SpriteFit.Height(sr, height);
            return sr;
        }

        private void EndCarry()
        {
            _carrying = false;
            if (_carryIcon != null) Destroy(_carryIcon.gameObject);
            if (_ghost != null) Destroy(_ghost.gameObject);
            _carryIcon = null;
            _ghost = null;
        }

        private static int NearestColumn(float worldX)
        {
            int col = Mathf.RoundToInt((worldX - Constants.GRID_ORIGIN_X) / Constants.CELL_WIDTH);
            return Mathf.Clamp(col, 0, Constants.COLUMN_COUNT - 1);
        }

        private static bool IsOverPlayfield(Vector3 worldPos)
        {
            float left = Constants.GRID_ORIGIN_X - Constants.CELL_WIDTH * 0.5f;
            float right = Constants.GRID_ORIGIN_X + (Constants.COLUMN_COUNT - 0.5f) * Constants.CELL_WIDTH;
            float bottom = Constants.GRID_ORIGIN_Y;
            float top = Constants.GRID_ORIGIN_Y + Constants.MAX_ROWS * Constants.CELL_VISUAL_HEIGHT;
            return worldPos.x >= left && worldPos.x <= right && worldPos.y >= bottom && worldPos.y <= top;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            EndCarry();
        }
    }
}
