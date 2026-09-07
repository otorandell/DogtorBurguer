namespace DogtorBurguer
{
    /// <summary>
    /// Where one finger press stands in TouchInputHandler's gesture resolution.
    /// </summary>
    public enum PressPhase
    {
        /// <summary>No press in progress.</summary>
        None,
        /// <summary>Pressed; the lift may still resolve as a tap or a swipe (Drag mode, and a
        /// consumable carry).</summary>
        Open,
        /// <summary>The tap intent was already resolved on the press (Tap mode); only a swipe
        /// can still fire before the lift.</summary>
        SwipeOnly
    }
}
