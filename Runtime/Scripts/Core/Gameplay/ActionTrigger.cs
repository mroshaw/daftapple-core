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
    public abstract class ActionTrigger : MonoBehaviour
    {
        #region Class Variables

        [BoxGroup("Collider Settings")] [Tooltip("Trigger will only fire if the collider has any one of these tags.")] [SerializeField] private string[] triggerTags;
        [Tooltip("Trigger will only fire if the collider is on any one of these layers.")] [SerializeField] protected LayerMask triggerLayerMask;
        [Tooltip("Colliders marked as triggers will be ignored.")] [SerializeField] protected bool ignoreTriggers;
        [BoxGroup("Events")] public UnityEvent<Collider> triggerEnterEvent;
        [BoxGroup("Events")] public UnityEvent<Collider> triggerExitEvent;

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
                if (triggerLayerMask == 0 || ((1 << other.gameObject.layer) & triggerLayerMask) != 0)
                {
                    return true;
                }
            }

            return false;
        }

        public abstract void TriggerEnter(Collider other);
        public abstract void TriggerExit(Collider other);

        #region Unity Editor methods
#if UNITY_EDITOR
        public virtual void ConfigureInEditor(LayerMask newTriggerLayerMask, string[] newTriggerTags)
        {
            triggerLayerMask = newTriggerLayerMask;
            triggerTags = newTriggerTags;
        }
#endif
        #endregion
    }
}