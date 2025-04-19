using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DaftAppleGames.Editor
{
    public abstract class ButtonWizardEditorWindow : BaseEditorWindow
    {
        [SerializeField] private EditorToolsList editorToolsList;
        [SerializeField] private ButtonWizardEditorSettings editorSettings;
        [SerializeField] private GameObject selectedGameObject;

        /// <summary>
        /// Create and binds the UI
        /// </summary>
        public override void CreateGUI()
        {
            base.CreateGUI();

            // Tell the Tools when settings are changed
            ObjectField settingsObjectField = CustomEditorRootVisualElement.Q<ObjectField>("SettingsField");
            settingsObjectField.RegisterValueChangedCallback(SettingsChangedCallback);

            ObjectField selectedGameObjectField = CustomEditorRootVisualElement.Q<ObjectField>("SelectedObjectField");
            selectedGameObjectField.RegisterValueChangedCallback(SelectedGameObjectChangedCallback);

            Button saveSettingCopyButton = CustomEditorRootVisualElement.Q<Button>("SaveLocalSettingsCopyButton");
            saveSettingCopyButton.clicked -= SaveCopyOfSettings;
            saveSettingCopyButton.clicked += SaveCopyOfSettings;

            VisualElement toolsButtonsContainer = CustomEditorRootVisualElement.Q<VisualElement>("ToolButtonsContainer");
            toolsButtonsContainer.Add(editorToolsList.GetUserInterface(Log));

            selectedGameObject = Selection.activeGameObject;

            if (editorToolsList)
            {
                editorToolsList.SetBaseEditorWindow(this);
            }

            if (editorSettings && editorToolsList)
            {
                editorToolsList.SetEditorSettings(editorSettings);
            }

            if (selectedGameObject && editorToolsList)
            {
                editorToolsList.SetSelectedGameObject(selectedGameObject);
            }
        }

        private void OnSelectionChange()
        {
            selectedGameObject = Selection.activeGameObject;
        }

        private void SaveCopyOfSettings()
        {
            editorSettings = editorSettings.SaveALocalCopy();
            editorToolsList.SetEditorSettings(editorSettings);
        }

        /// <summary>
        /// If the user picks new settings, let the Tools List know so that it can propagate the change
        /// to each tool
        /// </summary>
        private void SettingsChangedCallback(ChangeEvent<Object> changeEvent)
        {
            if (changeEvent.newValue is ButtonWizardEditorSettings newEditorSettings)
            {
                editorToolsList.SetEditorSettings(newEditorSettings);
            }
        }

        private void SelectedGameObjectChangedCallback(ChangeEvent<Object> changeEvent)
        {
            editorToolsList.SetSelectedGameObject(changeEvent.newValue as GameObject);
        }
    }
}