using UnityEditor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.Editor
{
    public abstract class ButtonWizardEditorSettings : ScriptableObject
    {
        [Button("Save A Copy")]
        internal ButtonWizardEditorSettings SaveALocalCopy()
        {
            string pathToSave = EditorUtility.SaveFilePanel(
                "Save a local copy of settings",
                Application.dataPath,
                "myBuildingEditorSettings.asset",
                "asset");

            if (string.IsNullOrEmpty(pathToSave))
            {
                return this;
            }

            string relativePath = "Assets" + pathToSave[Application.dataPath.Length..];

            ButtonWizardEditorSettings newEditorSettings = Instantiate(this);
            AssetDatabase.CreateAsset(newEditorSettings, relativePath);
            return newEditorSettings;
        }
    }
}