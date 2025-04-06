using Aurora.Unity.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Aurora.UnityEditor.UI
{
    [CustomEditor(typeof(Annulus))]
    [CanEditMultipleObjects]
    internal sealed class AnnulusEditor : GraphicEditor
    {
        private SerializedProperty _texture;

        private SerializedProperty _segments;

        private SerializedProperty _thickness;

        private SerializedProperty _useExactRaycastLocation;

        private GUIContent _textureGUIContent;

        private GUIContent _segmentsGUIContent;

        private GUIContent _thicknessGUIContent;

        private GUIContent _useExactRaycastLocationGUIContent;

        private const string TextureName = nameof(Annulus.texture);

        private const string SegmentsName = nameof(Annulus.segments);

        private const string ThicknessName = nameof(Annulus.thickness);

        private const string UseExactRaycastLocationName = nameof(Annulus.useExactRaycastLocation);

        protected override void OnEnable()
        {
            base.OnEnable();
            _texture                           = serializedObject.FindProperty(TextureName);
            _segments                          = serializedObject.FindProperty(SegmentsName);
            _thickness                         = serializedObject.FindProperty(ThicknessName);
            _useExactRaycastLocation           = serializedObject.FindProperty(UseExactRaycastLocationName);
            _textureGUIContent                 = new GUIContent("Texture",                    "纹理");
            _segmentsGUIContent                = new GUIContent("Segments",                   "边数");
            _thicknessGUIContent               = new GUIContent("Thickness",                  "粗细");
            _useExactRaycastLocationGUIContent = new GUIContent("Use Exact Raycast Location", "使用精确点击区域");
            SetShowNativeSize(_texture.objectReferenceValue != null, true);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_texture, _textureGUIContent);
            AppearanceControlsGUI();
            EditorGUILayout.PropertyField(_segments,  _segmentsGUIContent);
            EditorGUILayout.PropertyField(_thickness, _thicknessGUIContent);
            RaycastControlsGUI();
            MaskableControlsGUI();
            EditorGUILayout.PropertyField(_useExactRaycastLocation, _useExactRaycastLocationGUIContent);
            SetShowNativeSize(_texture.objectReferenceValue != null, false);
            NativeSizeButtonGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
