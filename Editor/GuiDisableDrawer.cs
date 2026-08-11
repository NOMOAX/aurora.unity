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
            var disabled = ((GuiDisableAttribute)attribute).When switch
            {
                When.Always     => true,
                When.Playing    => EditorApplication.isPlaying,
                When.NotPlaying => !EditorApplication.isPlaying,
                _               => true
            };
            using (new EditorGUI.DisabledScope(disabled))
            {
                EditorGUI.PropertyField(position, property, label, UnityEditorUtility.IsChildrenIncluded(property));
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, UnityEditorUtility.IsChildrenIncluded(property));
        }
    }
}
