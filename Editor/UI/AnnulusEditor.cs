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
            _textureGUIContent                 = new GUIContent("Texture",                    "Texture");
            _segmentsGUIContent                = new GUIContent("Segments",                   "Segment Count");
            _thicknessGUIContent               = new GUIContent("Thickness",                  "Thickness");
            _useExactRaycastLocationGUIContent = new GUIContent("Use Exact Raycast Location", "Use Exact Hit Area");
            SetShowNativeSize(_texture.objectReferenceValue, true);
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
            SetShowNativeSize(_texture.objectReferenceValue, false);
            NativeSizeButtonGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
