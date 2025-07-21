#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif
using UnityEngine;

namespace DaftAppleGames.GameObjects
{
    [ExecuteInEditMode]
    public class ObjectGuid : MonoBehaviour
    {
        public string Guid { get; private set; }

        private void OnEnable()
        {
            Generate();
        }

        [Button("Generate Now")]
        private void Generate()
        {
            if (string.IsNullOrEmpty(Guid))
            {
                Guid = System.Guid.NewGuid().ToString();
            }
        }
    }
}