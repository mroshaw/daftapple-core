using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using DaftAppleGames.Attributes;
using Object = UnityEngine.Object;

namespace DaftAppleGames.Editor.Attributes
{
    public class ReorderableListPropertyDrawer : SpecialCasePropertyDrawerBase
    {
        public static readonly ReorderableListPropertyDrawer Instance = new();

        private readonly Dictionary<string, ReorderableList> _reorderableListsByPropertyName = new();

        private GUIStyle _labelStyle;

        private GUIStyle GetLabelStyle()
        {
            if (_labelStyle != null)
            {
                return _labelStyle;
            }

            _labelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                richText = true
            };

            return _labelStyle;
        }

        private static string GetPropertyKeyName(SerializedProperty property)
        {
            return property.serializedObject.targetObject.GetInstanceID() + "." + property.name;
        }

        protected override float GetPropertyHeight_Internal(SerializedProperty property)
        {
            if (!property.isArray)
            {
                return EditorGUI.GetPropertyHeight(property, true);
            }

            string key = GetPropertyKeyName(property);

            if (_reorderableListsByPropertyName.TryGetValue(key, out ReorderableList reorderableList) == false)
            {
                return 0;
            }

            return reorderableList.GetHeight();
        }

        protected override void OnGUI_Internal(Rect rect, SerializedProperty property, GUIContent label)
        {
            if (property.isArray)
            {
                string key = GetPropertyKeyName(property);

                ReorderableList reorderableList = null;
                if (!_reorderableListsByPropertyName.ContainsKey(key))
                {
                    reorderableList = new ReorderableList(property.serializedObject, property, true, true, true, true)
                    {
                        drawHeaderCallback = r =>
                        {
                            EditorGUI.LabelField(r, $"{label.text}: {property.arraySize}", GetLabelStyle());
                            HandleDragAndDrop(r, null);
                        },

                        drawElementCallback = (r, index, _, _) =>
                        {
                            SerializedProperty element = property.GetArrayElementAtIndex(index);
                            r.y += 1.0f;
                            r.x += 10.0f;
                            r.width -= 10.0f;

                            EditorGUI.PropertyField(new Rect(r.x, r.y, r.width, EditorGUIUtility.singleLineHeight), element, true);
                        },

                        elementHeightCallback = (int index) => EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(index)) + 4.0f
                    };

                    _reorderableListsByPropertyName[key] = reorderableList;
                }

                reorderableList = _reorderableListsByPropertyName[key];

                if (rect == default)
                {
                    reorderableList.DoLayoutList();
                }
                else
                {
                    reorderableList.DoList(rect);
                }
            }
            else
            {
                string message = nameof(ReorderableListAttribute) + " can be used only on arrays or lists";
                DaftAppleEditorGUI.HelpBox_Layout(message, MessageType.Warning, property.serializedObject.targetObject);
                EditorGUILayout.PropertyField(property, true);
            }
        }

        public void ClearCache()
        {
            _reorderableListsByPropertyName.Clear();
        }

        private static Object GetAssignableObject(Object obj, ReorderableList list)
        {
            Type listType = PropertyUtility.GetPropertyType(list.serializedProperty);
            Type elementType = ReflectionUtility.GetListElementType(listType);

            if (elementType == null)
            {
                return null;
            }

            Type objType = obj.GetType();

            if (elementType.IsAssignableFrom(objType))
            {
                return obj;
            }

            if (objType != typeof(GameObject))
            {
                return null;
            }

            if (typeof(Transform).IsAssignableFrom(elementType))
            {
                Transform transform = ((GameObject)obj).transform;
                if (elementType == typeof(RectTransform))
                {
                    RectTransform rectTransform = transform as RectTransform;
                    return rectTransform;
                }
                else
                {
                    return transform;
                }
            }

            return typeof(MonoBehaviour).IsAssignableFrom(elementType) ? ((GameObject)obj).GetComponent(elementType) : null;
        }

        private void HandleDragAndDrop(Rect rect, ReorderableList list)
        {
            Event currentEvent = Event.current;
            bool usedEvent = false;

            switch (currentEvent.type)
            {
                case EventType.DragExited:
                    if (GUI.enabled)
                    {
                        HandleUtility.Repaint();
                    }

                    break;

                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (rect.Contains(currentEvent.mousePosition) && GUI.enabled)
                    {
                        // Check each single object, so we can add multiple objects in a single drag.
                        bool didAcceptDrag = false;
                        Object[] references = DragAndDrop.objectReferences;
                        foreach (Object obj in references)
                        {
                            Object assignableObject = GetAssignableObject(obj, list);
                            if (assignableObject == null)
                            {
                                continue;
                            }

                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                            if (currentEvent.type != EventType.DragPerform)
                            {
                                continue;
                            }

                            list.serializedProperty.arraySize++;
                            int arrayEnd = list.serializedProperty.arraySize - 1;
                            list.serializedProperty.GetArrayElementAtIndex(arrayEnd).objectReferenceValue = assignableObject;
                            didAcceptDrag = true;
                        }

                        if (didAcceptDrag)
                        {
                            GUI.changed = true;
                            DragAndDrop.AcceptDrag();
                            usedEvent = true;
                        }
                    }

                    break;
                case EventType.MouseDown:
                    break;
                case EventType.MouseUp:
                    break;
                case EventType.MouseMove:
                    break;
                case EventType.MouseDrag:
                    break;
                case EventType.KeyDown:
                    break;
                case EventType.KeyUp:
                    break;
                case EventType.ScrollWheel:
                    break;
                case EventType.Repaint:
                    break;
                case EventType.Layout:
                    break;
                case EventType.Ignore:
                    break;
                case EventType.Used:
                    break;
                case EventType.ValidateCommand:
                    break;
                case EventType.ExecuteCommand:
                    break;
                case EventType.ContextClick:
                    break;
                case EventType.MouseEnterWindow:
                    break;
                case EventType.MouseLeaveWindow:
                    break;
                case EventType.TouchDown:
                    break;
                case EventType.TouchUp:
                    break;
                case EventType.TouchMove:
                    break;
                case EventType.TouchEnter:
                    break;
                case EventType.TouchLeave:
                    break;
                case EventType.TouchStationary:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (usedEvent)
            {
                currentEvent.Use();
            }
        }
    }
}