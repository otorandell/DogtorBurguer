using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>Ketchup — drops onto the stack top and removes the whole targeted column.</summary>
    public sealed class KetchupEffect : ConsumableEffect
    {
        public override ConsumableType Type => ConsumableType.Ketchup;
        public override int ImpactRow(Column column) => column.StackHeight;
        public override bool CanApply(Column column) => column != null && !column.IsEmpty;
        public override Sprite GhostSprite => RewardArt.KetchupNozzle;
        // Nothing falls: the lingering ghost nozzle squirts the stream on release (instant resolve).
        public override Sprite FallerSprite => null;
        public override bool GhostLingers => true;
        public override void Apply(Column column) => GridManager.Instance?.ConsumableClearColumn(column);
        public override void PlayVfx(Column column) => ConsumableVfx.KetchupSquirt(column);
    }
}
