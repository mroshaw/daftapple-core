using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace DaftAppleGames.Editor.Extensions
{
    public static class VisualElementExtensions
    {
        /// <summary>
        /// Iterate over all elements in the parent and applies the provided Action
        /// </summary>
        public static void IterateHierarchy(this VisualElement visualElement, Action<VisualElement> action)
        {
            Stack<VisualElement> stack = new();
            stack.Push(visualElement);

            while (stack.Count > 0)
            {
                VisualElement currentElement = stack.Pop();
                for (int i = 0; i < currentElement.hierarchy.childCount; i++)
                {
                    VisualElement child = currentElement.hierarchy.ElementAt(i);
                    stack.Push(child);
                    action.Invoke(child);
                }
            }
        }

        /// <summary>
        /// Sets all child elements interactable state
        /// </summary>
        public static void SetInteractableState(this VisualElement visualElement, bool isInteractable)
        {
            visualElement.pickingMode = isInteractable ? PickingMode.Position : PickingMode.Ignore;
            visualElement.IterateHierarchy(delegate(VisualElement childElement) { childElement.pickingMode = isInteractable ? PickingMode.Position : PickingMode.Ignore; });
        }
    }
}