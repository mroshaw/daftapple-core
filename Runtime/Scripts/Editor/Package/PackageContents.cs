using System.Collections.Generic;
using System.IO;
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
    public abstract class PackageContents : ScriptableObject
    {
        [BoxGroup("Settings")] public string packageName;
        [BoxGroup("Package State")] [SerializeField] public bool allowUninstall;
        [BoxGroup("Package State")] [SerializeField] private bool isInstalled;

        public UnityEvent<bool> onInstallStateChanged;

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

        internal void SetInstallState(bool state)
        {
            isInstalled = state;
            onInstallStateChanged.Invoke(state);
        }

        internal bool IsAlreadyInstalled()
        {
            return isInstalled;
        }

        public bool Install(out List<string> log)
        {
            bool result = InstallPackage(out log);
            if (result)
            {
                isInstalled = true;
            }

            return result;
        }

        protected abstract bool InstallPackage(out List<string> log);

        public bool UnInstall(out List<string> log)
        {
            bool result = UnInstallPackage(out log);
            if (result)
            {
                isInstalled = false;
            }

            return result;
        }

        protected abstract bool UnInstallPackage(out List<string> log);
    }
}