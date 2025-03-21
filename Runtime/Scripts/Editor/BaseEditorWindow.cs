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
        [SerializeField] private bool loggingEnabled = true;

        // Bound text to display in the Editor
        [SerializeField] string logText;

        [SerializeField] private string titleText;
        [SerializeField] private string introText;

        protected virtual string WindowTitle => "Base Editor";
        protected virtual string ToolTitle => "Base Editor";
        protected virtual string IntroText => "Welcome to Base Editor!";
        protected virtual string WelcomeLogText => "Welcome to Base Editor!";

        private Button _clearLogButton;
        private TextField _logTextField;
        private SerializedObject _serializedObject;
        private VisualElement _customEditorContainer;

        // Logging instance
        protected readonly EditorLog log = new();

        public virtual void CreateGUI()
        {
            log.LogChangedEvent.RemoveListener(LogChangedHandler);
            log.LogChangedEvent.AddListener(LogChangedHandler);

            baseVisualTree.CloneTree(rootVisualElement);

            // Setup the custom editor content in the container placeholder
            VisualElement customEditorRoot = customEditorVisualTree.Instantiate();
            _customEditorContainer = rootVisualElement.Q<VisualElement>("CustomEditorContainer");
            _customEditorContainer.Add(customEditorRoot);

            _logTextField = rootVisualElement.Q<TextField>("LogText");
            if (_logTextField != null)
            {
                _logTextField.RegisterValueChangedCallback(evt => ScrollLogToBottom());
            }

            _clearLogButton = rootVisualElement.Q<Button>("ClearLogButton");
            if (_clearLogButton != null)
            {
                _clearLogButton.clicked += ClearLog;
            }

            // Bind to UI
            _serializedObject = new SerializedObject(this);
            rootVisualElement.Bind(_serializedObject);

            titleText = WindowTitle;
            introText = IntroText;

            ClearLog();
            log.Log(LogLevel.Info, WelcomeLogText);
        }


        private void LogChangedHandler(EditorLog changedLog)
        {
            logText = changedLog.GetLogAsString();
        }

        private void ClearLog()
        {
            log.Clear();
        }

        private void ScrollLogToBottom()
        {
            if (_logTextField != null)
            {
                // Get the internal ScrollView inside the TextField
                var scrollView = _logTextField.Q<ScrollView>();
                if (scrollView != null)
                {
                    scrollView.scrollOffset = new Vector2(0, float.MaxValue); // Force scroll to bottom
                }
            }
        }
    }
}