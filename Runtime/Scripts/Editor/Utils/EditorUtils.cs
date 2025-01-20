using UnityEditor;
using UnityEngine;

namespace DaftAppleGames.Utils.Editor
{
    public static class EditorUtils
    {
        public static void AddTag(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                Debug.LogError("Tag name cannot be null or empty.");
                return;
            }

            // Open the TagManager asset
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

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

        public static void AddLayer(string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                Debug.LogError("Layer name cannot be null or empty.");
                return;
            }

            // Open the TagManager asset
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);

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
    }
}