#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace DaftAppleGames.Scenes
{
    public class SceneLoaderEvents : MonoBehaviour
    {
        [BoxGroup("Events")] public UnityEvent allScenesLoadedEvent;
        [BoxGroup("Events")] public UnityEvent faderStartedEvent;
        [BoxGroup("Events")] public UnityEvent faderFinishedEvent;
        private void Start()
        {
            SceneLoaderManager loaderManager = FindAnyObjectByType<SceneLoaderManager>();
            if (loaderManager)
            {
                loaderManager.allScenesLoadedEvent.AddListener(AllScenesLoadedProxy);
                loaderManager.faderStartedEvent.AddListener(FaderStartedProxy);
                loaderManager.faderFinishedEvent.AddListener(FaderFinishedProxy);
            }
        }

        private void AllScenesLoadedProxy()
        {
            allScenesLoadedEvent.Invoke();
        }

        private void FaderStartedProxy()
        {
            faderStartedEvent.Invoke();
        }

        private void FaderFinishedProxy()
        {
            faderFinishedEvent.Invoke();
        }
    }
}