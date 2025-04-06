using Aurora.Unity.UI;
using UnityEditor;
using UnityEditor.UI;

namespace Aurora.UnityEditor.UI
{
    [CustomEditor(typeof(Clear))]
    [CanEditMultipleObjects]
    internal sealed class ClearEditor : GraphicEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            RaycastControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
