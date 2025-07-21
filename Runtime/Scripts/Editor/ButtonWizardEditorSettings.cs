using System.Collections.Generic;
using DaftAppleGames.Utilities;
using UnityEngine;
using UnityEngine.UIElements;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.Editor
{
    public abstract class ButtonWizardEditorSettings : EnhancedScriptableObject
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
                if (!editorTool)
                {
                    Debug.LogWarning($"An Editor Tool in the {name} settings is null. Please check the instance asset!");
                    continue;
                }

                rootElement.Add(editorTool.InitTool(baseButtonWizardEditorWindow, log));
            }

            return rootElement;
        }

        public override string SaveCopyInteractive(out EnhancedScriptableObject newInstance)
        {
            string pathToSave = base.SaveCopyInteractive(out EnhancedScriptableObject instanceCopy);

            ButtonWizardEditorSettings newSettings = instanceCopy as ButtonWizardEditorSettings;

            if (string.IsNullOrEmpty(pathToSave) || !newSettings)
            {
                newInstance = this;
                return string.Empty;
            }

            List<EditorTool> newToolsList = new();

            // Save a copy of each Tool config in the same folder
            foreach (EditorTool editorTool in toolsList)
            {
                EditorTool newTool = editorTool.SaveCopy(pathToSave, string.Empty, "Tool Settings") as EditorTool;
                newToolsList.Add(newTool);
            }

            newSettings.toolsList = newToolsList;

            newInstance = newSettings;
            return pathToSave;
        }
    }
}