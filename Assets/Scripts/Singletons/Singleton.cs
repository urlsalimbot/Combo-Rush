using UnityEngine;
using UnityEngine.SceneManagement;

namespace Singletons
{
    /// <summary>
    /// Base singleton that persists across scenes by default.
    /// Override PersistAcrossScenes to false for scene-specific singletons.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        /// <summary>
        /// Override this to make the singleton scene-specific (not persist across scene loads).
        /// </summary>
        protected virtual bool PersistAcrossScenes => true;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<T>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject(typeof(T).Name);
                        _instance = go.AddComponent<T>();
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
    }
}
