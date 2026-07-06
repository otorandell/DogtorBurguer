using UnityEngine;

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

        /// <summary>What visually falls when this consumable is used. Defaults to the reward
        /// badge; effects with authored drop art override (mustard drop, full skewer).
        /// Null → nothing falls and the effect resolves instantly on release (ketchup).</summary>
        public virtual Sprite FallerSprite => RewardArt.Badge(Type);

        /// <summary>True → the column ghost survives the release, lingering as the "locked on"
        /// nozzle over the column while the effect plays, then fades (ketchup/mustard).</summary>
        public virtual bool GhostLingers => false;

        /// <summary>The translucent column-targeting ghost shown while carrying. Defaults to the
        /// reward badge; ketchup/mustard show their nozzle (it "locks onto" the column).</summary>
        public virtual Sprite GhostSprite => RewardArt.Badge(Type);

        /// <summary>World height of the falling visual.</summary>
        public virtual float FallerHeight => UIStyles.CONSUMABLE_FALLER_HEIGHT;

        /// <summary>Raises the faller's end position (long art like the skewer lands center-high
        /// so its tip, not its middle, meets the impact row).</summary>
        public virtual float FallerImpactLift => 0f;

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
