namespace DogtorBurguer
{
    /// <summary>The tutorial state machine's steps, in play order.</summary>
    public enum TutorialStep
    {
        Move,     // swipe / side-tap to move the chef
        Swap,     // tap the chef to swap the two stacks
        Match,    // route falling twins onto their pairs (several rounds, loops until each lands right)
        FastDrop, // tap a falling ingredient to speed it up
        Burger,   // build a scripted burger (flip disabled)
        Order,    // complete a scripted Special Order -> mult level-up showcase
        Ketchup,  // clear a column
        Mustard,  // sweep the top two types board-wide
        Skewer,   // dig a buried bottom bun down to the floor
        Ready,    // closing message
        Done,
    }
}
