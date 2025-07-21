using System.Collections.Generic;
using UnityEngine;

namespace DaftAppleGames.Extensions
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// Wraps up Get and Add to either return a component if it exists, otherwise create and return a new instance
        /// </summary>
        private static Component EnsureComponent(this GameObject gameObject, System.Type componentType)
        {
            Component component = gameObject.GetComponent(componentType);
            if (component)
            {
                return component;
            }

            component = gameObject.AddComponent(componentType);
            return component;
        }

        public static T EnsureComponent<T>(this GameObject gameObject) where T : Component
        {
            return EnsureComponent(gameObject, typeof(T)) as T;
        }

        public static T EnsureComponent<T>(this Component existingComponent) where T : Component
        {
            return EnsureComponent(existingComponent.gameObject, typeof(T)) as T;
        }

        public static T TryGetComponentInChildren<T>(this Component existingComponent, out T component, bool includeInactive) where T : Component
        {
            return existingComponent.gameObject.TryGetComponentInChildren<T>(out component, includeInactive);
        }

        public static T TryGetComponentInChildren<T>(this GameObject gameObject, out T component, bool includeInactive) where T : Component
        {
            component = gameObject.GetComponentInChildren<T>(includeInactive);
            return component;
        }

        /// <summary>
        /// Returns true if this GameObject has an instance of the given Component Type
        /// </summary>
        public static bool HasComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() != null;
        }

        public static bool HasComponent<T>(this Component component) where T : Component
        {
            return HasComponent<T>(component.gameObject);
        }

        /// <summary>
        /// Returns true if this GameObject shares a parent with the given GameObject
        /// </summary>
        public static bool IsParentedBy(this GameObject thisGameObject, GameObject otherGameObject)
        {
            if (!thisGameObject.transform.parent)
            {
                return false;
            }

            return thisGameObject.transform.parent == otherGameObject.transform;
        }

        /// <summary>
        /// Returns true if this GameObject shares a parent with any of the given GameObjects
        /// </summary>
        public static bool IsParentedByAny(this GameObject thisGameObject, GameObject[] otherGameObjects, out GameObject parentGameObject)
        {
            foreach (GameObject otherGameObject in otherGameObjects)
            {
                if (IsParentedBy(thisGameObject, otherGameObject))
                {
                    parentGameObject = otherGameObject;
                    return true;
                }
            }

            parentGameObject = null;
            return false;
        }

        public static GameObject FindChildGameObject(this GameObject fromGameObject, string childName, bool exact = true)
        {
            Transform[] transforms = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
            foreach (Transform currTransform in transforms)
            {
                if ((exact && currTransform.gameObject.name == childName) || (!exact && currTransform.gameObject.name.ToLower().Contains(childName.ToLower())))
                {
                    return currTransform.gameObject;
                }
            }

            return null;
        }

        public static GameObject[] FindChildGameObjects(this GameObject fromGameObject, string childName, bool exact = true)
        {
            List<GameObject> result = new();
            Transform[] transforms = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
            foreach (Transform currTransform in transforms)
            {
                if ((exact && currTransform.gameObject.name == childName) || (!exact && currTransform.gameObject.name.ToLower().Contains(childName.ToLower())))
                {
                    result.Add(currTransform.gameObject);
                }
            }

            return result.ToArray();
        }

        public static bool HasMatchingTag(this GameObject gameObject, string[] tags)
        {
            if (gameObject == null || tags == null || tags.Length == 0)
                return false;

            foreach (string tag in tags)
            {
                if (gameObject.CompareTag(tag))
                    return true;
            }

            return false;
        }
    }
}