using UnityEngine;

namespace DogtorBurguer
{
    /// <summary>Shared MonoBehaviour helpers.</summary>
    public static class MonoBehaviourUtil
    {
        /// <summary>Spawns a GameObject hosting a <typeparamref name="T"/> if none exists yet (F-1).</summary>
        public static void EnsureComponent<T>() where T : MonoBehaviour
        {
            if (Object.FindAnyObjectByType<T>() == null)
            {
                GameObject obj = new GameObject(typeof(T).Name);
                obj.AddComponent<T>();
            }
        }
    }
}
