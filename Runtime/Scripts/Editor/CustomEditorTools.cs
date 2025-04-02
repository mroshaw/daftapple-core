using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;

namespace DaftAppleGames.Editor
{
    /// <summary>
    /// Static methods for basic Editor stuff
    /// </summary>
    public static class CustomEditorTools
    {
        #region Static properties

        #endregion

        #region Class methods

        /// <summary>
        /// Sets a script define based on the active render pipeline in the project
        /// Runs immediately when the editor loads
        /// </summary>
        [InitializeOnLoadMethod]
        public static void SetRenderPipelineDefine()
        {
            // Get current scripting define symbols for Standalone
            string defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);

            // Check if HDRP is active
            if (GraphicsSettings.defaultRenderPipeline != null &&
                GraphicsSettings.defaultRenderPipeline.GetType().Name.Contains("HDRenderPipeline"))
            {
                // Add HDRP define if it's not already present
                if (!defines.Contains("DAG_HDRP"))
                {
                    defines += ";DAG_HDRP";
                }
            }
            // Check if URP is active
            else if (GraphicsSettings.defaultRenderPipeline != null &&
                     GraphicsSettings.defaultRenderPipeline.GetType().Name.Contains("UniversalRenderPipeline"))
            {
                // Add URP define if it's not already present
                if (!defines.Contains("DAG_URP"))
                {
                    defines += ";DAG_URP";
                }
            }
            // Check if Built-In Render Pipeline is active
            else if (GraphicsSettings.defaultRenderPipeline == null)
            {
                // Add Built-In define if it's not already present
                if (!defines.Contains("DAG_BIRP"))
                {
                    defines += ";DAG_BIRP";
                }
            }

            // Set the updated define symbols
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, defines);
        }

        /// <summary>
        /// Adds a new tag to the project tag list
        /// </summary>
        public static void AddTag(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                Debug.LogError("Tag name cannot be null or empty.");
                return;
            }

            // Open the TagManager asset
            SerializedObject tagManager = new(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            // Access the Tags property
            SerializedProperty tagsProp = tagManager.FindProperty("tags");

            // Check if the tag already exists
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty tagSp = tagsProp.GetArrayElementAtIndex(i);
                if (tagSp != null && tagSp.stringValue == tagName)
                {
                    Debug.LogWarning($"Tag \"{tagName}\" already exists.");
                    return;
                }
            }

            // Add the new tag
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            SerializedProperty newTag = tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1);
            newTag.stringValue = tagName;

            tagManager.ApplyModifiedProperties();
            Debug.Log($"Added tag \"{tagName}\" successfully.");
        }

        /// <summary>
        /// Changes the name of the given Rendering Layer
        /// </summary>
        public static void RenameRenderingLayer(int layerIndex, string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                Debug.LogError("Rendering Layer name cannot be null or empty.");
                return;
            }

            SerializedObject tagManager = new(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            // Access the Rendering Layer property
            SerializedProperty layersProp = tagManager.FindProperty("m_RenderingLayers");
            SerializedProperty layerSp = layersProp.GetArrayElementAtIndex(layerIndex);
            layerSp.stringValue = newName;
            tagManager.ApplyModifiedProperties();
        }

        /// <summary>
        /// Adds a Layer to the project layer list
        /// </summary>
        public static void AddLayer(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                Debug.LogError("Layer name cannot be null or empty.");
                return;
            }

            // Open the TagManager asset
            SerializedObject tagManager = new(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

            // Access the Layers property
            SerializedProperty layersProp = tagManager.FindProperty("layers");

            // Check if the layer already exists
            for (int i = 0; i < layersProp.arraySize; i++)
            {
                SerializedProperty layerSp = layersProp.GetArrayElementAtIndex(i);
                if (layerSp != null && layerSp.stringValue == layerName)
                {
                    Debug.LogWarning($"Layer \"{layerName}\" already exists.");
                    return;
                }
            }

            // Add the layer to the first empty slot (user-defined layers start at index 8)
            for (int i = 8; i < layersProp.arraySize; i++)
            {
                SerializedProperty layerSp = layersProp.GetArrayElementAtIndex(i);
                if (layerSp != null && string.IsNullOrEmpty(layerSp.stringValue))
                {
                    layerSp.stringValue = layerName;
                    Debug.Log($"Added layer \"{layerName}\" at index {i}.");
                    tagManager.ApplyModifiedProperties();
                    return;
                }
            }

            Debug.LogError("No empty slots available for new layers.");
        }

        #endregion
    }
}