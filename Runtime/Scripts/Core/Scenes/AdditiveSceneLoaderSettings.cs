using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace DaftAppleGames.Scenes
{
    [CreateAssetMenu(fileName = "AdditiveSceneLoaderSettings", menuName = "Daft Apple Games/Scenes/Additive Scene Loader Settings", order = 1)]
    public class AdditiveSceneLoaderSettings : ScriptableObject
    {
        [BoxGroup("Loader Scene")] public AdditiveScene loaderScene;
        [BoxGroup("Scenes")] [TableList] public List<AdditiveScene> additiveScenes;
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