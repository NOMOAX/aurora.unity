using Aurora.Unity;
using UnityEditor;
using UnityEngine;

namespace Aurora.UnityEditor
{
    [CustomPropertyDrawer(typeof(GuiDisableAttribute))]
    internal sealed class GuiDisableDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property, label, UnityEditorUtility.IsChildrenIncluded(property));
            EditorGUI.EndDisabledGroup();
        }
    }
}
