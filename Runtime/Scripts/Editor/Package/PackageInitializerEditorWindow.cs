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
        private const string BaseInstallFolderName = "DaftAppleGames";

        private string PackageDataRelativeInstallFolder => Path.Combine("Assets", BaseInstallFolderName, "Packages");
        private string PackageDataAbsoluteInstallFolder => Path.Combine(Application.dataPath, BaseInstallFolderName, "Packages");
        private string PackageRelativeInstallBaseFolder => Path.Combine("Assets", BaseInstallFolderName);
        private string PackageAbsoluteInstallBaseFolder => Path.Combine(Application.dataPath, BaseInstallFolderName);

        private string PackageAbsoluteInstallFolder => Path.Combine(PackageAbsoluteInstallBaseFolder, packageContents.packageInstallFolder);
        private string PackageRelativeInstallFolder => Path.Combine(PackageRelativeInstallBaseFolder, packageContents.packageInstallFolder);

        private Button _installButton;
        private Button _unInstallButton;
        private Button _reInstallButton;
        private Button _clearLogButton;

        private Toggle _showAtStartupToggle;

        // Show the target folder in bound UI control
        [SerializeField] private string packageFullInstallFolder;

        private PackageContents _localPackageCopy;

        private string _packageInstallFullPath;

        public override void CreateGUI()
        {
            packageFullInstallFolder = PackageRelativeInstallFolder;
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
                if (!packageContents.allowUninstall)
                {
                    _unInstallButton.visible = false;
                }
            }

            _reInstallButton = rootVisualElement.Q<Button>("ReInstallButton");
            if (_reInstallButton != null)
            {
                _reInstallButton.clicked += ReInstall;
            }

            _localPackageCopy.onInstallStateChanged.AddListener(SetButtonState);

            SetButtonState(_localPackageCopy.IsAlreadyInstalled());
        }

        private void InitInstaller()
        {
            // Create the base folder, so we can save some bits and pieces across
            if (!Directory.Exists(PackageAbsoluteInstallBaseFolder))
            {
                AssetDatabase.CreateFolder("Assets", BaseInstallFolderName);
            }

            // Create a packages subfolder for copies of the package content scriptable objects
            if (!Directory.Exists(PackageDataAbsoluteInstallFolder))
            {
                AssetDatabase.CreateFolder(PackageRelativeInstallBaseFolder, "Packages");
            }

            // If the copy of the package already exists, use that from now on
            _localPackageCopy = packageContents.GetLocalCopy(PackageDataAbsoluteInstallFolder, PackageDataRelativeInstallFolder);
        }

        private void Install()
        {
            Install(false);
        }

        private void Install(bool force)
        {
            if (!force && _localPackageCopy.IsAlreadyInstalled())
            {
                log.Log(LogLevel.Error, "Already installed!");
                return;
            }

            log.Log(LogLevel.Info, "Installing... ", true);
            bool installResult = packageContents.Install(PackageAbsoluteInstallBaseFolder, PackageRelativeInstallBaseFolder, log);
            if (installResult)
            {
                PostInstallation(packageContents, log);
                log.Log(LogLevel.Info, "Install Complete!", true);
                _localPackageCopy.SetInstallState(true);
            }
            else
            {
                log.Log(LogLevel.Error, "Install Failed! Check logs!", true);
            }
        }

        private void UnInstall()
        {
            if (!_localPackageCopy.IsAlreadyInstalled())
            {
                log.Log(LogLevel.Error, $"Package is not installed!!");
            }

            log.Log(LogLevel.Info, $"Uninstalling...", true);
            PostUnInstallation(packageContents, log);
            log.Log(LogLevel.Info, $"Uninstall Complete!", true);
            _localPackageCopy.SetInstallState(false);
        }

        private void ReInstall()
        {
            if (!_localPackageCopy.IsAlreadyInstalled())
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