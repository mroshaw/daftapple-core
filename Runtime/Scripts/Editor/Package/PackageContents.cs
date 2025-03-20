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
        [BoxGroup("Package Contents")] internal PackageItem[] packageItems;

        [SerializeField] private bool isInstalled = false;

        [SerializeField] private bool welcomeWindowDisplayed = false;

        public UnityEvent<bool> onInstallStateChanged;

        internal bool Install(Action<LogLevel, string> logDelegate, string baseInstallLocation)
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

        internal void UnsetInstallComplete()
        {
            isInstalled = false;
            onInstallStateChanged.Invoke(isInstalled);
            UpdateAssetFile();
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

        internal void SetWelcomeWindowDisplayed(bool displayed)
        {
            welcomeWindowDisplayed = displayed;
            UpdateAssetFile();
        }
    }
}