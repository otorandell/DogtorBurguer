namespace DogtorBurguer
{
    /// <summary>
    /// Cross-scene tutorial flags. <see cref="Pending"/> is the explicit request (the How to Play
    /// PLAY TUTORIAL button); <see cref="ShouldRun"/> also fires on a fresh save (first ever Play).
    /// While <see cref="IsActive"/>, the systems the tutorial scripts stand down: no auto waves or
    /// previews (IngredientSpawner), no fairies, no difficulty progression, no auto orders, no
    /// star persistence — and input is masked per step via the Allow* switches below.
    /// </summary>
    public static class TutorialMode
    {
        public static bool Pending;
        public static bool IsActive { get; private set; }

        public static bool ShouldRun =>
            Pending || (SaveDataManager.Instance != null && !SaveDataManager.Instance.TutorialSeen);

        // The PowerUp step's free Ketchup: while true, ConsumableInventory shows at least one
        // Ketchup and using it never touches the persistent stock.
        public static bool VirtualKetchup;

        // Per-step input mask (all true outside the tutorial). Set by TutorialManager.
        public static bool AllowMove = true;
        public static bool AllowFlip = true;
        public static bool AllowFastDrop = true;
        public static bool AllowConsumable = true;

        public static void Begin()
        {
            Pending = false;
            IsActive = true;
            SetMask(false, false, false, false);
        }

        public static void End()
        {
            IsActive = false;
            VirtualKetchup = false;
            SetMask(true, true, true, true);
        }

        public static void SetMask(bool move, bool flip, bool fastDrop, bool consumable)
        {
            AllowMove = move;
            AllowFlip = flip;
            AllowFastDrop = fastDrop;
            AllowConsumable = consumable;
        }
    }
}
