using System;
using DaftAppleGames.Attributes;
using UnityEditor;
using UnityEngine;

namespace DaftAppleGames.Editor.Attributes
{
    [CustomPropertyDrawer(typeof(InfoBoxAttribute))]
    public class InfoBoxDecoratorDrawer : DecoratorDrawer
    {
        public override float GetHeight()
        {
            return GetHelpBoxHeight();
        }

        public override void OnGUI(Rect rect)
        {
            InfoBoxAttribute infoBoxAttribute = (InfoBoxAttribute)attribute;

            float indentLength = DaftAppleEditorGUI.GetIndentLength(rect);
            Rect infoBoxRect = new(
                rect.x + indentLength,
                rect.y,
                rect.width - indentLength,
                GetHelpBoxHeight());

            DrawInfoBox(infoBoxRect, infoBoxAttribute.Text, infoBoxAttribute.Type);
        }

        private float GetHelpBoxHeight()
        {
            InfoBoxAttribute infoBoxAttribute = (InfoBoxAttribute)attribute;
            float minHeight = EditorGUIUtility.singleLineHeight * 2.0f;
            float desiredHeight = GUI.skin.box.CalcHeight(new GUIContent(infoBoxAttribute.Text), EditorGUIUtility.currentViewWidth);
            float height = Mathf.Max(minHeight, desiredHeight);

            return height;
        }

        private static void DrawInfoBox(Rect rect, string infoText, EInfoBoxType infoBoxType)
        {
            MessageType messageType = infoBoxType switch
            {
                EInfoBoxType.Normal => MessageType.Info,
                EInfoBoxType.Warning => MessageType.Warning,
                EInfoBoxType.Error => MessageType.Error,
                _ => throw new ArgumentOutOfRangeException(nameof(infoBoxType), infoBoxType, null)
            };

            DaftAppleEditorGUI.HelpBox(rect, infoText, messageType);
        }
    }
}