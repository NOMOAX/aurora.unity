using Aurora.Unity.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Aurora.UnityEditor.UI
{
    [CustomEditor(typeof(CustomGraphic))]
    [CanEditMultipleObjects]
    internal sealed class CustomGraphicEditor : GraphicEditor
    {
        private SerializedProperty _texture;

        private SerializedProperty _vertices;

        private SerializedProperty _triangles;

        private GUIContent _textureGUIContent;

        private GUIContent _verticesGUIContent;

        private GUIContent _trianglesGUIContent;

        private const string TextureName = nameof(CustomGraphic.texture);

        private const string VerticesName = nameof(CustomGraphic.vertices);

        private const string TrianglesName = nameof(CustomGraphic.triangles);

        protected override void OnEnable()
        {
            base.OnEnable();
            _texture             = serializedObject.FindProperty(TextureName);
            _vertices            = serializedObject.FindProperty(VerticesName);
            _triangles           = serializedObject.FindProperty(TrianglesName);
            _textureGUIContent   = new GUIContent("Texture",   "Texture");
            _verticesGUIContent  = new GUIContent("Vertices",  "Vertices");
            _trianglesGUIContent = new GUIContent("Triangles", "Triangles");
            SetShowNativeSize(_texture.objectReferenceValue, true);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_texture, _textureGUIContent);
            AppearanceControlsGUI();
            EditorGUILayout.PropertyField(_vertices,  _verticesGUIContent);
            EditorGUILayout.PropertyField(_triangles, _trianglesGUIContent);
            RaycastControlsGUI();
            MaskableControlsGUI();
            SetShowNativeSize(_texture.objectReferenceValue, false);
            NativeSizeButtonGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
