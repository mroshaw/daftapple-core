using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.Utilities
{
    /// <summary>
    /// Simple component to unparent a GameObject on start or awake
    /// </summary>
    public class UnparentGameObject : MonoBehaviour
    {
        [BoxGroup("Settings")] [SerializeField] private bool unparentOnAwake;
        [BoxGroup("Settings")] [SerializeField] private bool unparentOnStart;

        private bool _isUnparented;

        private void Awake()
        {
            if (unparentOnAwake && !_isUnparented)
            {
                Unparent();
            }
        }

        private void Start()
        {
            if (unparentOnStart && !_isUnparented)
            {
                Unparent();
            }
        }

        private void Unparent()
        {
            if (transform.parent != null)
            {
                transform.SetParent(null);
                _isUnparented = true;
            }
        }
    }
}