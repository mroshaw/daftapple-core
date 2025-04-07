using System;
using DaftAppleGames.Darskerry.Core.Buildings;
using DaftAppleGames.Extensions;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace DaftAppleGames.Editor
{
    /// <summary>
    /// Abstract class for creating new Editor tools that can be added to the
    /// main editor window
    /// </summary>
    [Serializable] public abstract class EditorTool : ScriptableObject
    {
        [SerializeField] private VisualTreeAsset visualTreeAsset;
        [SerializeField] private string runToolButtonName;

        // Use a local instance to avoid writing changes to the asset
        private ButtonWizardEditorSettings _localEditorSettingsInstance;

        protected EditorLog Log;

        private VisualElement _rootVisualElement;
        private GameObject _selectedGameObject;
        private EditorTools _parentTools;

        internal void SetParentEditorTools(EditorTools parentTools, EditorLog log)
        {
            _parentTools = parentTools;
            Log = log;
        }

        /// <summary>
        /// Construct the UI and bind the button click to the tool run method
        /// </summary>
        protected internal virtual VisualElement InitUserInterface()
        {
            _rootVisualElement = visualTreeAsset.CloneTree();

            // Bind the button click to RunTool
            Button runToolButton = _rootVisualElement.Q<Button>(runToolButtonName);
            if (runToolButton == null)
            {
                Debug.LogError($"Could not find button named {runToolButtonName}!!!");
                return null;
            }

            runToolButton.clicked -= RunToolClicked;
            runToolButton.clicked += RunToolClicked;

            AddCustomBindings();

            return _rootVisualElement;
        }

        protected virtual void AddCustomBindings()
        {
        }

        /// <summary>
        /// Run the tool, taking the settings, Game Object, and log from the parent tools
        /// </summary>
        private void RunToolClicked()
        {
            if (!CanRunTool(_parentTools.SelectedGameObject, _parentTools.EditorSettings))
            {
                return;
            }

            RunTool(_parentTools.SelectedGameObject, _parentTools.EditorSettings);
        }

        protected bool RequiredBuildingValidation()
        {
            if (_parentTools.SelectedGameObject && _parentTools.SelectedGameObject.HasComponent<Building>())
            {
                return true;
            }

            _parentTools.EditorLog.Log(LogLevel.Error, "The selected game object must contain a Building component to run this tool!");
            return false;
        }

        /// <summary>
        /// A standard validation for use by tools to check for settings and a selected game object
        /// </summary>
        /// <returns></returns>
        protected bool RequireSettingsAndGameObjectValidation()
        {
            if (_parentTools.EditorSettings && _parentTools.SelectedGameObject)
            {
                return true;
            }

            _parentTools.EditorLog.Log(LogLevel.Error, "You must select some settings and a game object to run this tool!");
            return false;
        }

        /// <summary>
        /// Binds the given toggle control to an event callback. This allows tools to present their own toggle options in the UI
        /// and bind the control to a local bool
        /// </summary>
        /// <param name="toggleName"></param>
        /// <param name="toggleChangeEvent"></param>
        /// <returns></returns>
        protected bool BindToToggleOption(string toggleName, EventCallback<ChangeEvent<bool>> toggleChangeEvent)
        {
            Toggle toggle = _rootVisualElement.Q<Toggle>(toggleName);
            if (toggle == null)
            {
                _parentTools.EditorLog.Log(LogLevel.Error, "Couldn't find toggle. Failed to bind option to toggle: " + toggleName);
                return false;
            }

            toggle.RegisterValueChangedCallback(toggleChangeEvent);
            return toggle.value;
        }

        /// <summary>
        /// Return true if the tool can run with the given parameters, otherwise false
        /// </summary>
        protected abstract bool CanRunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings);

        /// <summary>
        /// Runs the tool, on the basis that inputs are valid
        /// </summary>
        protected abstract void RunTool(GameObject selectedGameObject, ButtonWizardEditorSettings editorSettings);
    }
}