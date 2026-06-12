using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Shared geometry for the inventory slots — the single source both the view (renders them) and
    /// the drag controller (hit-tests them) read from. Positions live in UIStyles. Sized for the
    /// 2-slot inventory (<see cref="ConsumableInventory.CAPACITY"/>).
    /// </summary>
    public static class ConsumableSlotLayout
    {
        public const float TapRadius = 0.55f;

        public static Vector3 Position(int index) => index == 0
            ? (Vector3)UIStyles.CONSUMABLE_SLOT_0_POS
            : (Vector3)UIStyles.CONSUMABLE_SLOT_1_POS;
    }
}
