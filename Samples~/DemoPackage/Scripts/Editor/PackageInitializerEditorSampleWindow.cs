using UnityEditor;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.Editor
{
    public class PackageInitializerEditorSampleWindow : PackageInitializerEditorWindow
    {
        [MenuItem("Daft Apple Games/Packages/Sample Package Initializer")]
        public static void ShowWindow()
        {
            PackageInitializerEditorSampleWindow packageInitWindow = GetWindow<PackageInitializerEditorSampleWindow>();
            packageInitWindow.titleContent = new GUIContent("Sample Package");
        }

        protected override string GetIntroText()
        {
            return "Welcome to the Daft Apple Games sample package!";
        }

        protected override string GetBaseInstallLocation()
        {
            return "Assets/Sample Package";
        }
    }
}