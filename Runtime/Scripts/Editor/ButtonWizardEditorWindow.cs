using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DaftAppleGames.Editor
{
    public abstract class ButtonWizardEditorWindow : BaseEditorWindow
    {
        [SerializeField] private ButtonWizardEditorSettings editorSettings;
        [SerializeField] private GameObject selectedGameObject;

        protected internal UnityEvent<GameObject> SelectedGameObjectChanged = new();

        private VisualElement _toolButtonsContainer;

        /// <summary>
        /// Create and binds the UI
        /// </summary>
        public override void CreateGUI()
        {
            base.CreateGUI();

            // Tell the Tools when settings are changed
            ObjectField settingsObjectField = CustomEditorRootVisualElement.Q<ObjectField>("SettingsField");
            settingsObjectField.RegisterValueChangedCallback(SettingsChangedCallback);

            Button saveSettingCopyButton = CustomEditorRootVisualElement.Q<Button>("SaveLocalSettingsCopyButton");
            saveSettingCopyButton.clicked -= SaveCopyOfSettings;
            saveSettingCopyButton.clicked += SaveCopyOfSettings;

            _toolButtonsContainer = CustomEditorRootVisualElement.Q<VisualElement>("ToolButtonsContainer");
            selectedGameObject = Selection.activeGameObject;

            InitTools();
            OnSelectionChange();
        }

        private void InitTools()
        {
            if (!editorSettings)
            {
                return;
            }

            _toolButtonsContainer.Clear();
            _toolButtonsContainer.Add(editorSettings.InitSettings(this, Log));
        }

        private void OnSelectionChange()
        {
            selectedGameObject = Selection.activeGameObject;
            SelectedGameObjectChanged?.Invoke(selectedGameObject);
        }

        /// <summary>
        /// If the user docks/hides the window, when they return, we want to refresh the selected object
        /// </summary>
        private void OnFocus()
        {
            OnSelectionChange();
        }

        private void SaveCopyOfSettings()
        {
            ;
            _ = editorSettings.SaveCopyInteractive(out EnhancedScriptableObject newSettingsSoInstance);
            ButtonWizardEditorSettings newSettings = newSettingsSoInstance as ButtonWizardEditorSettings;
            editorSettings = newSettings;
        }

        /// <summary>
        /// If the user picks new settings, refresh the tools
        /// to each tool
        /// </summary>
        private void SettingsChangedCallback(ChangeEvent<Object> changeEvent)
        {
            InitTools();
        }
    }
}