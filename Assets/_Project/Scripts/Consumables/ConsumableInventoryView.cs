using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Renders the persistent consumable inventory as a screen-space row of three slots below the
    /// Level/Score panels (one per type — Ketchup, Mustard, Skewer). Driven by
    /// <see cref="ConsumableInventory.OnChanged"/>. The drag controller hit-tests slots via
    /// <see cref="TryGetSlotTypeAt"/> and hides a slot's icon while its item is carried.
    /// </summary>
    public class ConsumableInventoryView : Singleton<ConsumableInventoryView>
    {
        private Canvas _canvas;
        private ConsumableSlotWidget[] _slots;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;
            BuildSlots();
        }

        private void Start()
        {
            if (ConsumableInventory.Instance != null)
                ConsumableInventory.Instance.OnChanged += Refresh;
            Refresh();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (ConsumableInventory.Instance != null)
                ConsumableInventory.Instance.OnChanged -= Refresh;
        }

        private void BuildSlots()
        {
            // Screen Space - Camera (like the HUD) so fairies (sort 100) fly over the slots.
            _canvas = UIFactory.CreateCanvas(transform, "ConsumableCanvas", Constants.SORT_CONSUMABLE_SLOT, Camera.main);
            _slots = new ConsumableSlotWidget[ConsumableInventory.TypeCount];
            for (int i = 0; i < _slots.Length; i++)
            {
                Vector2 pos = new(
                    UIStyles.CONSUMABLE_SLOT_X_START + i * UIStyles.CONSUMABLE_SLOT_SPACING,
                    UIStyles.CONSUMABLE_ROW_Y);
                _slots[i] = new ConsumableSlotWidget(_canvas.transform, _canvas.worldCamera, (ConsumableType)i, pos);
            }
        }

        private void Refresh()
        {
            if (_slots == null) return;
            ConsumableInventory inv = ConsumableInventory.Instance;
            for (int i = 0; i < _slots.Length; i++)
                _slots[i].Refresh(inv != null ? inv.CountOf((ConsumableType)i) : 0);
        }

        /// <summary>If the screen point is on a stocked slot, returns its type (used to begin a carry).</summary>
        public bool TryGetSlotTypeAt(Vector2 screenPos, out ConsumableType type)
        {
            type = default;
            if (_slots == null) return false;

            ConsumableInventory inv = ConsumableInventory.Instance;
            foreach (ConsumableSlotWidget slot in _slots)
            {
                if (slot.Contains(screenPos) && inv != null && inv.CountOf(slot.Type) > 0)
                {
                    type = slot.Type;
                    return true;
                }
            }
            return false;
        }

        /// <summary>Hide/show a type's icon while its item is carried.</summary>
        public void SetTypeHidden(ConsumableType type, bool hidden)
        {
            if (_slots == null) return;
            _slots[(int)type].SetIconHidden(hidden);
        }
    }
}
