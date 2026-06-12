using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Renders the per-run consumable inventory as world-space icons in the top-left score panel.
    /// Driven by <see cref="ConsumableInventory.OnChanged"/>. The drag controller hides a slot while
    /// its item is being carried (see <see cref="SetSlotHidden"/>).
    /// </summary>
    public class ConsumableInventoryView : Singleton<ConsumableInventoryView>
    {
        private SpriteRenderer[] _slots;

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
            _slots = new SpriteRenderer[ConsumableInventory.CAPACITY];
            for (int i = 0; i < _slots.Length; i++)
            {
                GameObject obj = new GameObject($"ConsumableSlot_{i}");
                obj.transform.SetParent(transform, false);
                obj.transform.position = ConsumableSlotLayout.Position(i);

                SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = Constants.SORT_CONSUMABLE_SLOT;
                sr.enabled = false;
                _slots[i] = sr;
            }
        }

        private void Refresh()
        {
            if (_slots == null) return;

            ConsumableInventory inv = ConsumableInventory.Instance;
            int count = inv != null ? inv.Count : 0;

            for (int i = 0; i < _slots.Length; i++)
            {
                if (i < count)
                {
                    _slots[i].sprite = RewardArt.Badge(inv[i]);
                    SpriteFit.Height(_slots[i], UIStyles.CONSUMABLE_ICON_HEIGHT);
                    _slots[i].enabled = true;
                }
                else
                {
                    _slots[i].enabled = false;
                }
            }
        }

        /// <summary>Hide/show a single slot icon while its item is carried.</summary>
        public void SetSlotHidden(int index, bool hidden)
        {
            if (_slots == null || index < 0 || index >= _slots.Length) return;
            _slots[index].enabled = !hidden;
        }
    }
}
