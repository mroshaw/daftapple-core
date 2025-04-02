using UnityEngine;
using UnityEditor;

namespace DaftAppleGames.Editor.Utils
{
    /// <summary>
    /// This forces the Editor to select the parent game object, rather than the
    /// LOD mesh game object, when selecting a multi-LOD object in the scene view
    /// Can be enabled/disabled
    /// </summary>
    [InitializeOnLoad]
    public class AlwaysSelectParentInHierarchy
    {
        private static bool _isEnabled;
        private static bool _debugMessages;

        static AlwaysSelectParentInHierarchy()
        {
            Selection.selectionChanged += OnSelectionChanged;
            Log("SelectionHandler initialized.");
        }

        static void OnSelectionChanged()
        {
            if (!_isEnabled)
                return;

            Log("Selection changed.");

            if (!MouseIsOverAnySceneView())
            {
                Log("Mouse is not over any SceneView.");
                return;
            }

            GameObject[] selectedObjects = Selection.gameObjects;

            for (int goIndex = 0; goIndex < selectedObjects.Length; goIndex++)
            {
                GameObject currGameObject = selectedObjects[goIndex];
                if (currGameObject == null)
                {
                    continue;
                }

                Transform parentWithLOD = currGameObject.transform;

                while (parentWithLOD != null)
                {
                    if (parentWithLOD.GetComponent<LODGroup>() != null)
                    {
                        Log("Found parent with LOD. Scheduling a selection change.");
                        GameObject newSelection = parentWithLOD.gameObject;
                        int index = goIndex;
                        EditorApplication.delayCall += () =>
                        {
                            selectedObjects[index] = newSelection;
                            Selection.objects = selectedObjects;
                            Log("Selection changed to parent with LOD.");
                        };
                        break;
                    }

                    parentWithLOD = parentWithLOD.parent;
                }
            }

            GameObject selectedObject = Selection.activeGameObject;

            if (selectedObject == null)
                return;

            Log("Checking for parent with LOD.");
        }

        /// <summary>
        /// Check to see if user is selecting something in the scene view
        /// </summary>
        private static bool MouseIsOverAnySceneView()
        {
            foreach (SceneView view in SceneView.sceneViews)
            {
                if (view == EditorWindow.mouseOverWindow)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Outputs log if logging enabled
        /// </summary>
        private static void Log(string message)
        {
            if (_debugMessages)
            {
                Debug.Log(message);
            }
        }

        [MenuItem("Daft Apple Games/Tools/Toggle LOD Parent Selection")]
        public static void ToggleFunctionality()
        {
            _isEnabled = !_isEnabled;
            Log("LOD Parent Selection is " + (_isEnabled ? "enabled" : "disabled"));
        }

        [MenuItem("Daft Apple Games/Tools/Toggle LOD Debug")]
        public static void ToggleDebugMessages()
        {
            _debugMessages = !_debugMessages;
            Debug.Log("LOD Debug Messages are " + (_debugMessages ? "enabled" : "disabled"));
        }
    }
}