using System;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DaftAppleGames.Editor.Package
{
    [Serializable]
    internal class PackageItem
    {
        internal Object itemAsset;
        internal string itemDestinationPath;
        internal bool overwriteExisting = true;

        internal string Name => itemAsset.name;

        internal bool Install(string basePath, EditorLog log)
        {
            if (!itemAsset)
            {
                log.Log(LogLevel.Error, "Item asset is missing. Aborting.");
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
                log.Log(LogLevel.Error, $"The destination base path for the package does not exit: {basePath}");
                return false;
            }

            if (!Directory.Exists(fullDestinationFolderPath))
            {
                // Try to create the folder
                Directory.CreateDirectory(fullDestinationFolderPath);
            }

            if (fileAlreadyExists && !overwriteExisting)
            {
                log.Log(LogLevel.Info, $"File already exists and overwrite is false. Skipping item: {itemAsset.name}");
                return true;
            }

            // If this is a prefab, instantiate an instance and save it as an asset to the target location
            if (PrefabUtility.IsPartOfPrefabAsset(itemAsset))
            {
                GameObject prefabInstance = PrefabUtility.InstantiatePrefab(itemAsset) as GameObject;
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, fullDestinationFilePath);
                log.Log(LogLevel.Info, "Prefab instance created at: " + originalAssetFullPath);
                Object.DestroyImmediate(prefabInstance);
                return true;
            }

            // Save a copy to the destination
            AssetDatabase.CopyAsset(originalAssetFullPath, fullDestinationFilePath);
            return true;
        }
    }
}