using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Persistence for the in-progress run (<see cref="RunSnapshot"/>), plus the in-memory
    /// handoff that survives the menu -> Game scene load.
    ///
    /// Separate from SaveDataManager on purpose: that owns the player PROFILE (currency, high
    /// score, settings) which is read constantly and lives as individual keys. This owns one
    /// throwaway blob describing a single run — different lifetime, different failure cost. A
    /// corrupt profile matters; a corrupt snapshot costs one run and is simply dropped.
    /// </summary>
    public static class RunSnapshotStore
    {
        private const string KEY_RUN = "runSnapshot";

        /// <summary>The snapshot the Game scene is booting into, or null for a fresh run. Set by
        /// the menu's RESUME button before the scene load; systems that initialize BEFORE
        /// GameManager.Start (DifficultyManager at execution order -100, IngredientSpawner.Awake)
        /// read it here, everything else is handed the snapshot directly. GameManager clears it
        /// at the end of its Start, once those early readers have run — otherwise an in-game
        /// Restart, which is a scene reload, would restore the old board again.</summary>
        public static RunSnapshot Pending { get; private set; }

        public static void SetPending(RunSnapshot snapshot) => Pending = snapshot;

        public static void ClearPending() => Pending = null;

        public static void Save(RunSnapshot snapshot)
        {
            if (snapshot == null) return;

            PlayerPrefs.SetString(KEY_RUN, JsonUtility.ToJson(snapshot));
            PlayerPrefs.Save();
        }

        /// <summary>The saved run, or null when there is none, it is unreadable, or it was
        /// written by a different schema version. Every failure path returns null: a resume is
        /// a convenience, so dropping a doubtful one is always better than restoring a wrong
        /// board.</summary>
        public static RunSnapshot Load()
        {
            string json = PlayerPrefs.GetString(KEY_RUN, string.Empty);
            if (string.IsNullOrEmpty(json)) return null;

            RunSnapshot snapshot;
            try
            {
                snapshot = JsonUtility.FromJson<RunSnapshot>(json);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[RunSnapshot] Unreadable saved run, discarding: {e.Message}");
                Clear();
                return null;
            }

            if (snapshot == null || snapshot.Version != RunSnapshot.CURRENT_VERSION
                || snapshot.Columns == null || snapshot.RosterOrder == null)
            {
                Clear();
                return null;
            }

            return snapshot;
        }

        public static void Clear()
        {
            PlayerPrefs.DeleteKey(KEY_RUN);
            PlayerPrefs.Save();
        }
    }
}
