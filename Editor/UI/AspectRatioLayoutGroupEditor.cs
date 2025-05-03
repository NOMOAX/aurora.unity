using Aurora.Unity.UI;
using UnityEditor;

namespace Aurora.UnityEditor.UI
{
    [CustomEditor(typeof(AspectRatioLayoutGroup))]
    [CanEditMultipleObjects]
    internal sealed class AspectRatioLayoutGroupEditor : OneElementLayoutGroupEditor
    {
        private SerializedProperty _aspectRatio;

        private void OnEnable()
        {
            _aspectRatio = serializedObject.FindProperty(nameof(AspectRatioLayoutGroup.aspectRatio));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_aspectRatio);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}
