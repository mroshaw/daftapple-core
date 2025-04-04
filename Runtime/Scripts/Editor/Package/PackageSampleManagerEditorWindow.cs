using System.IO;
using DaftAppleGames.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DaftAppleGames.Editor
{
    // Simple Editor tool to sync Sample folders for various assets
    public class PackageSampleManagerEditorWindow : BaseEditorWindow
    {
        protected override string ToolTitle => "Package Sample Manager";

        private Button _deploySamplesBuildingTools;

        [MenuItem("Daft Apple Games/Tools/Package Sample Manager")]
        public static void ShowWindow()
        {
            PackageSampleManagerEditorWindow editorWindow = GetWindow<PackageSampleManagerEditorWindow>();
            editorWindow.titleContent = new GUIContent("Package Sample Manager");
        }

        protected void OnGUI()
        {
            _deploySamplesBuildingTools = rootVisualElement.Q<Button>("DeployBuildingToolsSamplesButton");
            if (_deploySamplesBuildingTools != null)
            {
                _deploySamplesBuildingTools.clicked -= DeployBuildingToolsSample;
                _deploySamplesBuildingTools.clicked += DeployBuildingToolsSample;
            }
        }

        private void DeployBuildingToolsSample()
        {
            string sourcePath = "E:\\Dev\\DAG\\DAG Framework\\Assets\\SampleMASTER\\BuildingTools";
            string destPath = Path.Combine("E:\\Dev\\DAG\\DAG Framework\\Assets\\building-tools");

            string destSamplePath = Path.Combine(destPath, "Samples~");

            log.Log(LogLevel.Info, $"Copying files from {sourcePath} tp {destSamplePath}");

            DirectoryInfo sourceDirectory = new(sourcePath);
            sourceDirectory.DeepCopy(destSamplePath);

            log.Log(LogLevel.Info, "Done!");
        }
    }
}