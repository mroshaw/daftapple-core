using System;
using System.Collections.Generic;
using DaftAppleGames.Attributes;
using UnityEditor;

namespace DaftAppleGames.Editor.Attributes
{
    public abstract class PropertyValidatorBase
    {
        public abstract void ValidateProperty(SerializedProperty property);
    }

    public static class ValidatorAttributeExtensions
    {
        private static readonly Dictionary<Type, PropertyValidatorBase> ValidatorsByAttributeType;

        static ValidatorAttributeExtensions()
        {
            ValidatorsByAttributeType = new Dictionary<Type, PropertyValidatorBase>
            {
                [typeof(MinValueAttribute)] = new MinValuePropertyValidator(),
                [typeof(MaxValueAttribute)] = new MaxValuePropertyValidator(),
                [typeof(RequiredAttribute)] = new RequiredPropertyValidator(),
                [typeof(ValidateInputAttribute)] = new ValidateInputPropertyValidator()
            };
        }

        public static PropertyValidatorBase GetValidator(this ValidatorAttribute attr)
        {
            return ValidatorsByAttributeType.GetValueOrDefault(attr.GetType());
        }
    }
}