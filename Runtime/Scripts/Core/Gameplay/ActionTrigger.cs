using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace DaftAppleGames.Gameplay
{
    public abstract class ActionTrigger : MonoBehaviour
    {
        #region Class Variables

        [Header("Collider Settings")]
        [Tooltip("Trigger will only fire if the collider has any one of these tags.")] public string[] triggerTags;
        [Tooltip("Trigger will only fire if the collider is on any one of these layers.")] public LayerMask triggerLayers;
        [Tooltip("Colliders marked as triggers will be ignored.")] public bool ignoreTriggers;
        [Header("Events")]
        public UnityEvent<Collider> triggerEnterEvent;

        public UnityEvent<Collider> triggerExitEvent;

        #endregion

        #region Startup

        /// <summary>
        /// Configure the component on awake
        /// </summary>   
        private void Awake()
        {
            if (!GetComponent<Collider>())
            {
                Debug.LogError($"CharacterTrigger: There is no collider on this gameobject! {gameObject}");
            }
        }

        #endregion

        private void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger && ignoreTriggers)
            {
                return;
            }

            if (CollisionIsValid(other))
            {
                TriggerEnter(other);
                triggerEnterEvent.Invoke(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.isTrigger && ignoreTriggers)
            {
                return;
            }

            if (CollisionIsValid(other))
            {
                TriggerExit(other);
                triggerExitEvent.Invoke(other);
            }
        }

        private bool CollisionIsValid(Collider other)
        {
            // Compare tags
            if (triggerTags.Length == 0 || triggerTags.Contains(other.tag))
            {
                // Compare Layers
                if (triggerLayers == 0 || ((1 << other.gameObject.layer) & triggerLayers) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        public abstract void TriggerEnter(Collider other);
        public abstract void TriggerExit(Collider other);
    }
}