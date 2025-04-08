#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace DaftAppleGames.Extensions
{
    public static class UndoExtensions
    {
        /// <summary>
        /// Returns a component if it exists, otherwise adds a new one wrapped in an Undo
        /// </summary>
        public static bool EnsureComponent<T>(this Undo undo, GameObject gameObject) where T : Component
        {
            return gameObject.TryGetComponent<T>(out T component) ? component : Undo.AddComponent<T>(gameObject);
        }
    }
}