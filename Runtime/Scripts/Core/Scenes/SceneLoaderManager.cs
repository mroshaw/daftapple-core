using System.Collections;
using DaftAppleGames.Utilities;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace DaftAppleGames.Scenes
{
    public enum SceneLoadStatus
    {
        Idle,
        Loading,
        Loaded,
        Activating,
        Activated
    }

    /// <summary>
    /// MonoBehaviour class to manage the additive / incremental loading of many scenes
    /// </summary>
    public class SceneLoaderManager : Singleton<SceneLoaderManager>
    {
        /// <summary>
        /// Internal class for metadata related to each scene
        /// </summary>
        [BoxGroup("Behaviour")] [SerializeField] private SceneFader sceneFader;

        [BoxGroup("Scenes")] [SerializeField] private string mainMenuLoaderScene;
        [BoxGroup("Scenes")] [SerializeField] private string gameLoaderScene;

        [FoldoutGroup("Scene Events")] public UnityEvent allScenesLoadStartedEvent;
        [FoldoutGroup("Scene Events")] public UnityEvent sceneLoadedEvent;
        [FoldoutGroup("Scene Events")] public UnityEvent sceneActivatedEvent;
        [FoldoutGroup("Scene Events")] public UnityEvent allScenesLoadedEvent;
        [FoldoutGroup("Scene Events")] public UnityEvent allScenesActivatedEvent;
        [FoldoutGroup("Scene Events")] public UnityEvent thisSceneLoadedEvent;
        [FoldoutGroup("Fader Events")] public UnityEvent faderStartedEvent;
        [FoldoutGroup("Fader Events")] public UnityEvent faderFinishedEvent;

        internal SceneLoadStatus SceneLoadStatus => _sceneLoadStatus;
        private SceneLoadStatus _sceneLoadStatus;

        private bool _isLoading;

        private void ThisSceneLoadedHandler(Scene scene, LoadSceneMode mode)
        {
            thisSceneLoadedEvent?.Invoke();
        }

#if UNITY_EDITOR
        private void EditorSceneLoadedHandler(Scene scene, LoadSceneMode _)
        {
            Debug.Log($"AdditiveSceneManager: Editor Scene Loaded detected: {scene.name}");
        }
#endif


        public void LoadMainMenuLoaderScene(bool fadeOut = true)
        {
            StartCoroutine(LoadSingleSceneAsync(mainMenuLoaderScene, true));
        }

        public void LoadGameLoaderScene(bool fadeOut = true)
        {
            StartCoroutine(LoadSingleSceneAsync(gameLoaderScene, true));
        }

        private IEnumerator LoadSingleSceneAsync(string sceneName, bool fadeOut = true)
        {
            if (fadeOut)
            {
                sceneFader.FadeOut();
                while (sceneFader.FadeState != FadeState.FadeComplete)
                {
                    yield return null;
                }
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        public void FaderStartedProxy()
        {
            faderStartedEvent.Invoke();
        }

        public void FaderFinishedProxy()
        {
            faderFinishedEvent.Invoke();
        }
    }
}