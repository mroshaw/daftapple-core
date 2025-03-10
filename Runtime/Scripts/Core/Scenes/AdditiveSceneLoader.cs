using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#if GPU_INSTANCER

#endif

#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.Scenes
{
    public class AdditiveSceneLoader : MonoBehaviour
    {
        [BoxGroup("Behaviour")] [SerializeField] private bool loadScenesOnStart = true;
        [BoxGroup("Behaviour")] [SerializeField] private bool loadScenesOnAwake;
        [BoxGroup("Behaviour")] [SerializeField] private bool showProgress = true;
        [BoxGroup("Behaviour")] [SerializeField] private GameObject progressPanel;
        [BoxGroup("Behaviour")] [SerializeField] private ProgressSlider progressSlider;
        [BoxGroup("Scenes")] [SerializeField] private AdditiveSceneLoaderSettings additiveSceneLoaderSettings;

        [FoldoutGroup("Events")] public UnityEvent loadStartedEvent;
        [FoldoutGroup("Events")] public UnityEvent scenesLoadedEvent;
        [FoldoutGroup("Events")] public UnityEvent scenesActivatedEvent;
        [FoldoutGroup("Events")] public UnityEvent<float> loadProgressUpdatedEvent;

        public SceneLoadStatus SceneLoadStatus => _sceneLoadStatus;
        private SceneLoadStatus _sceneLoadStatus;

        private void OnEnable()
        {
            progressPanel.SetActive(true);
        }

        private void Awake()
        {
            _sceneLoadStatus = SceneLoadStatus.Idle;

            if (loadScenesOnAwake)
            {
                LoadAndActivateAllScenes();
            }
        }

        private void Start()
        {
            if (loadScenesOnStart)
            {
                LoadAndActivateAllScenes();
            }
        }

        /// <summary>
        /// Monitor progress and update progress bar while loading
        /// </summary>
        private void Update()
        {
            if (_sceneLoadStatus == SceneLoadStatus.Idle)
            {
                return;
            }

            UpdateProgress();
        }

        private IEnumerator LoadAllScenesAsync()
        {
            ShowProgress();
            loadStartedEvent?.Invoke();
            _sceneLoadStatus = SceneLoadStatus.Loading;

            // Load scenes
            LoadScenes(LoadSceneMode.Additive);

            // Wait for all scenes to load
            while (!AllScenesInState(SceneLoadStatus.Loaded))
            {
                yield return null;
            }

            scenesLoadedEvent?.Invoke();
            _sceneLoadStatus = SceneLoadStatus.Activating;

            HideProgress();

            // Activate scenes
            ActivateScenes();

            // Wait for all scenes to activate
            while (!AllScenesInState(SceneLoadStatus.Activated))
            {
                yield return null;
            }

            // Done!
            yield return new WaitForSeconds(1);
            scenesActivatedEvent?.Invoke();
        }

        /// <summary>
        /// True if all scenes are in the given state, otherwise false
        /// </summary>
        private bool AllScenesInState(SceneLoadStatus status)
        {
            foreach (AdditiveScene additiveScene in additiveSceneLoaderSettings.additiveScenes)
            {
                if (additiveScene.LoadStatus != status)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Iterate and load each Additive scene asynchronously
        /// </summary>
        private void LoadScenes(LoadSceneMode loadSceneMode)
        {
            foreach (AdditiveScene additiveScene in additiveSceneLoaderSettings.additiveScenes)
            {
#if UNITY_EDITOR
                Debug.Log($"Starting Editor load of scene: {additiveScene.sceneName}...");
                Scene currentScene = EditorSceneManager.GetSceneByName(additiveScene.sceneName);
                if (!currentScene.IsValid())
                {
                    if (!Application.isPlaying)
                    {
                        EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(additiveScene.sceneAsset), OpenSceneMode.Additive);
                        additiveScene.LoadStatus = SceneLoadStatus.Loaded;
                    }
                    else
                    {
                        StartCoroutine(LoadSceneAsync(additiveScene, loadSceneMode));
                    }
                }
                else
                {
                    Debug.Log($"Already Loaded: {additiveScene.sceneName}");
                    additiveScene.LoadStatus = SceneLoadStatus.Loaded;
                }
#else
                StartCoroutine(LoadSceneAsync(additiveScene, loadSceneMode));
#endif
            }
        }

        /// <summary>
        /// Iterate and activate all scenes
        /// </summary>
        private void ActivateScenes()
        {
            foreach (AdditiveScene additiveScene in additiveSceneLoaderSettings.additiveScenes)
            {
                StartCoroutine(ActivateSceneAsync(additiveScene));
            }
        }

        /// <summary>
        /// Load the given scene async, and remove from list once loaded. Do not activate
        /// </summary>
        private IEnumerator LoadSceneAsync(AdditiveScene additiveScene, LoadSceneMode loadSceneMode)
        {
            additiveScene.LoadStatus = SceneLoadStatus.Loading;

            // Wait until next frame
            yield return null;

            Debug.Log($"Async Load Scene: {additiveScene.sceneName}");
            //Begin to load the Scene
#if UNITY_EDITOR
            AsyncOperation asyncOperation =
                EditorSceneManager.LoadSceneAsync(additiveScene.sceneName, loadSceneMode);
            asyncOperation.allowSceneActivation = false;

#else
            AsyncOperation asyncOperation =
                SceneManager.LoadSceneAsync(additiveScene.sceneName, loadSceneMode);
            asyncOperation.allowSceneActivation = false;
#endif
            additiveScene.SceneOp = asyncOperation;

            // When the load is still in progress, output the Text and progress bar
            while (asyncOperation.progress < 0.9f)
            {
                yield return null;
            }

            additiveScene.LoadStatus = SceneLoadStatus.Loaded;
            Debug.Log($"Async Load Scene DONE: {additiveScene.sceneName}");
            yield return null;
        }

        private IEnumerator ActivateSceneAsync(AdditiveScene additiveScene)
        {
            // Wait until next frame
            yield return null;
            additiveScene.LoadStatus = SceneLoadStatus.Activating;
            Debug.Log($"Async Activate Scene: {additiveScene.sceneName}");

            if (additiveScene.SceneOp != null)
            {
                // Activate the scene
                additiveScene.SceneOp.allowSceneActivation = true;

                // Wait for scene to fully activate
                while (!additiveScene.SceneOp.isDone)
                {
                    yield return null;
                }
            }

            additiveScene.LoadStatus = SceneLoadStatus.Activated;
            scenesActivatedEvent?.Invoke();
            Debug.Log($"Async Activate Scene DONE: {additiveScene.sceneName}");

            if (additiveScene.isMain)
            {
                Debug.Log($"Setting Main Scene: {additiveScene.sceneName}");
#if UNITY_EDITOR
                EditorSceneManager.SetActiveScene(SceneManager.GetSceneByName(additiveScene.sceneName));
#else
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(additiveScene.sceneName));
#endif
            }
        }

        private void UpdateProgress()
        {
            float totalProgress = 0.0f;
            foreach (AdditiveScene additiveScene in additiveSceneLoaderSettings.additiveScenes)
            {
                if (additiveScene.SceneOp != null)
                {
                    totalProgress += additiveScene.SceneOp.progress;
                }
            }

            if (showProgress)
            {
                progressSlider.SetProgress(totalProgress);
            }
            loadProgressUpdatedEvent?.Invoke(totalProgress);
        }

        /// <summary>
        /// Init the scene load status
        /// </summary>
        private void InitLoadStatus()
        {
            foreach (AdditiveScene additiveScene in additiveSceneLoaderSettings.additiveScenes)
            {
                additiveScene.LoadStatus = SceneLoadStatus.Idle;
                additiveScene.SceneOp = null;
            }
        }

        /// <summary>
        /// Top level process orchestration
        /// </summary>
        [Button("Load Scenes")]
        public void LoadAndActivateAllScenes()
        {
            InitLoadStatus();
            Debug.Log($"Total Additive Scenes:{additiveSceneLoaderSettings.additiveScenes.Count}");

            StartCoroutine(LoadAllScenesAsync());
        }

        private void ShowProgress()
        {
            if (showProgress)
            {
                progressPanel.SetActive(true);
            }
            else
            {
                progressPanel.SetActive(false);
            }
        }

        private void HideProgress()
        {
            progressPanel.SetActive(false);
        }

#if UNITY_EDITOR
        [Button("Unload Scenes")]
        public void UnloadScenes()
        {
            int sceneLoadedCount = EditorSceneManager.sceneCount;
            Scene[] loadedScenes = new Scene[sceneLoadedCount];

            for (int currSceneIndex = 0; currSceneIndex < sceneLoadedCount; currSceneIndex++)
            {
                loadedScenes[currSceneIndex] = EditorSceneManager.GetSceneAt(currSceneIndex);
            }

            foreach (Scene currScene in loadedScenes)
            {
                if (currScene.name != gameObject.scene.name)
                {
                    Debug.Log($"Closing... {currScene.name}");
                    EditorSceneManager.CloseScene(currScene, true);
                }
            }
        }
#endif
    }
}