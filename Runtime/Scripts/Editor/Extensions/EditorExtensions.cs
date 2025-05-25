using UnityEditor;
using UnityEngine;

namespace DaftAppleGames.Editor.Extensions
{
    public static class EditorExtensions
    {
        public static float GetInspectorGUIHeight(this UnityEditor.Editor editor)
        {
            float height = 0f;

            using EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope();
            SerializedObject so = editor.serializedObject;
            SerializedProperty iterator = so.GetIterator();

            if (iterator.NextVisible(true))
            {
                do
                {
                    height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
                } while (iterator.NextVisible(false));
            }

            return height;
        }
    }
}