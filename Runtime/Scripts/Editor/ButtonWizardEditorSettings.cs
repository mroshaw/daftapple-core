using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.Editor
{
    public abstract class ButtonWizardEditorSettings : ScriptableObject
    {
        [SerializeField] [BoxGroup("Tools")] internal List<EditorTool> toolsList;

        /// <summary>
        /// Constructs the Tools by iterating over each tool and getting its UI
        /// </summary>
        protected internal VisualElement InitSettings(ButtonWizardEditorWindow baseButtonWizardEditorWindow, EditorLog log)
        {
            VisualElement rootElement = new();

            foreach (EditorTool editorTool in toolsList)
            {
                rootElement.Add(editorTool.InitTool(baseButtonWizardEditorWindow, log));
            }

            return rootElement;
        }

        [Button("Save A Copy")]
        internal ButtonWizardEditorSettings SaveALocalCopy()
        {
            string pathToSave = EditorUtility.SaveFilePanel(
                "Save a local copy of settings",
                Application.dataPath,
                "ButtonWizardEditorSettings.asset",
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