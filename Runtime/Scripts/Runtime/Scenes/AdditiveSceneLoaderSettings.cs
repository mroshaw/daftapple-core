using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.Scenes
{
    [CreateAssetMenu(fileName = "AdditiveSceneLoaderSettings", menuName = "Daft Apple Games/Scenes/Additive Scene Loader Settings", order = 1)]
    public class AdditiveSceneLoaderSettings : ScriptableObject
    {
        [BoxGroup("Loader Scene")] public AdditiveScene loaderScene;
#if ODIN_INSPECTOR
        [BoxGroup("Scenes")] [TableList] public List<AdditiveScene> additiveScenes;
#else
        [BoxGroup("Scenes")] public List<AdditiveScene> additiveScenes;
#endif
#if UNITY_EDITOR
        public List<string> GetAllScenePaths()
        {
            List<string> scenePaths = new();

            scenePaths.Add(loaderScene.GetScenePath());

            foreach (AdditiveScene currScene in additiveScenes)
            {
                scenePaths.Add(currScene.GetScenePath());
            }

            return scenePaths;
        }
#endif
    }
}