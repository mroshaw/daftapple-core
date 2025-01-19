using System.Collections.Generic;
using UnityEngine;

namespace DaftAppleGames.Extensions
{
    public static class GameObjectExtensions
    {
        #region Class Methods

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

    #endregion
}