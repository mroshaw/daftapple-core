using DaftAppleGames.Attributes;
using UnityEditor;

namespace DaftAppleGames.Editor.Attributes
{
    public class RequiredPropertyValidator : PropertyValidatorBase
    {
        public override void ValidateProperty(SerializedProperty property)
        {
            RequiredAttribute requiredAttribute = PropertyUtility.GetAttribute<RequiredAttribute>(property);

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (property.objectReferenceValue != null)
                {
                    return;
                }

                string errorMessage = property.name + " is required";
                if (!string.IsNullOrEmpty(requiredAttribute.Message))
                {
                    errorMessage = requiredAttribute.Message;
                }

                DaftAppleEditorGUI.HelpBox_Layout(errorMessage, MessageType.Error, property.serializedObject.targetObject);
            }
            else
            {
                string warning = requiredAttribute.GetType().Name + " works only on reference types";
                DaftAppleEditorGUI.HelpBox_Layout(warning, MessageType.Warning, property.serializedObject.targetObject);
            }
        }
    }
}