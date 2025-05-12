using System.Collections.Generic;
using DaftAppleGames.Attributes;
using UnityEditor;
using UnityEngine;

namespace DaftAppleGames.Editor.Attributes
{
    [CustomPropertyDrawer(typeof(InlineEditorAttribute))]
    public class InlineEditorDrawer : PropertyDrawerBase
    {
        private const string TypeWarningMessage = "{0} must be a scriptable object";

        // Stores foldout states per property
        private static readonly Dictionary<string, bool> FoldoutStates = new();
        private UnityEditor.Editor _inlineEditor;

        protected override float GetPropertyHeight_Internal(SerializedProperty property, GUIContent label)
        {
            string key = property.propertyPath;
            float height = IsValidProperty() ? GetPropertyHeight(property) : GetPropertyHeight(property) + GetHelpBoxHeight();

            if (FoldoutStates.TryGetValue(key, out bool expanded) && expanded && property.objectReferenceValue != null)
            {
                // We let GUILayout handle vertical space, so return extra height for spacing only
                height += EditorGUIUtility.standardVerticalSpacing * 2f;
            }

            return height;
        }

        protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
        {
            if (!IsValidProperty())
            {
                EditorGUI.BeginProperty(rect, label, property);
                string message = string.Format(TypeWarningMessage, property.name);
                DrawDefaultPropertyAndHelpBox(rect, property, message, MessageType.Warning);
                EditorGUI.EndProperty();
                return;
            }

            string key = property.propertyPath;

            // Ensure state exists for this property
            if (!FoldoutStates.ContainsKey(key))
            {
                FoldoutStates[key] = false;
            }

            // Draw the object field with a foldout toggle
            Rect foldoutRect = new(rect.x, rect.y, 20f, EditorGUIUtility.singleLineHeight);
            Rect objectFieldRect = new(rect.x + 20f, rect.y, rect.width - 20f, EditorGUIUtility.singleLineHeight);

            FoldoutStates[key] = EditorGUI.Foldout(foldoutRect, FoldoutStates[key], GUIContent.none);
            EditorGUI.BeginProperty(rect, label, property);
            property.objectReferenceValue = EditorGUI.ObjectField(objectFieldRect, label, property.objectReferenceValue, fieldInfo.FieldType, false);
            EditorGUI.EndProperty();

            // Draw the inline editor only if expanded and not null
            if (FoldoutStates[key] && property.objectReferenceValue != null)
            {
                EditorGUI.indentLevel++;

                // Create or reuse the cached editor
                if (_inlineEditor == null || _inlineEditor.target != property.objectReferenceValue)
                {
                    UnityEditor.Editor.CreateCachedEditor(property.objectReferenceValue, null, ref _inlineEditor);
                }

                if (_inlineEditor != null)
                {
                    EditorGUILayout.Space(4);
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    _inlineEditor.OnInspectorGUI();
                    EditorGUILayout.EndVertical();
                }

                EditorGUI.indentLevel--;
            }
        }

        private bool IsValidProperty()
        {
            System.Type expectedType = fieldInfo.FieldType;
            return typeof(ScriptableObject).IsAssignableFrom(expectedType);
        }
    }
}