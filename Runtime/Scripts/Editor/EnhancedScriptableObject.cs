using System.IO;
using UnityEditor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.Editor
{
    /// <summary>
    /// Adds some additional functionality to the basic ScriptableObject class
    /// </summary>
    public abstract class EnhancedScriptableObject : ScriptableObject
    {
        /// <summary>
        /// Opens a Save dialog and allows the user to save a copy of this ScriptableObject instance
        /// </summary>
        public virtual string SaveCopyInteractive(out EnhancedScriptableObject newInstance)
        {
            string defaultFileName = $"CopyOf_{GetAssetFileName()}";

            string saveFullFilePath = EditorUtility.SaveFilePanel(
                "Save a copy",
                Application.dataPath,
                defaultFileName,
                "asset");

            if (string.IsNullOrEmpty(saveFullFilePath))
            {
                newInstance = this;
                return string.Empty;
            }

            string saveFileName = Path.GetFileName(saveFullFilePath);
            string saveFolderPath = Path.GetDirectoryName(saveFullFilePath);

            newInstance = SaveCopy(saveFolderPath, saveFileName);
            return saveFolderPath;
        }

        /// <summary>
        /// Adds a button to Scriptable Object inspectors to allow user to save a copy
        /// </summary>
        [Button("Save A Copy")]
        private void SaveCopyInteractiveEditor()
        {
            SaveCopyInteractive(out EnhancedScriptableObject _);
        }

        /// <summary>
        /// Returns the name of the current scriptable object instance asset file
        /// </summary>
        /// <returns></returns>
        private string GetAssetFileName()
        {
            string path = AssetDatabase.GetAssetPath(this);
            return Path.GetFileName(path);
        }

        /// <summary>
        /// Can be called externally to force a save without prompting for a path
        /// </summary>
        public EnhancedScriptableObject SaveCopy(string pathToSave, string fileName)
        {
            string fileNameToSave = string.IsNullOrEmpty(fileName) ? GetAssetFileName() : fileName;

            string fullFilePath = Path.Combine(pathToSave, fileNameToSave);

            Debug.Log($"Saving copy of Scriptable Object {fileNameToSave} to: {fullFilePath}...");
            string relativePath = "Assets" + fullFilePath[Application.dataPath.Length..];
            EnhancedScriptableObject scriptableObjectInstanceCopy = Instantiate(this);
            AssetDatabase.CreateAsset(scriptableObjectInstanceCopy, relativePath);
            return scriptableObjectInstanceCopy;
        }
    }
}