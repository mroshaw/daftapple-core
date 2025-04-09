using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DaftAppleGames.Editor
{
    public class MissingScriptsEditorWindow : BaseEditorWindow
    {
        [SerializeField] private List<string> missingAssetNames = new();
        [SerializeField] private List<GameObject> objectsWithMissingScriptsInAssets = new();
        [SerializeField] private List<GameObject> objectsWithMissingScriptsInScenes = new();

        private Button _deleteMissingScriptsInAssetsButton;
        private Button _deleteMissingScriptsInSceneButton;
        private Button _findMissingScriptsInAssetsButton;
        private Button _findMissingScriptsInSceneButton;

        private ListView _objectsInSceneListView;
        private ListView _assetObjectsListView;

        /// <summary>
        ///     Custom editor overrides
        /// </summary>
        protected override string ToolTitle => "Missing Scripts Tool";

        protected override string WelcomeLogText =>
            "Welcome to the Missing Script Tool! Click one of the buttons above to manage missing scripts in your scene and prefab assets.\n" +
            "WARNING: Finding missing scripts in assets can take several minutes!";

        [MenuItem("Daft Apple Games/Tools/Missing Script Editor")]
        public static void ShowWindow()
        {
            MissingScriptsEditorWindow editorWindow = GetWindow<MissingScriptsEditorWindow>();
            editorWindow.titleContent = new GUIContent("Missing Scripts Tool");
        }

        #region UI creation

        /// <summary>
        ///     Override the base CreateGUI to add in the specific editor content
        /// </summary>
        public override void CreateGUI()
        {
            base.CreateGUI();

            // Register buttons
            _findMissingScriptsInSceneButton = rootVisualElement.Q<Button>("FindMissingScriptsInSceneButton");
            if (_findMissingScriptsInSceneButton != null)
            {
                _findMissingScriptsInSceneButton.clicked -= FindInScene;
                _findMissingScriptsInSceneButton.clicked += FindInScene;
            }

            _deleteMissingScriptsInSceneButton = rootVisualElement.Q<Button>("DeleteMissingScriptsInSceneButton");
            if (_deleteMissingScriptsInSceneButton != null)
            {
                _deleteMissingScriptsInSceneButton.clicked -= FindAndDeleteInScene;
                _deleteMissingScriptsInSceneButton.clicked += FindAndDeleteInScene;
            }

            _findMissingScriptsInAssetsButton = rootVisualElement.Q<Button>("FindMissingScriptsInAssetsButton");
            if (_findMissingScriptsInAssetsButton != null)
            {
                _findMissingScriptsInAssetsButton.clicked -= FindInAssets;
                _findMissingScriptsInAssetsButton.clicked += FindInAssets;
            }

            _deleteMissingScriptsInAssetsButton = rootVisualElement.Q<Button>("DeleteMissingScriptsInAssetsButton");
            if (_deleteMissingScriptsInAssetsButton != null)
            {
                _deleteMissingScriptsInAssetsButton.clicked -= FindAndDeleteInAssets;
                _deleteMissingScriptsInAssetsButton.clicked += FindAndDeleteInAssets;
            }

            // Configure the object list views
            _objectsInSceneListView = rootVisualElement.Q<ListView>("SceneGameObjectsListView");
            ConfigureListView(_objectsInSceneListView);
            _assetObjectsListView = rootVisualElement.Q<ListView>("AssetsObjectsListView");
            ConfigureListView(_assetObjectsListView);

            // Bind UI controls
            BindUI();
        }

        private static void ConfigureListView(ListView listView)
        {
            listView.showBoundCollectionSize = false;
            listView.SetEnabled(false);
        }

        #endregion

        /// <summary>
        ///     Button handler for Find Scripts in scene
        /// </summary>
        private void FindInScene()
        {
            Log.Log(LogLevel.Info, "Searching for missing scripts in open scenes...");
            FindMissingScriptsInOpenScenes(false);
        }

        /// <summary>
        ///     Button handler for Find and Delete Scripts in scene
        /// </summary>
        private void FindAndDeleteInScene()
        {
            Log.Log(LogLevel.Info, "Searching for and deleting missing scripts in open scenes...");
            FindMissingScriptsInOpenScenes(true);
        }

        /// <summary>
        ///     Button handler for Find Scripts in assets
        /// </summary>
        private void FindInAssets()
        {
            Log.Log(LogLevel.Info, "Searching for missing scripts in assets...");
            FindMissingScriptsInAssets(false);
        }

        /// <summary>
        ///     Button handler for Find and Delete Scripts in assets
        /// </summary>
        private void FindAndDeleteInAssets()
        {
            Log.Log(LogLevel.Info, "Searching for and deleting missing scripts in assets...");
            FindMissingScriptsInAssets(true);
        }

        /// <summary>
        ///     Find game objects in open scenes for missing scripts
        /// </summary>
        private void FindMissingScriptsInOpenScenes(bool deleteScripts)
        {
            objectsWithMissingScriptsInScenes.Clear();
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (GameObject go in allObjects)
            {
                if (go.transform.parent == null) // Only start with root objects
                {
                    FindMissingScriptsInGameObjectAndChildren(go);
                }
            }

            Log.Log(LogLevel.Info, $"Found {objectsWithMissingScriptsInScenes.Count} missing script(s) in open scenes.");

            if (deleteScripts)
            {
                DeleteMissingScripts(objectsWithMissingScriptsInScenes);
            }

            _objectsInSceneListView?.RefreshItems();
        }

        /// <summary>
        ///     Search through entire game object structure for missing scripts
        /// </summary>
        private void FindMissingScriptsInGameObjectAndChildren(GameObject parentGameObject)
        {
            Component[] components = parentGameObject.GetComponents<Component>();
            bool hasMissingScript = components.Any(c => c == null);
            if (hasMissingScript)
            {
                Log.Log(LogLevel.Info, $"Found missing script on: {parentGameObject.name}");
                objectsWithMissingScriptsInScenes.Add(parentGameObject);
            }

            foreach (Transform child in parentGameObject.transform) // Recursively check children
            {
                FindMissingScriptsInGameObjectAndChildren(child.gameObject);
            }
        }

        /// <summary>
        ///     Search assets for objects with missing scripts
        /// </summary>
        private void FindMissingScriptsInAssets(bool deleteScripts)
        {
            missingAssetNames.Clear();
            objectsWithMissingScriptsInAssets.Clear();

            string[] allAssets = AssetDatabase.GetAllAssetPaths();
            foreach (string assetPath in allAssets)
            {
                if (assetPath.StartsWith("Packages/"))
                {
                    continue;
                }

                if (Path.GetExtension(assetPath) != ".prefab")
                {
                    continue;
                }

                GameObject assetRoot = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (assetRoot == null)
                {
                    continue;
                }

                Component[] components = assetRoot.GetComponentsInChildren<Component>(true);
                bool hasMissingScript = components.Any(c => c == null);
                if (!hasMissingScript)
                {
                    continue;
                }

                Log.Log(LogLevel.Info, $"Found missing script on: {assetRoot.name}");
                missingAssetNames.Add(assetPath);
                objectsWithMissingScriptsInAssets.Add(assetRoot);
            }

            Log.Log(LogLevel.Info, $"Found {objectsWithMissingScriptsInAssets.Count} missing script(s) in assets.");

            if (deleteScripts)
            {
                DeleteMissingScripts(objectsWithMissingScriptsInAssets);
            }

            _assetObjectsListView?.RefreshItems();
        }

        private void DeleteMissingScripts(List<GameObject> gameObjectsWithMissingScripts)
        {
            Log.Log(LogLevel.Info, "Deleting missing scripts...");
            foreach (GameObject go in gameObjectsWithMissingScripts)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                Log.Log($"Deleted missing script from: {go.name}");
                if (!go.hideFlags.HasFlag(HideFlags.HideInHierarchy))
                {
                    continue;
                }

                Log.Log($"Revealing hidden GameObject: {go.name}");
                go.hideFlags &= ~HideFlags.HideInHierarchy;
            }

            Log.Log(LogLevel.Info, "Deleting missing scripts... Done.");
        }
    }
}