using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>
    /// Base for the project's manager singletons. Claims <see cref="Instance"/> in
    /// Awake (destroying duplicates) and clears it in OnDestroy so no stale reference
    /// survives a scene reload.
    ///
    /// Deliberately does NOT auto-create a missing instance or call
    /// DontDestroyOnLoad — the documented manager init order is driven by scene/
    /// bootstrap setup, and only some managers persist. Persistent managers call
    /// DontDestroyOnLoad themselves in their Awake override.
    ///
    /// Derived Awake/OnDestroy must chain to base:
    /// <code>
    /// protected override void Awake()
    /// {
    ///     base.Awake();
    ///     if (Instance != this) return; // lost the duplicate guard; skip setup
    ///     // ... setup ...
    /// }
    /// </code>
    /// A derived class with no OnDestroy inherits the base one automatically.
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = (T)this;
        }

        protected virtual void OnDestroy()
        {
            // Only the live instance clears the reference — a destroyed duplicate
            // (Instance still points at the original) must leave it intact.
            if (Instance == this)
                Instance = null;
        }
    }
}
