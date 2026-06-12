using System;
using System.Collections.Generic;

namespace DogtorBurguer
{
    /// <summary>
    /// Per-run consumable inventory: a FIFO ring of <see cref="CAPACITY"/> slots. Collecting while
    /// full evicts the oldest. Not persisted — lost on game over. Drives the HUD slots and is the
    /// source the drag controller consumes from.
    /// </summary>
    public class ConsumableInventory : Singleton<ConsumableInventory>
    {
        public const int CAPACITY = 2;

        private readonly List<ConsumableType> _slots = new List<ConsumableType>(CAPACITY);

        /// <summary>Fired whenever the slot contents change (add / consume).</summary>
        public event Action OnChanged;

        public int Count => _slots.Count;

        public ConsumableType this[int index] => _slots[index];

        /// <summary>Adds a consumable; evicts the oldest if both slots are already full.</summary>
        public void Add(ConsumableType type)
        {
            if (_slots.Count >= CAPACITY)
                _slots.RemoveAt(0);
            _slots.Add(type);
            OnChanged?.Invoke();
        }

        /// <summary>Removes the consumable at a slot once it has been used. No-op if out of range.</summary>
        public void ConsumeAt(int index)
        {
            if (index < 0 || index >= _slots.Count) return;
            _slots.RemoveAt(index);
            OnChanged?.Invoke();
        }
    }
}
