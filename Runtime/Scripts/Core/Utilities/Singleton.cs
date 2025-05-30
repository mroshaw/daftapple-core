using UnityEngine;

namespace DaftAppleGames.Utilities
{
    [DefaultExecutionOrder(-500)]
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object _instanceLock = new object();
        private static bool _quitting = false;

        public static T Instance
        {
            get
            {
                lock (_instanceLock)
                {
                    if (_instance == null && !_quitting)
                    {

                        _instance = FindFirstObjectByType<T>(FindObjectsInactive.Include);
                        if (_instance == null)
                        {
                            GameObject go = new(typeof(T).ToString());
                            _instance = go.AddComponent<T>();
                            DontDestroyOnLoad(_instance.gameObject);
                            /*
                            if (doNotDestroyOnLoad)
                            {
                                DontDestroyOnLoad(_instance.gameObject);
                            }
                            */
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = gameObject.GetComponent<T>();
            }
            else if (_instance.GetInstanceID() != GetInstanceID())
            {
                Destroy(gameObject);
                throw new System.Exception(string.Format("Instance of {0} already exists, removing {1}", GetType().FullName, ToString()));
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _quitting = true;
        }

    }
}