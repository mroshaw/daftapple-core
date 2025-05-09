using System;
using DaftAppleGames.Attributes;
using UnityEditor;
using UnityEngine;

namespace DaftAppleGames.Editor.Attributes.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ForceEnumDropdownAttribute))]
    public class ForceEnumDropdownDrawer : PropertyDrawerBase
    {
        private const string TypeWarningMessage = "{0} must be an enum field only";

        protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Enum)
            {
                string message = string.Format(TypeWarningMessage, property.name);
                DrawDefaultPropertyAndHelpBox(rect, property, message, MessageType.Warning);
                return;
            }

            EditorGUI.BeginProperty(rect, label, property);

            Type enumType = fieldInfo.FieldType;
            Array enumValues = Enum.GetValues(enumType);
            int currentIndex = Mathf.Clamp(property.enumValueIndex, 0, enumValues.Length - 1);

            Enum currentEnum = (Enum)enumValues.GetValue(currentIndex);
            Enum selectedEnum = EditorGUI.EnumPopup(rect, label, currentEnum);

            int newIndex = Array.IndexOf(enumValues, selectedEnum);
            property.enumValueIndex = newIndex;

            EditorGUI.EndProperty();
        }
    }
}