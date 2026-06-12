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
        private int _slotIndex;
        private ConsumableType _type;

        private SpriteRenderer _carryIcon;
        private SpriteRenderer _ghost;

        public bool IsCarrying => _carrying;

        /// <summary>Starts a carry if the press landed on an occupied slot. Returns true if it did.</summary>
        public bool TryBegin(Vector3 worldPos)
        {
            ConsumableInventory inv = ConsumableInventory.Instance;
            if (_carrying || inv == null) return false;

            for (int i = 0; i < inv.Count; i++)
            {
                if (Vector2.Distance(worldPos, ConsumableSlotLayout.Position(i)) <= ConsumableSlotLayout.TapRadius)
                {
                    StartCarry(i, inv[i]);
                    UpdateCarry(worldPos);
                    return true;
                }
            }
            return false;
        }

        public void UpdateCarry(Vector3 worldPos)
        {
            if (!_carrying) return;

            _carryIcon.transform.position = worldPos;

            Column col = GridManager.Instance?.GetColumn(NearestColumn(worldPos.x));
            if (col != null)
            {
                _ghost.enabled = true;
                _ghost.transform.position = col.GetWorldPositionForRow(Constants.MAX_ROWS);
            }
        }

        public void Release(Vector3 worldPos)
        {
            if (!_carrying) return;

            if (IsOverPlayfield(worldPos))
            {
                Column col = GridManager.Instance?.GetColumn(NearestColumn(worldPos.x));
                ConsumableFaller.Spawn(_type, col);
                ConsumableInventory.Instance?.ConsumeAt(_slotIndex); // OnChanged → view refresh
            }
            else
            {
                // Cancel: restore the source slot (no OnChanged fires since nothing was consumed).
                ConsumableInventoryView.Instance?.SetSlotHidden(_slotIndex, false);
            }

            EndCarry();
        }

        public void Cancel()
        {
            if (!_carrying) return;
            ConsumableInventoryView.Instance?.SetSlotHidden(_slotIndex, false);
            EndCarry();
        }

        private void StartCarry(int slotIndex, ConsumableType type)
        {
            _carrying = true;
            _slotIndex = slotIndex;
            _type = type;

            ConsumableInventoryView.Instance?.SetSlotHidden(slotIndex, true);

            _carryIcon = CreateRenderer("CarryIcon", Constants.SORT_CONSUMABLE_CARRY, 1f, UIStyles.CONSUMABLE_CARRY_HEIGHT);
            _ghost = CreateRenderer("ColumnGhost", Constants.SORT_CONSUMABLE_GHOST, UIStyles.CONSUMABLE_GHOST_ALPHA, UIStyles.CONSUMABLE_GHOST_HEIGHT);
            _ghost.enabled = false; // revealed once a column is targeted
        }

        private SpriteRenderer CreateRenderer(string name, int sortingOrder, float alpha, float height)
        {
            GameObject obj = new GameObject(name);
            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = RewardArt.Badge(_type);
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
