using System;
using PlasticGui.Configuration.CloudEdition.Welcome;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DaftAppleGames.Editor
{
    /// <summary>
    /// Abstract class for creating new Editor tools that can be added to the
    /// main editor window
    /// </summary>
    [Serializable] public abstract class EditorTool : ScriptableObject
    {
        // Standard return strings for validation functions
        protected static string selectEditorSettingsError = "Please select some editor settings!";
        protected static string selectEditorSettingsAndGameObjectError = "Please select some editor settings and a game object!";
        protected static string selectGameObjectError = "Please a game object!";

        [SerializeField] private VisualTreeAsset visualTreeAsset;
        [SerializeField] private string runToolButtonName;

        // Use a local instance to avoid writing changes to the asset
        private ButtonWizardEditorSettings _localEditorSettingsInstance;

        protected static EditorLog log;

        protected VisualElement RootVisualElement;
        protected EditorToolsList ParentToolsList;

        internal void SetParentEditorTools(EditorToolsList parentToolsList, EditorLog editorLog)
        {
            ParentToolsList = parentToolsList;
            log = editorLog;
        }

        /// <summary>
        /// Shows a popup dialog, locking the base window until the popup is closed
        /// </summary>
        protected void ShowPopupWindow(string windowTitleText, string popUpTitleText, string popUpContentText)
        {
            ParentToolsList.BaseEditorWindow.ShowPopupWindow(windowTitleText, popUpTitleText, popUpContentText);
        }

        /// <summary>
        /// Construct the UI and bind the button click to the tool run method
        /// </summary>
        protected internal virtual VisualElement InitUserInterface()
        {
            RootVisualElement = visualTreeAsset.CloneTree();

            // Find the button
            Button runToolButton = RootVisualElement.Q<Button>(runToolButtonName);
            if (runToolButton == null)
            {
                Debug.LogError($"Could not find button named {runToolButtonName}!!!");
                return null;
            }

            // Check to see if the tool is supported. If not, disable the button and add a tooltip
            if (!IsSupported(out string notSupportedReason))
            {
                runToolButton.SetEnabled(false);
                runToolButton.tooltip = notSupportedReason;
                return RootVisualElement;
            }

            // Bind the button click to RunTool
            runToolButton.clicked -= RunToolClicked;
            runToolButton.clicked += RunToolClicked;

            runToolButton.text = GetToolName();

            // Find the Options FoldoutGroup, if it exists, and close it by default
            Foldout optionsFoldout = RootVisualElement.Q<Foldout>();
            if (optionsFoldout != null)
            {
                optionsFoldout.text = $"{GetToolName()} Options";
                optionsFoldout.value = false;
            }

            // Bind any other custom controls and properties
            AddCustomBindings();

            return RootVisualElement;
        }

        /// <summary>
        /// Can be override to provide custom UI control bindings in the tool window
        /// </summary>
        protected virtual void AddCustomBindings()
        {
        }

        /// <summary>
        /// Binds the given toggle control to an event callback. This allows tools to present their own toggle options in the UI
        /// and bind the control to a local bool
        /// </summary>
        protected bool BindToToggleOption(string toggleName, EventCallback<ChangeEvent<bool>> toggleChangeEvent)
        {
            Toggle toggle = RootVisualElement.Q<Toggle>(toggleName);
            if (toggle == null)
            {
                ParentToolsList.EditorLog.Log(LogLevel.Error, "Couldn't find toggle. Failed to bind option to toggle: " + toggleName);
                return false;
            }

            toggle.RegisterValueChangedCallback(toggleChangeEvent);
            return toggle.value;
        }

        /// <summary>
        /// Run the tool, taking the settings, Game Object, and log from the parent tools
        /// </summary>
        private void RunToolClicked()
        {
            if (!CanRunTool(ParentToolsList.SelectedGameObject, ParentToolsList.EditorSettings, out string cannotRunReason))
            {
                log.Log(LogLevel.Error, cannotRunReason);
                return;
            }

            int undoGroup = Undo.GetCurrentGroup();
            string undoGroupName = $"Run {GetToolName()}";

            // Set up Undo
            if (ParentToolsList.SelectedGameObject)
            {
                Undo.SetCurrentGroupName(undoGroupName);
                Undo.RegisterFullObjectHierarchyUndo(ParentToolsList.SelectedGameObject, undoGroupName);
            }

            log.Log(LogLevel.Info, $"Running: {GetToolName()}...");

            // Run the tool
            RunTool(ParentToolsList.SelectedGameObject, ParentToolsList.EditorSettings, undoGroupName);

            // Collapse all undo operations into a single entry
            Undo.CollapseUndoOperations(undoGroup);

            log.Log(LogLevel.Info, $"Running: {GetToolName()}... DONE!");
        }

        /// <summary>
        /// A standard validation for use by tools to check for settings and a selected game object
        /// </summary>
        /// <returns></returns>
        protected bool RequireSettingsAndGameObjectValidation()
        {
            if (ParentToolsList.EditorSettings && ParentToolsList.SelectedGameObject)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the name of the tool for use in debugging, undo etc.
        /// </summary>
        protected abstract string GetToolName();

        /// <summary>
        /// Returns true if the tool is supported in the current environment. For example, depending on the current Render Pipeline
        /// </summary>
        /// <returns></returns>
        protected abstract bool IsSupported(out string notSupportedReason);

        /// <summary>
        /// Return true if the tool can run with the given parameters, otherwise false
        /// </summary>
        protected abstract bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings, out string cannotRunReason);

        /// <summary>
        /// Runs the tool, on the basis that inputs are valid
        /// </summary>
        protected abstract void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings, string undoGroupName);
    }
}