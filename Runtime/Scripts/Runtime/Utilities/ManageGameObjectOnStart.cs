using DaftAppleGames.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace DaftAppleGames.Utilities
{
    public class ManageGameObjectOnStart : MonoBehaviour
    {
        [BoxGroup("Settings")] [SerializeField] private bool disableOnAwake = false;
        [BoxGroup("Settings")] [SerializeField] private bool disableOnStart = false;
        [BoxGroup("Settings")] [SerializeField] private bool destroyOnAwake = false;
        [BoxGroup("Settings")] [SerializeField] private bool destroyOnStart = false;
        [BoxGroup("Settings")] [SerializeField] private GameObject targetGameObject = null;

        [BoxGroup("Events")] public UnityEvent objectDisabledEvent;
        [BoxGroup("Events")] public UnityEvent objectDestroyedEvent;

        private void Awake()
        {
            if (!targetGameObject)
            {
                targetGameObject = gameObject;
            }

            if (disableOnAwake)
            {
                DisableTargetGameObject();
            }

            if (destroyOnAwake)
            {
                DestroyTargetGameObject();
            }
        }

        private void Start()
        {
            if (disableOnStart)
            {
                DisableTargetGameObject();
            }

            if (destroyOnStart)
            {
                DestroyTargetGameObject();
            }
        }

        private void DisableTargetGameObject()
        {
            if (targetGameObject)
            {
                targetGameObject.SetActive(false);
                objectDisabledEvent?.Invoke();
            }
        }

        private void DestroyTargetGameObject()
        {
            if (targetGameObject)
            {
                Destroy(targetGameObject);
                objectDestroyedEvent?.Invoke();
            }
        }

    }
}