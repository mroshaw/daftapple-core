using System;
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
    /// Scriptable Object: TODO Purpose and Summary
    /// </summary>
    [CreateAssetMenu(fileName = "PackageContents", menuName = "Daft Apple Games/Package/Package Contents", order = 1)]
    public class PackageContents : ScriptableObject
    {
        [BoxGroup("Package Contents")] public PackageItem[] packageItems;

        [SerializeField] private bool isInstalled = false;

        [SerializeField] private bool welcomeWindowDisplayed = false;

        public UnityEvent<bool> onInstallStateChanged;

        public bool Install(Action<LogLevel, string> logDelegate, string baseInstallLocation)
        {
            bool installedState = true;

            // Create destination base folder
            if (!Directory.Exists(baseInstallLocation))
            {
                logDelegate(LogLevel.Info, $"Directory does not exist: {baseInstallLocation}. Creating...");
                Directory.CreateDirectory(baseInstallLocation);
            }

            foreach (PackageItem packageItem in packageItems)
            {
                logDelegate(LogLevel.Info, $"Processing item: {packageItem.Name}...");
                installedState = installedState && packageItem.Install(baseInstallLocation, logDelegate);
            }

            return installedState;
        }

        public void UnsetInstallComplete()
        {
            isInstalled = false;
            onInstallStateChanged.Invoke(isInstalled);
            UpdateAssetFile();
        }

        public void SetInstallState(bool state)
        {
            isInstalled = state;
            onInstallStateChanged.Invoke(state);
            UpdateAssetFile();
        }

        public bool IsAlreadyInstalled()
        {
            return isInstalled;
        }

        private void UpdateAssetFile()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }

        public void SetWelcomeWindowDisplayed(bool displayed)
        {
            welcomeWindowDisplayed = displayed;
            UpdateAssetFile();
        }
    }
}