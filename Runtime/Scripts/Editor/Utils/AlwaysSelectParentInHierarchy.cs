using UnityEngine;
using UnityEditor;
using UnityEditor.TerrainTools;

namespace DaftAppleGames.Editor.Utils
{



    [InitializeOnLoad]
    public class AlwaysSelectParentInHierarchy
    {
        public static bool IsEnabled = true; // Flag to check if the script functionality is enabled
        public static bool DebugMessages = true; // Flag to check if debug messages should be displayed

        static AlwaysSelectParentInHierarchy()
        {
            Selection.selectionChanged += OnSelectionChanged;
            Log("SelectionHandler initialized.");
        }

        static void OnSelectionChanged()
        {
            if (!IsEnabled)
                return;

            Log("Selection changed.");

            if (!MouseIsOverAnySceneView())
            {
                Log("Mouse is not over any SceneView.");
                return;
            }

            GameObject[] selectedObjects = Selection.gameObjects;

            for (int goIndex=0; goIndex<selectedObjects.Length; goIndex++)
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

        static bool MouseIsOverAnySceneView()
        {
            foreach (SceneView view in SceneView.sceneViews)
            {
                if ((Object)view == SceneView.mouseOverWindow)
                    return true;
            }

            return false;
        }

        // Helper function to display logs based on the DebugMessages flag
        static void Log(string message)
        {
            if (DebugMessages)
                Debug.Log(message);
        }

        [MenuItem("Tools/Toggle LOD Parent Selection")]
        public static void ToggleFunctionality()
        {
            IsEnabled = !IsEnabled;
            Log("LOD Parent Selection is " + (IsEnabled ? "enabled" : "disabled"));
        }

        [MenuItem("Tools/Toggle LOD Debug Messages")]
        public static void ToggleDebugMessages()
        {
            DebugMessages = !DebugMessages;
            Debug.Log("LOD Debug Messages are " + (DebugMessages ? "enabled" : "disabled"));
        }
    }
}