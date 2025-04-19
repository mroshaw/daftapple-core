using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace DaftAppleGames.Editor
{
    public abstract class EditorToolsList : ScriptableObject
    {
        // List of tools to be made available via the Wizard UI
        [SerializeField] private List<EditorTool> editorToolsList;

        // Properties available to child Tool instances
        public ButtonWizardEditorSettings EditorSettings { get; private set; }

        public GameObject SelectedGameObject { get; private set; }

        public EditorLog EditorLog { get; private set; }

        // Allows the tools to access UI methods of the main window
        internal BaseEditorWindow BaseEditorWindow { get; private set; }

        /// <summary>
        /// Constructs the UI by iterating over each tool and getting its UI
        /// </summary>
        /// <returns></returns>
        protected internal VisualElement GetUserInterface(EditorLog log)
        {
            EditorLog = log;
            VisualElement rootElement = new();

            foreach (EditorTool editorTool in editorToolsList)
            {
                VisualElement toolContainer = new();
                toolContainer.AddToClassList("OutlineContainer");
                // Register the tool with parent Tools List so we can obtain the settings and selected Game Object
                editorTool.SetParentEditorTools(this, log);
                toolContainer.Add(editorTool.InitUserInterface());
                rootElement.Add(toolContainer);
            }

            return rootElement;
        }

        /// <summary>
        /// Called by the parent window when the user selects settings for the wizard
        /// </summary>
        public void SetEditorSettings(ButtonWizardEditorSettings editorSettings)
        {
            EditorSettings = editorSettings;
        }

        public void SetBaseEditorWindow(BaseEditorWindow baseEditorWindow)
        {
            BaseEditorWindow = baseEditorWindow;
        }

        /// <summary>
        /// Call by the parent window when the user changes the selected Game Object
        /// </summary>
        public void SetSelectedGameObject(GameObject selectedGameObject)
        {
            SelectedGameObject = selectedGameObject;
        }
    }
}