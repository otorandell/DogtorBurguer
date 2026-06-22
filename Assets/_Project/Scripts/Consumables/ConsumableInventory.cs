using System;

namespace DogtorBurguer
{
    /// <summary>
    /// Per-run consumable inventory: a quantity per consumable type (three fixed slots — Ketchup,
    /// Mustard, Skewer). Collecting increments the type; using decrements it. Not persisted — lost on
    /// game over. Drives the HUD slots and is the source the drag controller consumes from.
    /// </summary>
    public class ConsumableInventory : Singleton<ConsumableInventory>
    {
        public const int TypeCount = 3; // Ketchup, Mustard, Skewer — one slot each, indexed by enum

        private readonly int[] _counts = new int[TypeCount];

        /// <summary>Fired whenever any count changes (collect / use).</summary>
        public event Action OnChanged;

        public int CountOf(ConsumableType type) => _counts[(int)type];

        /// <summary>Adds one of a consumable (no cap).</summary>
        public void Add(ConsumableType type)
        {
            _counts[(int)type]++;
            OnChanged?.Invoke();
        }

        /// <summary>Uses one if available. Returns false (no-op) when that slot is empty.</summary>
        public bool TryConsume(ConsumableType type)
        {
            int i = (int)type;
            if (_counts[i] <= 0) return false;
            _counts[i]--;
            OnChanged?.Invoke();
            return true;
        }
    }
}
