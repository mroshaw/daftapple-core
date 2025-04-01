using UnityEditor;
using UnityEditor.Build;
using UnityEngine.Rendering;

namespace DaftAppleGames.BuildingTools.Editor
{
    /// <summary>
    /// Static methods for basic Editor stuff
    /// </summary>
    internal static class EditorTools
    {
        #region Static properties

        [InitializeOnLoadMethod]
        public static void SetRenderPipelineDefine()
        {
            // Get current scripting define symbols for Standalone
            string defines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);

            // Check if HDRP is active
            if (GraphicsSettings.defaultRenderPipeline != null &&
                GraphicsSettings.defaultRenderPipeline.GetType().Name.Contains("HDRenderPipeline"))
            {
                // Add HDRP define if it's not already present
                if (!defines.Contains("DAG_HDRP"))
                {
                    defines += ";DAG_HDRP";
                }
            }
            // Check if URP is active
            else if (GraphicsSettings.defaultRenderPipeline != null &&
                     GraphicsSettings.defaultRenderPipeline.GetType().Name.Contains("UniversalRenderPipeline"))
            {
                // Add URP define if it's not already present
                if (!defines.Contains("DAG_URP"))
                {
                    defines += ";DAG_URP";
                }
            }
            // Check if Built-In Render Pipeline is active
            else if (GraphicsSettings.defaultRenderPipeline == null)
            {
                // Add Built-In define if it's not already present
                if (!defines.Contains("DAG_BIRP"))
                {
                    defines += ";DAG_BIRP";
                }
            }

            // Set the updated define symbols
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, defines);
        }

        #endregion
    }
}