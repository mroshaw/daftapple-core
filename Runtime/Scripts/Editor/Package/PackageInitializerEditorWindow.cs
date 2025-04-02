using System.IO;
using DaftAppleGames.Editor.Package;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DaftAppleGames.Editor
{
    // Use this as a basis for all Package Initializer windows
    public abstract class PackageInitializerEditorWindow : BaseEditorWindow
    {
        [SerializeField] private PackageContents packageContents;
        private const string BaseInstallFolder = "DaftAppleGames";

        private Button _installButton;
        private Button _unInstallButton;
        private Button _reInstallButton;
        private Button _clearLogButton;

        private Toggle _showAtStartupToggle;

        [SerializeField] private string packageFullInstallFolder;

        private PackageContents _installerPackage;

        private string _packageInstallFullPath;

        public override void CreateGUI()
        {
            InitInstaller();

            base.CreateGUI();

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
                if (!_installerPackage.allowUninstall)
                {
                    _unInstallButton.visible = false;
                }
            }

            _reInstallButton = rootVisualElement.Q<Button>("ReInstallButton");
            if (_reInstallButton != null)
            {
                _reInstallButton.clicked += ReInstall;
            }

            _installerPackage.onInstallStateChanged.AddListener(SetButtonState);

            SetButtonState(_installerPackage.IsAlreadyInstalled());
            packageFullInstallFolder = Path.Combine(BaseInstallFolder, _installerPackage.packageInstallFolder);
            _packageInstallFullPath = Path.Combine(Application.dataPath, packageFullInstallFolder);
        }

        private void InitInstaller()
        {
            // Create the base folder, so we can save some bits and pieces across
            string baseFolderFullPath = Path.Combine(Application.dataPath, BaseInstallFolder);
            if (!Directory.Exists(baseFolderFullPath))
            {
                Directory.CreateDirectory(baseFolderFullPath);
            }

            // Create a packages subfolder for copies of the package content scriptable objects
            string packagesFolderFullPath = Path.Combine(baseFolderFullPath, "Packages");
            if (!Directory.Exists(packagesFolderFullPath))
            {
                Directory.CreateDirectory(packagesFolderFullPath);
            }

            // If the copy of the package already exists, use that from now on
            _installerPackage = packageContents.GetLocalCopy(packagesFolderFullPath);
        }

        private void Install()
        {
            Install(false);
        }

        private void Install(bool force)
        {
            if (!force && _installerPackage.IsAlreadyInstalled())
            {
                log.Log(LogLevel.Error, "Already installed!");
                return;
            }

            log.Log(LogLevel.Info, "Installing... ", true);
            bool installResult = _installerPackage.Install(packageFullInstallFolder, log);
            if (installResult)
            {
                PostInstallation(_installerPackage, log);
                log.Log(LogLevel.Info, "Install Complete!", true);
                _installerPackage.SetInstallState(true);
            }
            else
            {
                log.Log(LogLevel.Error, "Install Failed! Check logs!", true);
            }
        }

        private void UnInstall()
        {
            if (!_installerPackage.IsAlreadyInstalled())
            {
                log.Log(LogLevel.Error, $"Package is not installed!!");
            }

            log.Log(LogLevel.Info, $"Uninstalling...", true);
            PostUnInstallation(_installerPackage, log);
            log.Log(LogLevel.Info, $"Uninstall Complete!", true);
            _installerPackage.SetInstallState(false);
        }

        private void ReInstall()
        {
            if (!_installerPackage.IsAlreadyInstalled())
            {
                log.Log(LogLevel.Error, $"Package is not installed!!");
            }

            log.Log(LogLevel.Info, $"Reinstalling...", true);
            Install(true);
            log.Log(LogLevel.Info, $"Reinstall Complete!", true);
        }

        private void SetButtonState(bool installedState)
        {
            _installButton.SetEnabled(!installedState);
            _unInstallButton.SetEnabled(installedState);
            _reInstallButton.SetEnabled(installedState);
        }

        protected abstract void PostInstallation(PackageContents packageContents, EditorLog editorLog);
        protected abstract void PostUnInstallation(PackageContents packageContents, EditorLog editorLog);
    }
}