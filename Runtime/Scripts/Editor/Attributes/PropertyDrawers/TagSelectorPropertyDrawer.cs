﻿using System.Collections.Generic;
using DaftAppleGames.Attributes;
using UnityEditor;
using UnityEngine;

namespace DaftAppleGames.Editor.Attributes
{
    [CustomPropertyDrawer(typeof(TagSelectorAttribute))]
    public class TagSelectorPropertyDrawer : PropertyDrawerBase
    {
        protected override float GetPropertyHeight_Internal(SerializedProperty property, GUIContent label)
        {
            return property.propertyType == SerializedPropertyType.String
                ? GetPropertyHeight(property)
                : GetPropertyHeight(property) + GetHelpBoxHeight();
        }

        protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(rect, label, property);

            if (property.propertyType == SerializedPropertyType.String)
            {
                // generate the taglist + custom tags
                List<string> tagList = new()
                {
                    "(None)",
                    "Untagged"
                };
                tagList.AddRange(UnityEditorInternal.InternalEditorUtility.tags);

                string propertyString = property.stringValue;
                int index = 0;
                // check if there is an entry that matches the entry and get the index
                // we skip index 0 as that is a special custom case
                for (int i = 1; i < tagList.Count; i++)
                {
                    if (!tagList[i].Equals(propertyString, System.StringComparison.Ordinal))
                    {
                        continue;
                    }

                    index = i;
                    break;
                }

                // Draw the popup box with the current selected index
                int newIndex = EditorGUI.Popup(rect, label.text, index, tagList.ToArray());

                // Adjust the actual string value of the property based on the selection
                string newValue = newIndex > 0 ? tagList[newIndex] : string.Empty;

                if (!property.stringValue.Equals(newValue, System.StringComparison.Ordinal))
                {
                    property.stringValue = newValue;
                }
            }
            else
            {
                string message = $"{nameof(TagSelectorAttribute)} supports only string fields";
                DrawDefaultPropertyAndHelpBox(rect, property, message, MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }
    }
}