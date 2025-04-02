using System.IO;
using System.Net;
using Codice.Client.Commands.WkTree;
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
        [BoxGroup("Settings")] public string packageName;
        [BoxGroup("Settings")] public string packageInstallFolder;
        [BoxGroup("Settings")] public bool allowUninstall;

        [BoxGroup("Package State")] [SerializeField] private bool isInstalled;
        [BoxGroup("Package Contents")] [SerializeField] private PackageItem[] packageItems;

        public UnityEvent<bool> onInstallStateChanged;

        public bool GetPackageObjectByName<T>(string itemName, out T assetObject)
        {
            foreach (PackageItem packageItem in packageItems)
            {
                if (packageItem.itemName.Equals(itemName))
                {
                    if (packageItem.ItemAsset is T asset)
                    {
                        assetObject = asset;
                        return true;
                    }
                }
            }

            assetObject = default;
            return false;
        }

        private string PackageCopyFileName => packageName + ".asset";

        /// <summary>
        /// Given a base path return the specific package file and filename
        /// </summary>
        private string GetPackageDataFilePath(string baseInstallFolderPath)
        {
            return Path.Combine(baseInstallFolderPath, PackageCopyFileName);
        }

        private bool LocalCopyExists(string fullFolderPath)
        {
            return File.Exists(GetPackageDataFilePath(fullFolderPath));
        }

        /// <summary>
        /// Retrieves a local copy at the path, if it exists, or creates and returns a new copy
        /// </summary>
        internal PackageContents GetLocalCopy(string absoluteFolderPath, string relativeFolderPath)
        {
            return LocalCopyExists(absoluteFolderPath) ? LoadLocalCopy(relativeFolderPath) : SaveALocalCopy(absoluteFolderPath, relativeFolderPath);
        }

        private PackageContents LoadLocalCopy(string relativeFolderPath)
        {
            string relativePath = GetPackageDataFilePath(relativeFolderPath);
            PackageContents newPackageContents = AssetDatabase.LoadAssetAtPath<PackageContents>(relativePath);
            return newPackageContents;
        }

        private PackageContents SaveALocalCopy(string absoluteFolderPath, string relativeFolderPath)
        {
            if (string.IsNullOrEmpty(absoluteFolderPath) || string.IsNullOrEmpty(relativeFolderPath) || !Directory.Exists(absoluteFolderPath))
            {
                return this;
            }

            string relativeFilePath = GetPackageDataFilePath(relativeFolderPath);

            PackageContents newPackageContents = Instantiate(this);
            AssetDatabase.CreateAsset(newPackageContents, relativeFilePath);
            CustomEditorTools.SaveChangesToAsset(newPackageContents);
            return LoadLocalCopy(relativeFolderPath);
        }

        /// <summary>
        /// Installs the package
        /// </summary>
        internal bool Install(string absoluteFolderPath, string relativeFolderPath, EditorLog log)
        {
            bool installedState = true;

            // Create destination base folder
            if (!Directory.Exists(absoluteFolderPath))
            {
                log.Log(LogLevel.Info, $"Directory does not exist. Creating folder {absoluteFolderPath}...");
                AssetDatabase.CreateFolder(relativeFolderPath, packageInstallFolder);
            }

            foreach (PackageItem packageItem in packageItems)
            {
                log.Log(LogLevel.Info, $"Processing item: {packageItem.Name}...");
                installedState = installedState && packageItem.Install(absoluteFolderPath, relativeFolderPath, log);
            }

            return installedState;
        }

        internal void SetInstallState(bool state)
        {
            isInstalled = state;
            onInstallStateChanged.Invoke(state);
        }

        internal bool IsAlreadyInstalled()
        {
            return isInstalled;
        }
    }
}