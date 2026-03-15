using UnityEngine;

namespace Singletons
{
    /// <summary>
    /// Base singleton that persists across scenes by default.
    /// Override PersistAcrossScenes to false for scene-specific singletons.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _lock = new object();

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = FindFirstObjectByType<T>();
                        }
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (PersistAcrossScenes)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// Override this to make the singleton scene-specific (not persist across scene loads).
        /// </summary>
        protected virtual bool PersistAcrossScenes => true;
    }
}
