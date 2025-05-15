using System.Collections.Generic;
using DaftAppleGames.Attributes;
using DaftAppleGames.Editor.Extensions;
using UnityEditor;
using UnityEngine;


namespace DaftAppleGames.Editor.Attributes
{
    [CustomPropertyDrawer(typeof(InlineEditorAttribute))]
    public class InlineEditorDrawer : PropertyDrawerBase
    {
        private const string TypeWarningMessage = "{0} must be a scriptable object";
        private const string SharedAssetWarningMessage = "This is a shared asset. Changes will affect all references to this asset.";

        private const float FoldoutWidth = 20f;
        private static readonly float IconSize = EditorGUIUtility.singleLineHeight;
        private const float IconPadding = 2f;

        // Stores foldout states per property
        private static readonly Dictionary<string, bool> FoldoutStates = new();
        private UnityEditor.Editor _inlineEditor;

        protected override float GetPropertyHeight_Internal(SerializedProperty property, GUIContent label)
        {
            float height = EditorGUIUtility.singleLineHeight;

            if (!property.isExpanded || property.objectReferenceValue == null)
            {
                return height;
            }

            UnityEditor.Editor.CreateCachedEditor(property.objectReferenceValue, null, ref _inlineEditor);

            if (_inlineEditor != null)
            {
                SerializedObject so = _inlineEditor.serializedObject;
                SerializedProperty prop = so.GetIterator();
                if (prop.NextVisible(true))
                {
                    do
                    {
                        if (prop.name == "m_Script")
                        {
                            continue;
                        }

                        height += EditorGUI.GetPropertyHeight(prop, true) + EditorGUIUtility.standardVerticalSpacing;
                    } while (prop.NextVisible(false));
                }
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

            EditorGUI.BeginProperty(rect, label, property);

            // Draw property field (with foldout and object picker)
            Rect fieldRect = new(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);

            // Calculate foldout arrow rect
            Rect foldoutRect = new(fieldRect.x, fieldRect.y, 16f, IconSize);
            Rect objectFieldRect = new(fieldRect.x + 16f, fieldRect.y, fieldRect.width - 16f - IconSize - IconPadding, IconSize);
            Rect iconRect = new(rect.xMax - IconSize, rect.y, IconSize, IconSize);

            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, GUIContent.none, true);
            EditorGUI.PropertyField(objectFieldRect, property, label);

            // Show info icon if it's an asset (i.e., not embedded SO)
            if (property.objectReferenceValue != null && AssetDatabase.Contains(property.objectReferenceValue))
            {
                GUIContent infoIcon = EditorGUIUtility.IconContent("console.infoicon");
                infoIcon.tooltip = "This object is an asset. Editing it here will affect all usages.";
                GUI.Label(iconRect, infoIcon);
            }

            // Inline editor for the SO
            if (property.isExpanded && property.objectReferenceValue != null)
            {
                UnityEditor.Editor.CreateCachedEditor(property.objectReferenceValue, null, ref _inlineEditor);

                if (_inlineEditor != null)
                {
                    SerializedObject so = _inlineEditor.serializedObject;
                    so.Update();

                    SerializedProperty prop = so.GetIterator();
                    float yOffset = rect.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

                    EditorGUI.indentLevel++;
                    if (prop.NextVisible(true))
                    {
                        do
                        {
                            if (prop.name == "m_Script")
                            {
                                continue;
                            }

                            float propHeight = EditorGUI.GetPropertyHeight(prop, true);
                            Rect propRect = new(rect.x, yOffset, rect.width, propHeight);
                            EditorGUI.PropertyField(propRect, prop, true);
                            yOffset += propHeight + EditorGUIUtility.standardVerticalSpacing;
                        } while (prop.NextVisible(false));
                    }

                    EditorGUI.indentLevel--;

                    so.ApplyModifiedProperties();
                }
            }

            EditorGUI.EndProperty();
        }

        private bool IsValidProperty()
        {
            System.Type expectedType = fieldInfo.FieldType;
            return typeof(ScriptableObject).IsAssignableFrom(expectedType);
        }
    }
}