namespace DogtorBurguer
{
    /// <summary>
    /// Stateless registry mapping a <see cref="ConsumableType"/> to its effect (the effects hold no
    /// per-use state, so one shared instance each — no per-call allocation). Order matches the enum.
    /// </summary>
    public static class ConsumableEffects
    {
        private static readonly ConsumableEffect[] _byType =
        {
            new KetchupEffect(), // ConsumableType.Ketchup = 0
            new MustardEffect(), // ConsumableType.Mustard = 1
            new SkewerEffect(),  // ConsumableType.Skewer  = 2
        };

        public static ConsumableEffect For(ConsumableType type) => _byType[(int)type];
    }
}
