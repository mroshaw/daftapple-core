using System;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DaftAppleGames.Scenes
{
    [Serializable]
    public class AdditiveScene
    {
        [TableColumnWidth(120, Resizable = true)] public string sceneName;
#if UNITY_EDITOR
        [TableColumnWidth(180, Resizable = true)][OnValueChanged("UpdateName")] public SceneAsset sceneAsset;
        [TableColumnWidth(20, Resizable = true)] [Button("U")] [LabelText("U")]
        private void UpdateName()
        {
            sceneName = sceneAsset.name;
        }
#endif
        [Tooltip("Is this is the main scene in the list?")][TableColumnWidth(90, Resizable = false)] public bool isMain;

        [Button("Load")]
        private void Load()
        {
            Debug.Log($"Loading: {sceneName}");
        }

        private AsyncOperation _sceneOp;
        private SceneLoadStatus _loadStatus = SceneLoadStatus.Idle;

        public AsyncOperation SceneOp
        {
            get => _sceneOp;
            set => _sceneOp = value;
        }

        public SceneLoadStatus LoadStatus
        {
            get => _loadStatus;
            set => _loadStatus = value;
        }
#if UNITY_EDITOR
        public string GetScenePath()
        {
            return AssetDatabase.GetAssetPath(sceneAsset);
        }
#endif
    }
}