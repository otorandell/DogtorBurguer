using System.Collections.Generic;
using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Maps a live run to a <see cref="RunSnapshot"/> and back. The one place that knows which
    /// systems hold run state, so <see cref="RunSnapshotStore"/> stays pure persistence and
    /// GameManager keeps its own job.
    ///
    /// Restoring is split by WHO ALREADY SEEDS THE STATE, not by timing. A system that picks a
    /// starting value for itself at init — DifficultyManager (level), IngredientSpawner (this
    /// run's roster), ChefController (position + facing), BurgerChallenge (the current order) —
    /// reads RunSnapshotStore.Pending right there, because the snapshot is simply another input
    /// to a seeding decision it already makes (the same shape as the TutorialMode gates). Pushing
    /// into them from here would also be a race: Start order between them is undefined, so their
    /// own seeding could overwrite a pushed value.
    ///
    /// <see cref="Apply"/> therefore covers only what nothing else writes: the run totals, the
    /// board itself, and the continue-used flag.
    /// </summary>
    public static class RunSnapshotService
    {
        /// <summary>Reads the live run into a snapshot. Callers must only invoke this while a
        /// run is actually in progress — see GameManager.SaveResumePoint for the guards.</summary>
        public static RunSnapshot Capture()
        {
            GameManager game = GameManager.Instance;
            DifficultyManager difficulty = game != null ? game.Difficulty : null;
            IngredientSpawner spawner = game != null ? game.Spawner : null;
            ChefController chef = Object.FindAnyObjectByType<ChefController>();
            BurgerChallenge challenge = BurgerChallenge.Instance;
            GameOverPanel gameOver = Object.FindAnyObjectByType<GameOverPanel>();

            RunSnapshot snapshot = new RunSnapshot
            {
                Version = RunSnapshot.CURRENT_VERSION,

                Score = game != null ? game.Score : 0,
                StarsEarnedThisRun = game != null ? game.StarsEarnedThisRun : 0,
                StarsPaidFromScore = game != null ? game.StarsPaidFromScore : 0,

                Level = difficulty != null ? difficulty.CurrentLevel : 1,
                IngredientsPlaced = difficulty != null ? difficulty.IngredientsPlaced : 0,

                RosterOrder = spawner != null ? spawner.RosterOrder : System.Array.Empty<IngredientType>(),
                Columns = CaptureColumns(),

                ChefPosition = chef != null ? chef.CurrentPosition : Constants.CHEF_START_POSITION,
                ChefFlipped = chef != null && chef.IsFlipped,

                ChallengeLevel = challenge != null ? challenge.Level : 1,
                ChallengeProgress = challenge != null ? challenge.Progress : 0,
                ChallengeRequiredSize = challenge != null ? challenge.RequiredSize : 0,
                ChallengeTargets = CaptureChallengeTargets(challenge),

                HasContinued = gameOver != null && gameOver.HasContinued,
            };

            return snapshot;
        }

        /// <summary>Rebuilds the run described by <paramref name="snapshot"/>. Called from
        /// GameManager.Start, after EnsureManagers, so every system below exists.</summary>
        public static void Apply(RunSnapshot snapshot)
        {
            if (snapshot == null) return;

            GameManager game = GameManager.Instance;
            game?.RestoreRunTotals(snapshot.Score, snapshot.StarsEarnedThisRun, snapshot.StarsPaidFromScore);

            RestoreColumns(snapshot.Columns, game != null ? game.Spawner : null);

            GameOverPanel gameOver = Object.FindAnyObjectByType<GameOverPanel>();
            gameOver?.RestoreContinued(snapshot.HasContinued);
        }

        private static RunSnapshotColumn[] CaptureColumns()
        {
            RunSnapshotColumn[] columns = new RunSnapshotColumn[Constants.COLUMN_COUNT];
            for (int c = 0; c < columns.Length; c++)
            {
                Column column = GridManager.Instance?.GetColumn(c);
                List<Ingredient> pieces = column != null ? column.GetAllIngredients() : new List<Ingredient>();

                // Bottom row first — Column.AddIngredient appends, so replaying this order
                // rebuilds the stack exactly, rows and sorting orders included.
                List<IngredientType> types = new List<IngredientType>(pieces.Count);
                foreach (Ingredient piece in pieces)
                {
                    if (piece != null) types.Add(piece.Type);
                }

                columns[c] = new RunSnapshotColumn { Types = types.ToArray() };
            }
            return columns;
        }

        private static void RestoreColumns(RunSnapshotColumn[] columns, IngredientSpawner spawner)
        {
            if (columns == null || spawner == null || GridManager.Instance == null) return;

            for (int c = 0; c < columns.Length && c < Constants.COLUMN_COUNT; c++)
            {
                Column column = GridManager.Instance.GetColumn(c);
                if (column == null || columns[c] == null || columns[c].Types == null) continue;

                foreach (IngredientType type in columns[c].Types)
                    spawner.SpawnRestored(type, column);
            }
        }

        private static IngredientType[] CaptureChallengeTargets(BurgerChallenge challenge)
        {
            if (challenge == null) return System.Array.Empty<IngredientType>();

            IReadOnlyList<IngredientType> targets = challenge.TargetIngredients;
            IngredientType[] copy = new IngredientType[targets.Count];
            for (int i = 0; i < copy.Length; i++) copy[i] = targets[i];
            return copy;
        }
    }
}
