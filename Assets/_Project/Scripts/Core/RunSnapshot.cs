using System;

namespace DogtorBurguer
{
    /// <summary>
    /// A whole in-progress run, flattened for storage — written when Android backgrounds the
    /// app (it kills the process to reclaim memory, so a run would otherwise be lost) and when
    /// the player leaves via Quit to Menu. Restored by <see cref="RunSnapshotService"/>.
    ///
    /// Structure only: no inline defaults, because <see cref="RunSnapshotService.Capture"/> is
    /// the one place that fills this in and it populates every field. A half-written snapshot
    /// must fail loudly, not restore a plausible-but-wrong board.
    ///
    /// Fields are public because Unity's JsonUtility only serializes public (or [SerializeField])
    /// fields — the project's no-public-fields rule targets inspector-facing MonoBehaviours.
    ///
    /// What is deliberately NOT captured: falling pieces, the preview queue and the shuffle bag.
    /// A restored run starts from a settled board with a fresh wave — the couple of in-flight
    /// pieces simply vanish, which reads as a small gift rather than a penalty, and it keeps the
    /// snapshot to things that cannot be recomputed.
    /// </summary>
    [Serializable]
    public class RunSnapshot
    {
        /// <summary>Bumped whenever the fields below change meaning. A snapshot from another
        /// version is discarded rather than guessed at — it only ever costs one run.</summary>
        public const int CURRENT_VERSION = 1;

        public int Version;

        // --- GameManager run totals ---
        public int Score;
        public int StarsEarnedThisRun;
        public int StarsPaidFromScore;

        // --- DifficultyManager progression ---
        public int Level;
        public int IngredientsPlaced;

        // --- IngredientSpawner: this run's random unlock order (IngredientRoster) ---
        public IngredientType[] RosterOrder;

        // --- The board: one entry per column, bottom row first ---
        public RunSnapshotColumn[] Columns;

        // --- ChefController ---
        public int ChefPosition;
        public bool ChefFlipped;

        // --- BurgerChallenge (Special Orders) ---
        public int ChallengeLevel;
        public int ChallengeProgress;
        public int ChallengeRequiredSize;
        public IngredientType[] ChallengeTargets;

        // --- GameOverPanel: one continue per run, and a resume must not hand out a second ---
        public bool HasContinued;
    }
}
