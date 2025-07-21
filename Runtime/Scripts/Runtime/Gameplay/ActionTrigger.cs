using System.Linq;
using UnityEngine;
using UnityEngine.Events;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using DaftAppleGames.Attributes;
#endif

namespace DaftAppleGames.Gameplay
{
    /// <summary>
    /// Provides an abstract base class for components that respond to colliders with given tags and layers entering and exiting a collider
    /// on the component game object
    /// </summary>
    public abstract class ActionTrigger : MonoBehaviour
    {
        [BoxGroup("Collider Settings")] [Tooltip("Trigger will only fire if the collider has any one of these tags.")] [SerializeField] private string[] triggerTags;
        [BoxGroup("Collider Settings")] [Tooltip("Trigger will only fire if the collider is on any one of these layers.")] [SerializeField] protected LayerMask triggerLayerMask;
        [BoxGroup("Collider Settings")] [Tooltip("Colliders marked as triggers will be ignored.")] [SerializeField] protected bool ignoreTriggers;
        [BoxGroup("Events")] public UnityEvent<Collider> triggerEnterEvent;
        [BoxGroup("Events")] public UnityEvent<Collider> triggerExitEvent;

        public string[] TriggerTags
        {
            set => triggerTags = value;
        }

        public LayerMask TriggerLayerMask
        {
            set => triggerLayerMask = value;
        }

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

        private void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger && ignoreTriggers)
            {
                return;
            }

            if (!CollisionIsValid(other))
            {
                return;
            }

            TriggerEnter(other);
            triggerEnterEvent.Invoke(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.isTrigger && ignoreTriggers)
            {
                return;
            }

            if (!CollisionIsValid(other))
            {
                return;
            }

            TriggerExit(other);
            triggerExitEvent.Invoke(other);
        }

        private bool CollisionIsValid(Collider other)
        {
            // Compare tags
            if (triggerTags.Length != 0 && !triggerTags.Contains(other.tag))
            {
                return false;
            }

            // Compare Layers
            return triggerLayerMask == 0 || ((1 << other.gameObject.layer) & triggerLayerMask) != 0;
        }

        protected abstract void TriggerEnter(Collider other);
        protected abstract void TriggerExit(Collider other);
    }
}