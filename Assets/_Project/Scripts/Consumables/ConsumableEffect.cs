namespace DogtorBurguer
{
    /// <summary>
    /// Base for the three consumables. The shared <see cref="ConsumableFaller"/> drives the fall;
    /// each subclass supplies its target rule (<see cref="ImpactRow"/> / <see cref="CanApply"/>)
    /// and its on-impact behavior (<see cref="Apply"/>). Behavior lives here — polymorphic, no
    /// switch. Effects are stateless singletons (see <see cref="ConsumableEffects"/>).
    /// </summary>
    public abstract class ConsumableEffect
    {
        public abstract ConsumableType Type { get; }

        /// <summary>Row the faller visually drops to (its impact point) for this column.</summary>
        public abstract int ImpactRow(Column column);

        /// <summary>
        /// False → the faller reaches the floor and fizzles with no effect (the item is still spent).
        /// </summary>
        public abstract bool CanApply(Column column);

        /// <summary>Resolve the effect on the targeted column. Only called when CanApply is true.</summary>
        public abstract void Apply(Column column);

        /// <summary>Cosmetic use effect, fired alongside Apply (see ConsumableVfx). Effects with
        /// no authored art yet keep the empty default.</summary>
        public virtual void PlayVfx(Column column) { }
    }
}
