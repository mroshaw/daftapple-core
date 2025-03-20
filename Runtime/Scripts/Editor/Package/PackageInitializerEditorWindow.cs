using DaftAppleGames.Editor.Package;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DaftAppleGames.Editor
{
    // Use this as a basis for all Package Initializer windows
    public abstract class PackageInitializerEditorWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset tree;
        [SerializeField] private PackageContents packageContents;
        [SerializeField] private bool loggingEnabled = true;
        [SerializeField] string logText;
        [SerializeField] private string introText;
        [SerializeField] private string baseInstallLocation;

        private Button _installButton;
        private Button _unInstallButton;
        private Button _reInstallButton;
        private Button _clearLogButton;

        private TextField _logTextField;

        private SerializedObject _serializedObject;

        public void CreateGUI()
        {
            if (!packageContents)
            {
                Log(LogLevel.Error, "PackageContents not set!");
                return;
            }

            tree.CloneTree(rootVisualElement);

            // Add button listeners
             _installButton = rootVisualElement.Q<Button>("InstallButton");
            if (_installButton != null)
            {
                _installButton.clicked += Install;
            }

            _unInstallButton = rootVisualElement.Q<Button>("UnInstallButton");
            if (_unInstallButton != null)
            {
                _unInstallButton.clicked += UnInstall;
            }

            _reInstallButton = rootVisualElement.Q<Button>("ReInstallButton");
            if (_reInstallButton != null)
            {
                _reInstallButton.clicked += ReInstall;
            }

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

            packageContents.onInstallStateChanged.AddListener(SetButtonState);
            SetButtonState(packageContents.IsAlreadyInstalled());

            // Bind to UI
            _serializedObject = new SerializedObject(this);
            rootVisualElement.Bind(_serializedObject);

            introText = GetIntroText();

            baseInstallLocation = GetBaseInstallLocation();

            ClearLog();
        }

        protected abstract string GetIntroText();

        protected abstract string GetBaseInstallLocation();

        private void Install()
        {
            if (packageContents.IsAlreadyInstalled())
            {
                Log(LogLevel.Error, "Already installed!");
                return;
            }

            Log(LogLevel.Info, $"Installing... Logging is: {loggingEnabled}", true);
            bool installResult = packageContents.Install(LogDelegate, baseInstallLocation);
            if (installResult)
            {
                Log(LogLevel.Info,$"Install Complete!", true);
                packageContents.SetInstallState(true);
            }
            else
            {
                Log(LogLevel.Error, $"Install Failed! Check logs!", true);
            }
        }

        private void UnInstall()
        {
            if (!packageContents.IsAlreadyInstalled())
            {
                Log(LogLevel.Error, $"Package is not installed!!");
            }
            Log(LogLevel.Info,$"Uninstalling... Logging is: {loggingEnabled}", true);
            Log(LogLevel.Info, $"Uninstall Complete!", true);
            packageContents.SetInstallState(false);

        }

        private void ReInstall()
        {
            if (!packageContents.IsAlreadyInstalled())
            {
                Log(LogLevel.Error, $"Package is not installed!!");
            }
            Log(LogLevel.Info, $"Reinstalling... Logging is: {loggingEnabled}", true);
            Log(LogLevel.Info, $"Reinstall Complete!", true);
        }

        private void LogDelegate(LogLevel logLevel, string message)
        {
            Log(logLevel, message);
        }

        private void Log(LogLevel logLevel, string message, bool force = false)
        {
            if (!loggingEnabled && !force && logLevel != LogLevel.Error)
            {
                return;
            }

            string fullLogText = "";

            switch (logLevel)
            {
                case LogLevel.Info:
                    fullLogText = $"\nInfo: {message}\n";
                    Debug.Log(message);
                    break;
                case LogLevel.Warning:
                    fullLogText = $"\nWarning: {message}\n";
                    Debug.LogWarning(message);
                    break;
                case LogLevel.Error:
                    fullLogText = $"\nError: {message}\n";
                    Debug.LogError(message);
                    break;
            }

            logText += fullLogText;
            logText = logText.TrimEnd('\r', '\n').TrimStart('\r', '\n');

        }

        private void ClearLog()
        {
            logText = string.Empty;
        }

        private void SetButtonState(bool installedState)
        {
            _installButton.SetEnabled(!installedState);
            _unInstallButton.SetEnabled(installedState);
            _reInstallButton.SetEnabled(installedState);
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