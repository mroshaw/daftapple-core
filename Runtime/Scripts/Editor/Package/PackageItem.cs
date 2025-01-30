using System;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DaftAppleGames.Editor.Package
{
    [Serializable]
    public class PackageItem
    {
        public Object itemAsset;
        public string itemDestinationPath;
        public bool overwriteExisting = true;

        public string Name => itemAsset.name;

        public bool Install(string basePath, Action<LogLevel, string> logDelegate)
        {
            if (!itemAsset)
            {
                logDelegate(LogLevel.Error, "Item asset is missing. Aborting.");
                return false;
            }

            // Check to see if the item already exists
            string originalAssetFullPath = AssetDatabase.GetAssetPath(itemAsset);
            string assetFileName = Path.GetFileName(originalAssetFullPath);
            string fullDestinationFolderPath = Path.Combine(basePath, itemDestinationPath);
            string fullDestinationFilePath = Path.Combine(fullDestinationFolderPath, assetFileName);

            bool fileAlreadyExists = File.Exists(fullDestinationFilePath);

            // Check destination path exists
            if (!Directory.Exists(basePath))
            {
                logDelegate(LogLevel.Error, $"The destination base path for the package does not exit: {basePath}");
                return false;
            }

            if (!Directory.Exists(fullDestinationFolderPath))
            {
                // Try to create the folder
                Directory.CreateDirectory(fullDestinationFolderPath);
            }

            if (fileAlreadyExists && !overwriteExisting)
            {
                logDelegate(LogLevel.Info, $"File already exists and overwrite is false. Skipping item: {itemAsset.name}");
                return true;
            }

            // If this is a prefab, instantiate an instance and save it as an asset to the target location
            if (PrefabUtility.IsPartOfPrefabAsset(itemAsset))
            {
                GameObject prefabInstance = PrefabUtility.InstantiatePrefab(itemAsset) as GameObject;
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, fullDestinationFilePath);
                logDelegate(LogLevel.Info, "Prefab instance created at: " + originalAssetFullPath);
                Object.DestroyImmediate(prefabInstance);
                return true;
            }

            // Save a copy to the destination
            AssetDatabase.CopyAsset(originalAssetFullPath, fullDestinationFilePath);
            return true;
        }
    }
}