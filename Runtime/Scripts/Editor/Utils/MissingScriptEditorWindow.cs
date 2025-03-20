using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DaftAppleGames.Editor.Utils
{
    public class MissingScriptWindow : EditorWindow
    {
        [SerializeField]
        private List<string> missingAssets = new List<string>();

        [SerializeField]
        private List<GameObject> objectsWithMissingScriptsInCurrentScene = new List<GameObject>();

        [MenuItem("Daft Apple Games/Tools/Find missing scripts")]
        public static void ShowWindow()
        {
            GetWindow(typeof(MissingScriptWindow));
        }

        private void OnGUI()
        {
            GUILayout.Label("Find Missing Scripts", EditorStyles.boldLabel);
            GUILayout.Space(10);

            var myBoxStyle = new GUIStyle(GUI.skin.box);

            // Scene block
            GUILayout.BeginVertical(myBoxStyle);
            if (GUILayout.Button("Find in current scene"))
            {
                FindMissingScriptsInCurrentScene();
            }

            if (GUILayout.Button("Delete in current scene"))
            {
                DeleteMissingScriptsInCurrentScene();
            }

            GUILayout.Label("Results (Open Scenes):", EditorStyles.boldLabel);

            for (var i = objectsWithMissingScriptsInCurrentScene.Count - 1; i >= 0; i--)
            {
                if (!objectsWithMissingScriptsInCurrentScene[i])
                {
                    objectsWithMissingScriptsInCurrentScene.RemoveAt(i);
                }
            }

            foreach (var go in objectsWithMissingScriptsInCurrentScene)
            {
                if (GUILayout.Button(go.name))
                {
                    EditorGUIUtility.PingObject(go);
                }
            }

            GUILayout.EndVertical();

            GUILayout.Space(20);

            // Assets block
            GUILayout.BeginVertical(myBoxStyle);
            if (GUILayout.Button("Find in assets"))
            {
                FindMissingScriptsInAssets();
            }

            GUILayout.Label("Results (Assets):", EditorStyles.boldLabel);
            foreach (var path in missingAssets)
            {
                if (GUILayout.Button(path))
                {
                    EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
                }
            }

            GUILayout.EndVertical();
        }

        private void FindMissingScriptsInCurrentScene()
        {
            objectsWithMissingScriptsInCurrentScene.Clear();
            var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            foreach (var go in allObjects)
            {
                if (go.transform.parent == null) // Only start with root objects
                {
                    FindMissingScriptsInGameObjectAndChildren(go);
                }
            }
        }

        private void FindMissingScriptsInGameObjectAndChildren(GameObject go)
        {
            var components = go.GetComponents<Component>();
            var hasMissingScript = components.Any(c => c == null);
            if (hasMissingScript)
            {
                objectsWithMissingScriptsInCurrentScene.Add(go);
            }

            foreach (Transform child in go.transform) // Recursively check children
            {
                FindMissingScriptsInGameObjectAndChildren(child.gameObject);
            }
        }

        private void FindMissingScriptsInAssets()
        {
            missingAssets.Clear();
            var allAssets = AssetDatabase.GetAllAssetPaths();
            foreach (var assetPath in allAssets)
            {
                if (assetPath.StartsWith("Packages/"))
                {
                    continue;
                }

                if (Path.GetExtension(assetPath) == ".prefab")
                {
                    var assetRoot = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                    if (assetRoot == null)
                    {
                        continue;
                    }
                    var components = assetRoot.GetComponentsInChildren<Component>(true);
                    var hasMissingScript = components.Any(c => c == null);
                    if (hasMissingScript)
                    {
                        missingAssets.Add(assetPath);
                    }
                }
            }
        }

        private void DeleteMissingScriptsInCurrentScene()
        {
            FindMissingScriptsInCurrentScene();
            foreach (GameObject go in objectsWithMissingScriptsInCurrentScene)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                Debug.Log($"Deleted missing script from: {go.name}");
                if (go.hideFlags.HasFlag(HideFlags.HideInHierarchy))
                {
                    Debug.Log("Revealing hidden GameObject " + go.name, go);
                    go.hideFlags &= ~HideFlags.HideInHierarchy;
                }
            }
        }

        private void DeleteMissingScriptsInAssets()
        {
            FindMissingScriptsInCurrentScene();
            foreach (string path in missingAssets)
            {
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(asset);
                UnityEditor.EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
            }
        }
    }
}