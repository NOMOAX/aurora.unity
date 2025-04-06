using System;
using Aurora.Unity;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Aurora.UnityEditor
{
    [CustomPropertyDrawer(typeof(TagAttribute))]
    internal sealed class TagDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                var tags  = InternalEditorUtility.tags;
                var index = Array.IndexOf(tags, property.stringValue);
                using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
                {
                    index = EditorGUI.Popup(position, label, index, Array.ConvertAll(tags, e => new GUIContent(e)));
                    if (changeCheckScope.changed)
                    {
                        property.stringValue = tags[index];
                    }
                }
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "Use Tag with string.");
            }
        }
    }
}
