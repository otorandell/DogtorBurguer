using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Outcome of a single processed match: where to play the effect and whether
    /// it was a bun cancellation (no points) rather than an ingredient match.
    /// </summary>
    public struct MatchResult
    {
        public Vector3 EffectPosition;
        public bool IsBunMatch;
    }
}
