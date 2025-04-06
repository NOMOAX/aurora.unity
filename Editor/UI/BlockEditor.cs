using Aurora.Unity.UI;
using UnityEditor;
using UnityEditor.UI;

namespace Aurora.UnityEditor.UI
{
    [CustomEditor(typeof(Block))]
    [CanEditMultipleObjects]
    internal sealed class BlockEditor : GraphicEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            AppearanceControlsGUI();
            RaycastControlsGUI();
            MaskableControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
