namespace DogtorBurguer
{
    /// <summary>The tutorial state machine's steps, in play order.</summary>
    public enum TutorialStep
    {
        Move,     // swipe to move the chef
        Swap,     // tap the chef to swap the two stacks
        Match,    // route a falling twin onto its pair (loops until it lands right)
        Burger,   // watch a scripted burger get built (flip disabled)
        Order,    // complete a scripted Special Order -> mult level-up showcase
        PowerUp,  // drag the granted Ketchup onto the junk column (re-granted on a miss)
        Ready,    // closing message
        Done,
    }
}
