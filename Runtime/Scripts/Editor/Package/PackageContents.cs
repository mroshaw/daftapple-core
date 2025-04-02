using System.IO;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.Editor.Package
{
    /// <summary>
    /// Scriptable Object storing package contents and package install state
    /// </summary>
    [CreateAssetMenu(fileName = "PackageContents", menuName = "Daft Apple Games/Package/Package Contents", order = 1)]
    public class PackageContents : ScriptableObject
    {
        [BoxGroup("Settings")] public string packageInstallFolder;
        [BoxGroup("Settings")] public bool allowUninstall;
        [BoxGroup("Package State")] [SerializeField] private bool isInstalled;
        [BoxGroup("Package Contents")] [SerializeField] private PackageItem[] packageItems;

        public UnityEvent<bool> onInstallStateChanged;


        private string PackageFileName => name + ".asset";

        private bool LocalCopyExists(string fullFolderPath)
        {
            string fullPath = Path.Combine(fullFolderPath, PackageFileName);
            return File.Exists(fullPath);
        }

        /// <summary>
        /// Retrieves a local copy at the path, if it exists, or creates and returns a new copy
        /// </summary>
        internal PackageContents GetLocalCopy(string fullFolderPath)
        {
            return LocalCopyExists(fullFolderPath) ? LoadLocalCopy(fullFolderPath) : SaveALocalCopy(fullFolderPath);
        }

        private PackageContents LoadLocalCopy(string fullFolderPath)
        {
            string fullPath = Path.Combine(fullFolderPath, PackageFileName);
            string relativePath = "Assets" + fullPath.Substring(Application.dataPath.Length);
            PackageContents newPackageContents = AssetDatabase.LoadAssetAtPath<PackageContents>(relativePath);
            return newPackageContents;
        }

        private PackageContents SaveALocalCopy(string fullFolderPath)
        {
            if (string.IsNullOrEmpty(fullFolderPath) || !Directory.Exists(fullFolderPath))
            {
                return this;
            }

            string fullPath = Path.Combine(fullFolderPath, PackageFileName);
            string relativePath = "Assets" + fullPath.Substring(Application.dataPath.Length);

            PackageContents newPackageContents = Instantiate(this);
            AssetDatabase.CreateAsset(newPackageContents, relativePath);
            return newPackageContents;
        }

        /// <summary>
        /// Installs the package
        /// </summary>
        internal bool Install(string fullFolderPath, EditorLog log)
        {
            bool installedState = true;

            string packageFolderPath = Path.Combine(fullFolderPath, packageInstallFolder);

            // Create destination base folder
            if (!Directory.Exists(packageFolderPath))
            {
                log.Log(LogLevel.Info, $"Directory does not exist: {packageFolderPath}. Creating...");
                Directory.CreateDirectory(packageFolderPath);
            }

            foreach (PackageItem packageItem in packageItems)
            {
                log.Log(LogLevel.Info, $"Processing item: {packageItem.Name}...");
                installedState = installedState && packageItem.Install(packageFolderPath, log);
            }

            return installedState;
        }

        internal void SetInstallState(bool state)
        {
            isInstalled = state;
            onInstallStateChanged.Invoke(state);
            UpdateAssetFile();
        }

        internal bool IsAlreadyInstalled()
        {
            return isInstalled;
        }

        private void UpdateAssetFile()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }
    }
}