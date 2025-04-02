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
        [SerializeField] private Object itemAsset;
        [SerializeField] internal string destinationFolder;
        [SerializeField] internal bool overwriteExisting = true;

        internal string Name => itemAsset.name;

        internal bool Install(string absoluteInstallPath, string relativeInstallPath, EditorLog log)
        {
            if (!itemAsset)
            {
                log.Log(LogLevel.Error, "Item asset is missing. Aborting.");
                return false;
            }

            string absoluteDestinationFolderPath = Path.Combine(absoluteInstallPath, destinationFolder);
            if (!Directory.Exists(absoluteDestinationFolderPath))
            {
                log.Log(LogLevel.Info, $"Creating folder {absoluteDestinationFolderPath}...");
                AssetDatabase.CreateFolder(relativeInstallPath, destinationFolder);
            }

            // Check to see if the item already exists
            string originalAssetFullPath = AssetDatabase.GetAssetPath(itemAsset);
            string assetFileName = Path.GetFileName(originalAssetFullPath);

            string absoluteDestinationFilePath = Path.Combine(absoluteDestinationFolderPath, assetFileName);

            bool fileAlreadyExists = File.Exists(absoluteDestinationFilePath);

            if (fileAlreadyExists && !overwriteExisting)
            {
                log.Log(LogLevel.Info, $"File already exists and overwrite is false. Skipping item: {itemAsset.name}");
                return true;
            }

            string relativeDestinationFilePath = Path.Combine(relativeInstallPath, destinationFolder, assetFileName);

            // If this is a prefab, instantiate an instance and save it as an asset to the target location
            if (PrefabUtility.IsPartOfPrefabAsset(itemAsset))
            {
                GameObject prefabInstance = PrefabUtility.InstantiatePrefab(itemAsset) as GameObject;
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, relativeDestinationFilePath);
                log.Log(LogLevel.Info, "Prefab instance created at: " + originalAssetFullPath);
                Object.DestroyImmediate(prefabInstance);
                return true;
            }

            // Save a copy to the destination
            AssetDatabase.CopyAsset(originalAssetFullPath, relativeDestinationFilePath);
            return true;
        }
    }
}