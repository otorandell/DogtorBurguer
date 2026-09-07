using System;

namespace DogtorBurguer
{
    /// <summary>
    /// Consumable inventory: a quantity per consumable type (three fixed slots — Ketchup, Mustard,
    /// Skewer). Collecting increments the type; using decrements it. The stock is PERSISTENT
    /// (<see cref="SaveDataManager"/>) — fairy drops and shop purchases feed the same pool and carry
    /// across runs. This class is the gameplay-facing facade: it drives the HUD slots and is the
    /// source the drag controller consumes from.
    /// </summary>
    public class ConsumableInventory : Singleton<ConsumableInventory>
    {
        public const int TypeCount = 3; // Ketchup, Mustard, Skewer — one slot each, indexed by enum

        /// <summary>Fired whenever any count changes (collect / purchase / use).</summary>
        public event Action OnChanged;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;

            if (SaveDataManager.Instance != null)
                SaveDataManager.Instance.OnConsumablesChanged += RaiseChanged;
        }

        protected override void OnDestroy()
        {
            if (Instance == this && SaveDataManager.Instance != null)
                SaveDataManager.Instance.OnConsumablesChanged -= RaiseChanged;
            base.OnDestroy();
        }

        private void RaiseChanged() => OnChanged?.Invoke();

        /// <summary>Tutorial: refreshes the slot views after toggling the virtual Ketchup.</summary>
        public void NotifyChanged() => RaiseChanged();

        public int CountOf(ConsumableType type)
        {
            // The tutorial's free Ketchup: always visible, never depletes (see TryConsume).
            if (TutorialMode.VirtualKetchup && type == ConsumableType.Ketchup) return 1;
            return SaveDataManager.Instance != null ? SaveDataManager.Instance.ConsumableCount(type) : 0;
        }

        /// <summary>Adds one of a consumable (no cap).</summary>
        public void Add(ConsumableType type) =>
            SaveDataManager.Instance?.AddConsumables(type, 1);

        /// <summary>Uses one if available. Returns false (no-op) when that slot is empty.</summary>
        public bool TryConsume(ConsumableType type)
        {
            // The tutorial's free Ketchup is spent without touching the persistent stock.
            if (TutorialMode.VirtualKetchup && type == ConsumableType.Ketchup)
            {
                RaiseChanged();
                return true;
            }
            return SaveDataManager.Instance != null && SaveDataManager.Instance.TryConsumeConsumable(type);
        }
    }
}
