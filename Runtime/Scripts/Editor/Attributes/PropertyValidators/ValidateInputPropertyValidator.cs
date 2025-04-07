using UnityEditor;
using System.Reflection;
using System;
using DaftAppleGames.Attributes;

namespace DaftAppleGames.Editor.Attributes
{
    public class ValidateInputPropertyValidator : PropertyValidatorBase
    {
        public override void ValidateProperty(SerializedProperty property)
        {
            ValidateInputAttribute validateInputAttribute = PropertyUtility.GetAttribute<ValidateInputAttribute>(property);
            object target = PropertyUtility.GetTargetObjectWithProperty(property);

            MethodInfo validationCallback = ReflectionUtility.GetMethod(target, validateInputAttribute.CallbackName);

            if (validationCallback == null ||
                validationCallback.ReturnType != typeof(bool))
            {
                return;
            }

            ParameterInfo[] callbackParameters = validationCallback.GetParameters();

            switch (callbackParameters.Length)
            {
                case 0:
                {
                    if (!(bool)validationCallback.Invoke(target, null))
                    {
                        if (string.IsNullOrEmpty(validateInputAttribute.Message))
                        {
                            DaftAppleEditorGUI.HelpBox_Layout(
                                property.name + " is not valid", MessageType.Error, property.serializedObject.targetObject);
                        }
                        else
                        {
                            DaftAppleEditorGUI.HelpBox_Layout(
                                validateInputAttribute.Message, MessageType.Error, property.serializedObject.targetObject);
                        }
                    }

                    break;
                }
                case 1:
                {
                    FieldInfo fieldInfo = ReflectionUtility.GetField(target, property.name);
                    Type fieldType = fieldInfo.FieldType;
                    Type parameterType = callbackParameters[0].ParameterType;

                    if (fieldType == parameterType)
                    {
                        if (!(bool)validationCallback.Invoke(target, new[] { fieldInfo.GetValue(target) }))
                        {
                            if (string.IsNullOrEmpty(validateInputAttribute.Message))
                            {
                                DaftAppleEditorGUI.HelpBox_Layout(
                                    property.name + " is not valid", MessageType.Error, property.serializedObject.targetObject);
                            }
                            else
                            {
                                DaftAppleEditorGUI.HelpBox_Layout(
                                    validateInputAttribute.Message, MessageType.Error, property.serializedObject.targetObject);
                            }
                        }
                    }
                    else
                    {
                        string warning = "The field type is not the same as the callback's parameter type";
                        DaftAppleEditorGUI.HelpBox_Layout(warning, MessageType.Warning, property.serializedObject.targetObject);
                    }

                    break;
                }
                default:
                {
                    string warning =
                        validateInputAttribute.GetType().Name +
                        " needs a callback with boolean return type and an optional single parameter of the same type as the field";

                    DaftAppleEditorGUI.HelpBox_Layout(warning, MessageType.Warning, property.serializedObject.targetObject);
                    break;
                }
            }
        }
    }
}