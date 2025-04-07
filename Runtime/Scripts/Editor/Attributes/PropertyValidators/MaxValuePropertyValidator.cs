using DaftAppleGames.Attributes;
using UnityEngine;
using UnityEditor;

namespace DaftAppleGames.Editor.Attributes
{
    public class MaxValuePropertyValidator : PropertyValidatorBase
    {
        public override void ValidateProperty(SerializedProperty property)
        {
            MaxValueAttribute maxValueAttribute = PropertyUtility.GetAttribute<MaxValueAttribute>(property);

            switch (property.propertyType)
            {
                case SerializedPropertyType.Integer:
                {
                    if (property.intValue > maxValueAttribute.MaxValue)
                    {
                        property.intValue = (int)maxValueAttribute.MaxValue;
                    }

                    break;
                }
                case SerializedPropertyType.Float:
                {
                    if (property.floatValue > maxValueAttribute.MaxValue)
                    {
                        property.floatValue = maxValueAttribute.MaxValue;
                    }

                    break;
                }
                case SerializedPropertyType.Vector2:
                    property.vector2Value = Vector2.Min(property.vector2Value, new Vector2(maxValueAttribute.MaxValue, maxValueAttribute.MaxValue));
                    break;
                case SerializedPropertyType.Vector3:
                    property.vector3Value = Vector3.Min(property.vector3Value, new Vector3(maxValueAttribute.MaxValue, maxValueAttribute.MaxValue, maxValueAttribute.MaxValue));
                    break;
                case SerializedPropertyType.Vector4:
                    property.vector4Value = Vector4.Min(property.vector4Value,
                        new Vector4(maxValueAttribute.MaxValue, maxValueAttribute.MaxValue, maxValueAttribute.MaxValue, maxValueAttribute.MaxValue));
                    break;
                case SerializedPropertyType.Vector2Int:
                    property.vector2IntValue = Vector2Int.Min(property.vector2IntValue, new Vector2Int((int)maxValueAttribute.MaxValue, (int)maxValueAttribute.MaxValue));
                    break;
                case SerializedPropertyType.Vector3Int:
                    property.vector3IntValue = Vector3Int.Min(property.vector3IntValue,
                        new Vector3Int((int)maxValueAttribute.MaxValue, (int)maxValueAttribute.MaxValue, (int)maxValueAttribute.MaxValue));
                    break;
                case SerializedPropertyType.Generic:
                case SerializedPropertyType.Boolean:
                case SerializedPropertyType.String:
                case SerializedPropertyType.Color:
                case SerializedPropertyType.ObjectReference:
                case SerializedPropertyType.LayerMask:
                case SerializedPropertyType.Enum:
                case SerializedPropertyType.Rect:
                case SerializedPropertyType.ArraySize:
                case SerializedPropertyType.Character:
                case SerializedPropertyType.AnimationCurve:
                case SerializedPropertyType.Bounds:
                case SerializedPropertyType.Gradient:
                case SerializedPropertyType.Quaternion:
                case SerializedPropertyType.ExposedReference:
                case SerializedPropertyType.FixedBufferSize:
                case SerializedPropertyType.RectInt:
                case SerializedPropertyType.BoundsInt:
                case SerializedPropertyType.ManagedReference:
                case SerializedPropertyType.Hash128:
                case SerializedPropertyType.RenderingLayerMask:
                default:
                {
                    string warning = maxValueAttribute.GetType().Name + " can be used only on int, float, Vector or VectorInt fields";
                    Debug.LogWarning(warning, property.serializedObject.targetObject);
                    break;
                }
            }
        }
    }
}