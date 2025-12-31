using System.Collections.Generic;
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

        private static string RelativePackageInstallFolder => Path.Combine("Assets", BaseInstallFolderName, "Packages");
        private static string AbsolutePackageInstallFolder => Path.Combine(Application.dataPath, BaseInstallFolderName, "Packages");
        private static string AbsoluteBaseInstallFolder => Path.Combine("Assets", BaseInstallFolderName);
        private static string RelativeBaseInstallFolder => Path.Combine(Application.dataPath, BaseInstallFolderName);

        private Button _installButton;
        private Button _unInstallButton;
        private Button _reInstallButton;
        private Button _clearLogButton;

        private Toggle _showAtStartupToggle;

        private PackageContents _localPackageCopy;

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
            if (!Directory.Exists(RelativeBaseInstallFolder))
            {
                AssetDatabase.CreateFolder("Assets", BaseInstallFolderName);
            }

            // Create a packages subfolder for copies of the package content scriptable objects
            if (!Directory.Exists(AbsolutePackageInstallFolder))
            {
                AssetDatabase.CreateFolder(AbsoluteBaseInstallFolder, "Packages");
            }

            if (!packageContents)
            {
                LogError("No package contents found for installer to use!");
            }

            // If the copy of the package already exists, use that from now on
            _localPackageCopy = packageContents.GetLocalCopy(AbsolutePackageInstallFolder, RelativePackageInstallFolder);
        }

        private void Install()
        {
            Install(false);
        }

        private void Install(bool force)
        {
            if (!force && _localPackageCopy.IsAlreadyInstalled())
            {
                LogError("Already installed!");
                return;
            }

            LogInfo( "Installing... ");
            bool installResult = _localPackageCopy.Install(out List<string> installLogs);
            LogInfo(installLogs);
            LogInfo( installResult ? "Install complete!" : "Install failed! Check logs!");
            CustomEditorTools.SaveChangesToAsset(_localPackageCopy);
        }

        private void UnInstall()
        {
            if (!_localPackageCopy.IsAlreadyInstalled())
            {
                LogError("Package is not installed!!");
            }

            LogInfo("Uninstalling...");
            bool installResult = _localPackageCopy.UnInstall(out List<string> uninstallLogs);
            LogInfo(uninstallLogs);
            LogInfo(installResult ? "Uninstall complete!" : "Install failed! Check logs!");
        }

        private void ReInstall()
        {
            if (!_localPackageCopy.IsAlreadyInstalled())
            {
                LogError("Package is not installed!!");
            }

            LogInfo("Reinstalling...");
            Install(true);
            LogInfo("Reinstall Complete!");
        }

        /// <summary>
        /// Toggles the button states
        /// </summary>
        private void SetButtonState(bool installedState)
        {
            _installButton.SetEnabled(!installedState);
            _unInstallButton.SetEnabled(installedState);
            _reInstallButton.SetEnabled(installedState);
        }
    }
}