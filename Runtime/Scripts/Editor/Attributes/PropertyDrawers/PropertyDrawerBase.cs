using DaftAppleGames.Attributes;
using UnityEditor;
using UnityEngine;

namespace DaftAppleGames.Editor.Attributes
{
    public abstract class PropertyDrawerBase : PropertyDrawer
    {
        public sealed override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            // Check if visible
            bool visible = PropertyUtility.IsVisible(property);
            if (!visible)
            {
                return;
            }

            // Validate
            ValidatorAttribute[] validatorAttributes = PropertyUtility.GetAttributes<ValidatorAttribute>(property);
            foreach (ValidatorAttribute validatorAttribute in validatorAttributes)
            {
                validatorAttribute.GetValidator().ValidateProperty(property);
            }

            // Check if enabled and draw
            EditorGUI.BeginChangeCheck();
            bool enabled = PropertyUtility.IsEnabled(property);

            using (new EditorGUI.DisabledScope(!enabled))
            {
                OnGUI_Internal(rect, property, PropertyUtility.GetLabel(property));
            }

            // Call OnValueChanged callbacks
            if (EditorGUI.EndChangeCheck())
            {
                PropertyUtility.CallOnValueChangedCallbacks(property);
            }
        }

        protected abstract void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label);

        public sealed override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool visible = PropertyUtility.IsVisible(property);
            if (!visible)
            {
                return 0.0f;
            }

            return GetPropertyHeight_Internal(property, label);
        }

        protected virtual float GetPropertyHeight_Internal(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }

        protected float GetPropertyHeight(SerializedProperty property)
        {
            SpecialCaseDrawerAttribute specialCaseAttribute = PropertyUtility.GetAttribute<SpecialCaseDrawerAttribute>(property);
            if (specialCaseAttribute != null)
            {
                return specialCaseAttribute.GetDrawer().GetPropertyHeight(property);
            }

            return EditorGUI.GetPropertyHeight(property, true);
        }

        protected virtual float GetHelpBoxHeight()
        {
            return EditorGUIUtility.singleLineHeight * 2.0f;
        }

        protected void DrawDefaultPropertyAndHelpBox(Rect rect, SerializedProperty property, string message, MessageType messageType)
        {
            float indentLength = DaftAppleEditorGUI.GetIndentLength(rect);
            Rect helpBoxRect = new(
                rect.x + indentLength,
                rect.y,
                rect.width - indentLength,
                GetHelpBoxHeight());

            DaftAppleEditorGUI.HelpBox(helpBoxRect, message, MessageType.Warning, property.serializedObject.targetObject);

            Rect propertyRect = new(
                rect.x,
                rect.y + GetHelpBoxHeight(),
                rect.width,
                GetPropertyHeight(property));

            EditorGUI.PropertyField(propertyRect, property, true);
        }
    }
}