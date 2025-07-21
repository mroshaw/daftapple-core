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
using GPUInstancer;
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
        [FoldoutGroup("Events")] public UnityEvent<string> sceneLoadedEvent;
        [FoldoutGroup("Events")] public UnityEvent<string> sceneActivatedEvent;
        [FoldoutGroup("Events")] public UnityEvent allScenesLoadedEvent;
        [FoldoutGroup("Events")] public UnityEvent allScenesActivatedEvent;
        [FoldoutGroup("Events")] public UnityEvent everythingReadyEvent;

        [FoldoutGroup("Events")] public UnityEvent<float> loadProgressUpdatedEvent;

        public SceneLoadStatus SceneLoadStatus => _sceneLoadStatus;
        private SceneLoadStatus _sceneLoadStatus;

        private bool _scenesReady;

#if GPU_INSTANCER
        private bool _gpuInstancerDetailReady;
#endif

        private void OnEnable()
        {
            progressPanel.SetActive(true);
        }

        private void Awake()
        {
#if GPU_INSTANCER
            _gpuInstancerDetailReady = true;

            GPUInstancerAPI.StartListeningGPUIEvent(GPUInstancerEventType.DetailInitializationFinished, GPUInstancerDetailManagerReady);
#endif
            _sceneLoadStatus = SceneLoadStatus.Idle;

            if (loadScenesOnAwake)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    LoadScenesInEditor();
                }
                else
                {
                    LoadAndActivateAllScenes();
                }
#else
                LoadAndActivateAllScenes();
#endif
            }
        }

        private void Start()
        {
            if (loadScenesOnStart)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    LoadScenesInEditor();
                }
                else
                {
                    LoadAndActivateAllScenes();
                }
#else
                LoadAndActivateAllScenes();
#endif
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

        private void CheckEverythingReady()
        {
            if (IsEverythingReady())
            {
                Debug.Log("Everything ready!");
                everythingReadyEvent?.Invoke();
            }
        }

        private bool IsEverythingReady()
        {
#if GPU_INSTANCER
            return _scenesReady && IsGPUInstancerReady();
#else
            return _scenesReady;
#endif
        }

#if GPU_INSTANCER
        private void GPUInstancerDetailManagerReady()
        {
            Debug.Log("GPU Instancer Detail Manager ready!");
            GPUInstancerAPI.StopListeningGPUIEvent(GPUInstancerEventType.DetailInitializationFinished, GPUInstancerDetailManagerReady);
            _gpuInstancerDetailReady = true;
            CheckEverythingReady();
        }

        private bool IsGPUInstancerReady()
        {
            return _gpuInstancerDetailReady;
        }
#endif
        private IEnumerator LoadAndActivateAllScenesAsync()
        {
#if UNITY_EDITOR
            UnloadScenesInEditor();
#endif

            ShowProgress();
            loadStartedEvent?.Invoke();
            _sceneLoadStatus = SceneLoadStatus.Loading;

            // Load scenes
            yield return LoadScenesAsync(LoadSceneMode.Additive);
            Debug.Log("AdditiveSceneLoader: All scenes loaded");

            // Activate scenes
            yield return ActivateScenesAsync();
            Debug.Log("AdditiveSceneLoader: All scenes activated");

            HideProgress();

            // Done!
            yield return new WaitForSeconds(1);
            Debug.Log("AdditiveSceneLoader: Done");
            _scenesReady = true;
            CheckEverythingReady();
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

#if UNITY_EDITOR
        [Button("Unload Scenes")]
        private void UnloadScenesInEditor()
        {
            int countLoaded = SceneManager.sceneCount;
            Scene[] loadedScenes = new Scene[countLoaded];

            // Get array of active scenes
            for (int i = 0; i < countLoaded; i++)
            {
                loadedScenes[i] = SceneManager.GetSceneAt(i);
            }

            // Close all but the open one
            for (int i = countLoaded - 1; i >= 0; i--)
            {
                Scene currScene = loadedScenes[i];
                if (currScene.name != additiveSceneLoaderSettings.loaderScene.sceneName)
                {
                    EditorSceneManager.UnloadSceneAsync(currScene);
                }
            }
        }
#endif

#if UNITY_EDITOR
        [Button("Load Scenes")]
        private void LoadScenesInEditor()
        {
            foreach (AdditiveScene additiveScene in additiveSceneLoaderSettings.additiveScenes)
            {
                Debug.Log($"Starting Editor load of scene: {additiveScene.sceneName}...");
                Scene currentScene = EditorSceneManager.GetSceneByName(additiveScene.sceneName);
                if (!currentScene.IsValid())
                {
                    EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(additiveScene.sceneAsset), OpenSceneMode.Additive);
                    additiveScene.LoadStatus = SceneLoadStatus.Loaded;
                }
                else
                {
                    Debug.Log($"Already Loaded: {additiveScene.sceneName}");
                    additiveScene.LoadStatus = SceneLoadStatus.Loaded;
                }

                if (additiveScene.isMain)
                {
                    EditorSceneManager.SetActiveScene(SceneManager.GetSceneByName(additiveScene.sceneName));
                }
            }
        }
#endif

        private IEnumerator LoadScenesAsync(LoadSceneMode loadSceneMode)
        {
            _sceneLoadStatus = SceneLoadStatus.Loading;
            foreach (AdditiveScene additiveScene in additiveSceneLoaderSettings.additiveScenes)
            {
                yield return LoadSceneAsync(additiveScene, loadSceneMode);
            }

            allScenesLoadedEvent?.Invoke();
        }

        private IEnumerator ActivateScenesAsync()
        {
            _sceneLoadStatus = SceneLoadStatus.Activating;
            foreach (AdditiveScene additiveScene in additiveSceneLoaderSettings.additiveScenes)
            {
                yield return ActivateSceneAsync(additiveScene);
            }

            allScenesActivatedEvent?.Invoke();
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

            AsyncOperation asyncOperation =
                SceneManager.LoadSceneAsync(additiveScene.sceneName, loadSceneMode);
            asyncOperation.allowSceneActivation = false;
            additiveScene.SceneOp = asyncOperation;

            // When the load is still in progress, output the Text and progress bar
            while (asyncOperation.progress < 0.9f)
            {
                yield return null;
            }

            additiveScene.LoadStatus = SceneLoadStatus.Loaded;
            sceneLoadedEvent?.Invoke(additiveScene.sceneName);
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
            sceneActivatedEvent?.Invoke(additiveScene.sceneName);
            Debug.Log($"Async Activate Scene DONE: {additiveScene.sceneName}");

            if (additiveScene.isMain)
            {
                Debug.Log($"Setting Main Scene: {additiveScene.sceneName}");
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(additiveScene.sceneName));
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

        private void LoadAndActivateAllScenes()
        {
            InitLoadStatus();
            Debug.Log($"Total Additive Scenes:{additiveSceneLoaderSettings.additiveScenes.Count}");

            StartCoroutine(LoadAndActivateAllScenesAsync());
        }

        private void ShowProgress()
        {
            progressPanel.SetActive(showProgress);
        }

        private void HideProgress()
        {
            progressPanel.SetActive(false);
        }
    }
}