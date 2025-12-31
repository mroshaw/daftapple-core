using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DaftAppleGames.Editor.Package
{
    [Serializable] public class PackageItem
    {
        [SerializeField] private Object itemAsset;
        [SerializeField] internal string itemName;
        [SerializeField] internal string destinationFolder;
        [SerializeField] internal bool overwriteExisting = true;
        [SerializeField] private Object installedItemCopy;
        internal string Name => itemAsset.name;
        internal Object ItemAsset => installedItemCopy;

        internal bool Install(string absoluteInstallPath, string relativeInstallPath, EditorLog log, Action itemInstalledAction = null)
        {
            if (!itemAsset)
            {
                log.LogError("Item asset is missing. Aborting.");
                return false;
            }

            string absoluteDestinationFolderPath = Path.Combine(absoluteInstallPath, destinationFolder);
            if (!Directory.Exists(absoluteDestinationFolderPath))
            {
                log.LogInfo($"Creating folder {absoluteDestinationFolderPath}...");
                AssetDatabase.CreateFolder(relativeInstallPath, destinationFolder);
            }

            // Check to see if the item already exists
            string originalAssetFullPath = AssetDatabase.GetAssetPath(itemAsset);
            string assetFileName = Path.GetFileName(originalAssetFullPath);

            string absoluteDestinationFilePath = Path.Combine(absoluteDestinationFolderPath, assetFileName);

            bool fileAlreadyExists = File.Exists(absoluteDestinationFilePath);

            bool ignoreFile = fileAlreadyExists && !overwriteExisting;
            if (ignoreFile)
            {
                log.LogInfo($"File already exists and overwrite is false. Skipping item: {itemAsset.name}");
            }

            string relativeDestinationFilePath = Path.Combine(relativeInstallPath, destinationFolder, assetFileName);

            // If this is a prefab, instantiate an instance and save it as an asset to the target location
            if (PrefabUtility.IsPartOfPrefabAsset(itemAsset))
            {
                if (!ignoreFile)
                {
                    GameObject prefabInstance = PrefabUtility.InstantiatePrefab(itemAsset) as GameObject;
                    PrefabUtility.SaveAsPrefabAsset(prefabInstance, relativeDestinationFilePath);
                    log.LogInfo("Prefab instance created at: " + originalAssetFullPath);
                    Object.DestroyImmediate(prefabInstance);
                }

                installedItemCopy = AssetDatabase.LoadAssetAtPath(relativeDestinationFilePath, itemAsset.GetType());
                itemInstalledAction?.Invoke();

                return true;
            }

            // Save a copy to the destination
            if (!ignoreFile)
            {
                AssetDatabase.CopyAsset(originalAssetFullPath, relativeDestinationFilePath);
            }

            installedItemCopy = AssetDatabase.LoadAssetAtPath(relativeDestinationFilePath, itemAsset.GetType());
            return true;
        }
    }
}