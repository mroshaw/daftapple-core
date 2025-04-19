using System;
using DaftAppleGames.Editor.Extensions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DaftAppleGames.Editor
{
    public abstract class BaseEditorWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset baseVisualTree;
        [SerializeField] private VisualTreeAsset customEditorVisualTree;
        [SerializeField] private bool detailedLogging;
        [SerializeField] private bool logToConsole;

        // Bound text to display in the Editor
        [SerializeField] private string logText;
        [SerializeField] private string titleText;
        [SerializeField] private string introText;

        private bool _isPopupOpen = false;

        // Logging instance
        protected EditorLog Log;
        protected VisualElement CustomEditorRootVisualElement;

        private Button _clearLogButton;
        private VisualElement _customEditorContainer;
        private ScrollView _logTextScrollView;
        private SerializedObject _serializedObject;

        private bool _hasInternalLogging;

        // protected virtual string WindowTitle => "";
        protected virtual string ToolTitle => "";
        protected virtual string IntroText => "";
        protected virtual string WelcomeLogText => "";

        public virtual void CreateGUI()
        {
            Log = new EditorLog(logToConsole, detailedLogging);

            baseVisualTree.CloneTree(rootVisualElement);

            // Setup the custom editor content in the container placeholder
            _customEditorContainer = rootVisualElement.Q<VisualElement>("CustomEditorContainer");
            if (customEditorVisualTree)
            {
                CustomEditorRootVisualElement = customEditorVisualTree.Instantiate();
                _customEditorContainer.Add(CustomEditorRootVisualElement);
            }

            Toggle detailedLoggingToggle = rootVisualElement.Q<Toggle>("DetailedLoggingToggle");
            detailedLoggingToggle?.RegisterValueChangedCallback(evt => DetailedLoggingToggled(evt.newValue));
            if (detailedLoggingToggle != null)
            {
                detailedLogging = detailedLoggingToggle.value;
            }

            TextField logTextField = rootVisualElement.Q<TextField>("LogText");
            _hasInternalLogging = logTextField != null;

            if (_hasInternalLogging)
            {
                Log.LogChangedEvent.RemoveListener(LogChangedHandler);
                Log.LogChangedEvent.AddListener(LogChangedHandler);


                logTextField.RegisterValueChangedCallback(_ => ScrollLogToBottom());
                _logTextScrollView = logTextField.Q<ScrollView>();

                Toggle logToConsoleToggle = rootVisualElement.Q<Toggle>("LogToConsoleToggle");
                logToConsoleToggle?.RegisterValueChangedCallback(evt => LogToConsoleToggled(evt.newValue));
                if (logToConsoleToggle != null)
                {
                    logToConsole = logToConsoleToggle.value;
                }

                Button clearLogButton = rootVisualElement.Q<Button>("ClearLogButton");
                clearLogButton.clicked -= ClearLog;
                clearLogButton.clicked += ClearLog;

                ClearLog();
            }
            else
            {
                Log.LogToConsole = true;
            }

            // Set window titles
            titleText = ToolTitle;
            introText = IntroText;

            Log.Log(LogLevel.Info, WelcomeLogText);

            CreateCustomGUI();

            BindUI();
        }

        protected abstract void CreateCustomGUI();

        private void BindUI()
        {
            // Bind to UI
            _serializedObject = new SerializedObject(this);
            rootVisualElement.Bind(_serializedObject);
        }

        private void LogToConsoleToggled(bool value)
        {
            Log.LogToConsole = value;
        }

        private void DetailedLoggingToggled(bool value)
        {
            Log.DetailedLogging = value;
        }

        private void LogChangedHandler(EditorLog changedLog)
        {
            logText = changedLog.GetLogAsString();
            ScrollLogToBottom();
        }

        private void ClearLog()
        {
            Log.Clear();
        }

        private void ScrollLogToBottom()
        {
            if (_logTextScrollView != null)
            {
                _logTextScrollView.scrollOffset = _logTextScrollView.contentContainer.layout.max - _logTextScrollView.contentViewport.layout.size;
                // _logTextScrollView.scrollOffset = new Vector2(0, float.MaxValue); // Force scroll to bottom
            }
        }

        /// <summary>
        /// Shows a Popup dialog that blocks interaction with the window until closed.
        /// </summary>
        protected internal void ShowPopupWindow(string windowTitleText, string popUpTitleText, string popUpContentText)
        {
            // Check if popup is already being shown
            if (_isPopupOpen)
            {
                return;
            }

            // Disable window content
            SetContentInteractableState(false);
            PopupWindow.Show(windowTitleText, popUpTitleText, popUpContentText, PopupClosed);
        }

        private void PopupClosed()
        {
            _isPopupOpen = false;
            // Enable window content
            SetContentInteractableState(true);
        }

        /// <summary>
        /// Sets the content of the Editor Window to be interactable or not
        /// </summary>
        private void SetContentInteractableState(bool stateValue)
        {
            rootVisualElement.SetInteractableState(stateValue);
        }
    }
}