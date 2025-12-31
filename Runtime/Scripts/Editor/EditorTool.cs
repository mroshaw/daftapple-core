using System;
using System.Collections.Generic;
using DaftAppleGames.Utilities;
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
    /// <summary>
    /// Abstract class for creating new Editor tools that can be added to the
    /// main editor window
    /// </summary>
    [Serializable] public abstract class EditorTool : EnhancedScriptableObject
    {
        [BoxGroup("Editor Settings")] [SerializeField]
        [Tooltip("If set, this text will be appended to the tool name text on the editor tool button.")] private string toolNameAppendix;

        // Use a local instance to avoid writing changes to the asset
        private ButtonWizardEditorSettings _localEditorSettingsInstance;

        protected static EditorLog log;
        protected ButtonWizardEditorWindow ParentEditorWindow;

        protected internal GameObject SelectedGameObject { set; protected get; }

        /// <summary>
        /// Shows a popup dialog, locking the base window until the popup is closed
        /// </summary>
        protected void ShowPopupWindow(string windowTitleText, string popUpTitleText, string popUpContentText)
        {
            ParentEditorWindow.ShowPopupWindow(windowTitleText, popUpTitleText, popUpContentText);
        }

        /// <summary>
        /// Construct the UI and bind the button click to the tool run method
        /// </summary>
        protected internal virtual VisualElement InitTool(ButtonWizardEditorWindow editorWindow, out List<String> initLog)
        {
            ParentEditorWindow = editorWindow;
            initLog = new List<string>();
            
            VisualElement toolContainer = new();
            toolContainer.AddToClassList("RowContainer");

            // Create the run tool button
            Button runToolButton = new()
            {
                text = string.IsNullOrEmpty(toolNameAppendix) ? GetToolName() : $"{GetToolName()} ({toolNameAppendix})"
            };
            runToolButton.AddToClassList("StretchButton");

            // Create the tool settings button
            // Built-in gear icon (from UnityEditor built-in textures)
            Texture gearIcon = EditorGUIUtility.IconContent("_Popup").image;
            Image gearImage = new() { image = gearIcon };

            Button toolSettingsButton = new();
            toolSettingsButton.AddToClassList("FixedButton");
            toolSettingsButton.Add(gearImage);

            // Add to the row container
            toolContainer.Add(runToolButton);
            toolContainer.Add(toolSettingsButton);

            // Check to see if the tool is supported. If not, disable the button and add a tooltip
            if (!IsSupported(out string notSupportedReason))
            {
                runToolButton.SetEnabled(false);
                runToolButton.tooltip = notSupportedReason;
                return toolContainer;
            }

            // Bind the button click to RunTool
            runToolButton.clicked -= RunToolClicked;
            runToolButton.clicked += RunToolClicked;

            // Bind the settings button clicks
            toolSettingsButton.clicked -= SettingsClicked;
            toolSettingsButton.clicked += SettingsClicked;

            // Bind any other custom controls and properties
            AddCustomBindings();

            // Subscribe to the base window events
            editorWindow.SelectedGameObjectChanged?.RemoveListener(GameObjectChangedHandler);
            editorWindow.SelectedGameObjectChanged?.AddListener(GameObjectChangedHandler);

            return toolContainer;
        }

        private void GameObjectChangedHandler(GameObject newGameObject)
        {
            SelectedGameObject = newGameObject;
        }

        /// <summary>
        /// Can be override to provide custom UI control bindings in the tool window
        /// </summary>
        protected virtual void AddCustomBindings()
        {
        }

        /// <summary>
        /// Run the tool, taking the settings, Game Object, and log from the parent tools
        /// </summary>
        private void RunToolClicked()
        {
            if (!CanRunTool(out List<string> cannotRunReasons))
            {
                log.LogErrors(cannotRunReasons);
                return;
            }

            int undoGroup = Undo.GetCurrentGroup();
            string undoGroupName = $"Run {GetToolName()}";

            // Set up Undo
            if (SelectedGameObject)
            {
                Undo.SetCurrentGroupName(undoGroupName);
                Undo.RegisterFullObjectHierarchyUndo(SelectedGameObject, undoGroupName);
            }

            log.LogInfo($"Running: {GetToolName()}...");

            // Run the tool
            RunTool(undoGroupName);

            // Collapse all undo operations into a single entry
            Undo.CollapseUndoOperations(undoGroup);

            log.LogInfo($"Running: {GetToolName()}... DONE!");
        }

        /// <summary>
        /// Handle clicking the settings button
        /// </summary>
        private void SettingsClicked()
        {
            Selection.activeObject = this;
            EditorGUIUtility.PingObject(this);
        }

        /// <summary>
        /// A standard validation for use by tools to check for settings and a selected game object
        /// </summary>
        /// <returns></returns>
        protected bool RequireGameObjectValidation(out string failedReason)
        {
            if (SelectedGameObject)
            {
                failedReason = string.Empty;
                return true;
            }

            failedReason = "Please select a GameObject in the scene hierarchy!";
            return false;
        }

        /// <summary>
        /// Returns the name of the tool for use in debugging, undo etc.
        /// </summary>
        protected abstract string GetToolName();

        /// <summary>
        /// Returns true if the tool is supported in the current environment. For example, depending on the current Render Pipeline
        /// </summary>
        protected abstract bool IsSupported(out string notSupportedReason);

        /// <summary>
        /// Return true if the tool can run with the given parameters, otherwise false
        /// </summary>
        protected abstract bool CanRunTool(out List<string> cannotRunReasons);

        /// <summary>
        /// Runs the tool, on the basis that inputs are valid
        /// </summary>
        protected abstract void RunTool(string undoGroupName);
    }
}