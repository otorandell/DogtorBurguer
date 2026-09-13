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
        // Statics survive scene loads AND (with domain reload disabled) editor play sessions —
        // a tutorial abandoned mid-run (quit to menu, editor stop) must never leak its gates
        // into the next run: reset everything at every play start.
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Pending = false;
            End();
        }

        public static bool Pending;
        public static bool IsActive { get; private set; }

        public static bool ShouldRun =>
            Pending || (SaveDataManager.Instance != null && !SaveDataManager.Instance.TutorialSeen);

        // The power-up steps' free item: while set, ConsumableInventory shows one of that type
        // and using it never touches the persistent stock. Each of the three steps grants its
        // own (was Ketchup-only until 2026-09-13), so a wasted drop just means trying again.
        public static ConsumableType? VirtualItem;

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
            VirtualItem = null;
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
