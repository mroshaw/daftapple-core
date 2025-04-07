using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DaftAppleGames.Attributes;
using UnityEditor;
using UnityEngine;

namespace DaftAppleGames.Editor.Attributes
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Object), true)]
    public sealed class DaftAppleInspector : UnityEditor.Editor
    {
        private List<SerializedProperty> _serializedProperties = new();
        private IEnumerable<FieldInfo> _nonSerializedFields;
        private IEnumerable<PropertyInfo> _nativeProperties;
        private IEnumerable<MethodInfo> _methods;
        private readonly Dictionary<string, SavedBool> _foldouts = new();

        private void OnEnable()
        {
            _nonSerializedFields = ReflectionUtility.GetAllFields(
                target, f => f.GetCustomAttributes(typeof(ShowNonSerializedFieldAttribute), true).Length > 0);

            _nativeProperties = ReflectionUtility.GetAllProperties(
                target, p => p.GetCustomAttributes(typeof(ShowNativePropertyAttribute), true).Length > 0);

            _methods = ReflectionUtility.GetAllMethods(
                target, m => m.GetCustomAttributes(typeof(ButtonAttribute), true).Length > 0);
        }

        private void OnDisable()
        {
            ReorderableListPropertyDrawer.Instance.ClearCache();
        }

        public override void OnInspectorGUI()
        {
            GetSerializedProperties(ref _serializedProperties);

            bool anyCustomAttribute = _serializedProperties.Any(p => PropertyUtility.GetAttribute<IAttribute>(p) != null);
            if (!anyCustomAttribute)
            {
                DrawDefaultInspector();
            }
            else
            {
                DrawSerializedProperties();
            }

            DrawNonSerializedFields();
            DrawNativeProperties();
            DrawButtons();
        }

        private void GetSerializedProperties(ref List<SerializedProperty> outSerializedProperties)
        {
            outSerializedProperties.Clear();
            using SerializedProperty iterator = serializedObject.GetIterator();
            if (!iterator.NextVisible(true))
            {
                return;
            }

            do
            {
                outSerializedProperties.Add(serializedObject.FindProperty(iterator.name));
            } while (iterator.NextVisible(false));
        }

        private void DrawSerializedProperties()
        {
            serializedObject.Update();

            // Draw non-grouped serialized properties
            foreach (SerializedProperty property in GetNonGroupedProperties(_serializedProperties))
            {
                if (property.name.Equals("m_Script", System.StringComparison.Ordinal))
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.PropertyField(property);
                    }
                }
                else
                {
                    DaftAppleEditorGUI.PropertyField_Layout(property, true);
                }
            }

            // Draw grouped serialized properties
            foreach (IGrouping<string, SerializedProperty> group in GetGroupedProperties(_serializedProperties))
            {
                IEnumerable<SerializedProperty> visibleProperties = group.Where(PropertyUtility.IsVisible);
                IEnumerable<SerializedProperty> serializedProperties = visibleProperties as SerializedProperty[] ?? visibleProperties.ToArray();
                if (!serializedProperties.Any())
                {
                    continue;
                }

                DaftAppleEditorGUI.BeginBoxGroup_Layout(group.Key);
                foreach (SerializedProperty property in serializedProperties)
                {
                    DaftAppleEditorGUI.PropertyField_Layout(property, true);
                }

                DaftAppleEditorGUI.EndBoxGroup_Layout();
            }

            // Draw foldout serialized properties
            foreach (IGrouping<string, SerializedProperty> group in GetFoldoutProperties(_serializedProperties))
            {
                IEnumerable<SerializedProperty> visibleProperties = group.Where(PropertyUtility.IsVisible);
                IEnumerable<SerializedProperty> serializedProperties = visibleProperties as SerializedProperty[] ?? visibleProperties.ToArray();
                if (!serializedProperties.Any())
                {
                    continue;
                }

                if (!_foldouts.ContainsKey(group.Key))
                {
                    _foldouts[group.Key] = new SavedBool($"{target.GetInstanceID()}.{group.Key}", false);
                }

                _foldouts[group.Key].Value = EditorGUILayout.Foldout(_foldouts[group.Key].Value, group.Key, true);
                if (!_foldouts[group.Key].Value)
                {
                    continue;
                }

                foreach (SerializedProperty property in serializedProperties)
                {
                    DaftAppleEditorGUI.PropertyField_Layout(property, true);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawNonSerializedFields(bool drawHeader = false)
        {
            if (!_nonSerializedFields.Any())
            {
                return;
            }

            if (drawHeader)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Non-Serialized Fields", GetHeaderGUIStyle());
                DaftAppleEditorGUI.HorizontalLine(
                    EditorGUILayout.GetControlRect(false), HorizontalLineAttribute.DefaultHeight, HorizontalLineAttribute.DefaultColor.GetColor());
            }

            foreach (FieldInfo field in _nonSerializedFields)
            {
                DaftAppleEditorGUI.NonSerializedField_Layout(serializedObject.targetObject, field);
            }
        }

        private void DrawNativeProperties(bool drawHeader = false)
        {
            if (!_nativeProperties.Any())
            {
                return;
            }

            if (drawHeader)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Native Properties", GetHeaderGUIStyle());
                DaftAppleEditorGUI.HorizontalLine(
                    EditorGUILayout.GetControlRect(false), HorizontalLineAttribute.DefaultHeight, HorizontalLineAttribute.DefaultColor.GetColor());
            }

            foreach (PropertyInfo property in _nativeProperties)
            {
                DaftAppleEditorGUI.NativeProperty_Layout(serializedObject.targetObject, property);
            }
        }

        private void DrawButtons(bool drawHeader = false)
        {
            if (!_methods.Any())
            {
                return;
            }

            if (drawHeader)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Buttons", GetHeaderGUIStyle());
                DaftAppleEditorGUI.HorizontalLine(
                    EditorGUILayout.GetControlRect(false), HorizontalLineAttribute.DefaultHeight, HorizontalLineAttribute.DefaultColor.GetColor());
            }

            foreach (MethodInfo method in _methods)
            {
                DaftAppleEditorGUI.Button(serializedObject.targetObject, method);
            }
        }

        private static IEnumerable<SerializedProperty> GetNonGroupedProperties(IEnumerable<SerializedProperty> properties)
        {
            return properties.Where(p => PropertyUtility.GetAttribute<IGroupAttribute>(p) == null);
        }

        private static IEnumerable<IGrouping<string, SerializedProperty>> GetGroupedProperties(IEnumerable<SerializedProperty> properties)
        {
            return properties
                .Where(p => PropertyUtility.GetAttribute<BoxGroupAttribute>(p) != null)
                .GroupBy(p => PropertyUtility.GetAttribute<BoxGroupAttribute>(p).Name);
        }

        private static IEnumerable<IGrouping<string, SerializedProperty>> GetFoldoutProperties(IEnumerable<SerializedProperty> properties)
        {
            return properties
                .Where(p => PropertyUtility.GetAttribute<FoldoutGroupAttribute>(p) != null)
                .GroupBy(p => PropertyUtility.GetAttribute<FoldoutGroupAttribute>(p).Name);
        }

        private static GUIStyle GetHeaderGUIStyle()
        {
            GUIStyle style = new(EditorStyles.centeredGreyMiniLabel)
            {
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter
            };

            return style;
        }
    }
}