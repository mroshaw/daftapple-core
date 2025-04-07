using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace DaftAppleGames.Editor
{
    [CreateAssetMenu(fileName = "EditorTools", menuName = "Daft Apple Games/Editor Tools/Editor Tools List")]
    public class EditorTools : ScriptableObject
    {
        // List of tools to be made available via the Wizard UI
        [SerializeField] private List<EditorTool> editorToolsList;

        // Properties available to child Tool instances
        internal ButtonWizardEditorSettings EditorSettings { get; private set; }

        internal GameObject SelectedGameObject { get; private set; }

        internal EditorLog EditorLog { get; private set; }

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

        /// <summary>
        /// Call by the parent window when the user changes the selected Game Object
        /// </summary>
        public void SetSelectedGameObject(GameObject selectedGameObject)
        {
            SelectedGameObject = selectedGameObject;
        }
    }
}