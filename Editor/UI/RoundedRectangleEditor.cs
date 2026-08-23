using System;
using Aurora.Unity.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Aurora.UnityEditor.UI
{
    [CustomEditor(typeof(RoundedRectangle))]
    internal sealed class RoundedRectangleEditor : GraphicEditor
    {
        private SerializedProperty _texture;

        private SerializedProperty _segments;

        private SerializedProperty _topLeftCornerRadiusNormalized;

        private SerializedProperty _topLeftCornerRadius;

        private SerializedProperty _topRightCornerRadiusNormalized;

        private SerializedProperty _topRightCornerRadius;

        private SerializedProperty _bottomLeftCornerRadiusNormalized;

        private SerializedProperty _bottomLeftCornerRadius;

        private SerializedProperty _bottomRightCornerRadiusNormalized;

        private SerializedProperty _bottomRightCornerRadius;

        private SerializedProperty _useExactRaycastLocation;

        private GUIContent _textureGUIContent;

        private GUIContent _segmentsGUIContent;

        private GUIContent _topLeftCornerGUIContent;

        private GUIContent _topRightCornerGUIContent;

        private GUIContent _bottomLeftCornerGUIContent;

        private GUIContent _bottomRightCornerGUIContent;

        private GUIContent _topLeftCornerRadiusNormalizedGUIContent;

        private GUIContent _topLeftCornerRadiusGUIContent;

        private GUIContent _topRightCornerRadiusNormalizedGUIContent;

        private GUIContent _topRightCornerRadiusGUIContent;

        private GUIContent _bottomLeftCornerRadiusNormalizedGUIContent;

        private GUIContent _bottomLeftCornerRadiusGUIContent;

        private GUIContent _bottomRightCornerRadiusNormalizedGUIContent;

        private GUIContent _bottomRightCornerRadiusGUIContent;

        private GUIContent _useExactRaycastLocationGUIContent;

        private static bool _showTopLeftCorner = true;

        private static bool _showTopRightCorner = true;

        private static bool _showBottomLeftCorner = true;

        private static bool _showBottomRightCorner = true;

        private static readonly Action<RoundedRectangle, bool> ActionSetTopLeftCornerRadiusNormalized =
            SetTopLeftCornerRadiusNormalized;

        private static readonly Func<RoundedRectangle, float> FuncGetTopLeftCornerRadius = GetTopLeftCornerRadius;

        private static readonly Action<RoundedRectangle, bool> ActionSetTopRightCornerRadiusNormalized =
            SetTopRightCornerRadiusNormalized;

        private static readonly Func<RoundedRectangle, float> FuncGetTopRightCornerRadius = GetTopRightCornerRadius;

        private static readonly Action<RoundedRectangle, bool> ActionSetBottomLeftCornerRadiusNormalized =
            SetBottomLeftCornerRadiusNormalized;

        private static readonly Func<RoundedRectangle, float> FuncGetBottomLeftCornerRadius = GetBottomLeftCornerRadius;

        private static readonly Action<RoundedRectangle, bool> ActionSetBottomRightCornerRadiusNormalized =
            SetBottomRightCornerRadiusNormalized;

        private static readonly Func<RoundedRectangle, float> FuncGetBottomRightCornerRadius =
            GetBottomRightCornerRadius;

        private const string TextureName = nameof(RoundedRectangle.texture);

        private const string SegmentsName = nameof(RoundedRectangle.segments);

        private const string TopLeftCornerRadiusNormalizedName = nameof(RoundedRectangle.topLeftCornerRadiusNormalized);

        private const string TopLeftCornerRadiusName = nameof(RoundedRectangle.topLeftCornerRadius);

        private const string TopRightCornerRadiusNormalizedName =
            nameof(RoundedRectangle.topRightCornerRadiusNormalized);

        private const string TopRightCornerRadiusName = nameof(RoundedRectangle.topRightCornerRadius);

        private const string BottomLeftCornerRadiusNormalizedName =
            nameof(RoundedRectangle.bottomLeftCornerRadiusNormalized);

        private const string BottomLeftCornerRadiusName = nameof(RoundedRectangle.bottomLeftCornerRadius);

        private const string BottomRightCornerRadiusNormalizedName =
            nameof(RoundedRectangle.bottomRightCornerRadiusNormalized);

        private const string BottomRightCornerRadiusName = nameof(RoundedRectangle.bottomRightCornerRadius);

        private const string UseExactRaycastLocationName = nameof(RoundedRectangle.useExactRaycastLocation);

        protected override void OnEnable()
        {
            base.OnEnable();
            _texture                           = serializedObject.FindProperty(TextureName);
            _segments                          = serializedObject.FindProperty(SegmentsName);
            _topLeftCornerRadiusNormalized     = serializedObject.FindProperty(TopLeftCornerRadiusNormalizedName);
            _topLeftCornerRadius               = serializedObject.FindProperty(TopLeftCornerRadiusName);
            _topRightCornerRadiusNormalized    = serializedObject.FindProperty(TopRightCornerRadiusNormalizedName);
            _topRightCornerRadius              = serializedObject.FindProperty(TopRightCornerRadiusName);
            _bottomLeftCornerRadiusNormalized  = serializedObject.FindProperty(BottomLeftCornerRadiusNormalizedName);
            _bottomLeftCornerRadius            = serializedObject.FindProperty(BottomLeftCornerRadiusName);
            _bottomRightCornerRadiusNormalized = serializedObject.FindProperty(BottomRightCornerRadiusNormalizedName);
            _bottomRightCornerRadius           = serializedObject.FindProperty(BottomRightCornerRadiusName);
            _useExactRaycastLocation           = serializedObject.FindProperty(UseExactRaycastLocationName);
            _textureGUIContent                 = new GUIContent("Texture",             "Texture");
            _segmentsGUIContent                = new GUIContent("Segments",            "Segment Count");
            _topLeftCornerGUIContent           = new GUIContent("Top Left Corner",     "Top Left Rounded Corner");
            _topRightCornerGUIContent          = new GUIContent("Top Right Corner",    "Top Right Rounded Corner");
            _bottomLeftCornerGUIContent        = new GUIContent("Bottom Left Corner",  "Bottom Left Rounded Corner");
            _bottomRightCornerGUIContent       = new GUIContent("Bottom Right Corner", "Bottom Right Rounded Corner");
            _topLeftCornerRadiusNormalizedGUIContent = new GUIContent(
                "Normalized",
                "Whether to use a normalized length to represent the top-left corner radius"
            );
            _topLeftCornerRadiusGUIContent = new GUIContent("Radius", "Top-Left Corner Radius");
            _topRightCornerRadiusNormalizedGUIContent = new GUIContent(
                "Normalized",
                "Whether to use a normalized length to represent the top-right corner radius"
            );
            _topRightCornerRadiusGUIContent = new GUIContent("Radius", "Top-Right Corner Radius");
            _bottomLeftCornerRadiusNormalizedGUIContent = new GUIContent(
                "Normalized",
                "Whether to use a normalized length to represent the bottom-left corner radius"
            );
            _bottomLeftCornerRadiusGUIContent = new GUIContent("Radius", "Bottom-Left Corner Radius");
            _bottomRightCornerRadiusNormalizedGUIContent = new GUIContent(
                "Normalized",
                "Whether to use a normalized length to represent the bottom-right corner radius"
            );
            _bottomRightCornerRadiusGUIContent = new GUIContent("Radius", "Bottom-Right Corner Radius");
            _useExactRaycastLocationGUIContent = new GUIContent("Use Exact Raycast Location", "Use Exact Hit Area");
            SetShowNativeSize(_texture.objectReferenceValue, true);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_texture, _textureGUIContent);
            AppearanceControlsGUI();
            EditorGUILayout.PropertyField(_segments, _segmentsGUIContent);
            CornersGUI();
            RaycastControlsGUI();
            MaskableControlsGUI();
            EditorGUILayout.PropertyField(_useExactRaycastLocation, _useExactRaycastLocationGUIContent);
            SetShowNativeSize(_texture.objectReferenceValue, false);
            NativeSizeButtonGUI();
            serializedObject.ApplyModifiedProperties();
        }

        private void CornersGUI()
        {
            var roundedRectangle  = (RoundedRectangle)target;
            var pixelAdjustedRect = roundedRectangle.GetPixelAdjustedRect();
            var halfMinSide       = Mathf.Min(pixelAdjustedRect.width, pixelAdjustedRect.height) * 0.5f;
            NormalizedFloatValueGUI(
                ref _showTopLeftCorner,
                _topLeftCornerGUIContent,
                _topLeftCornerRadiusNormalized,
                _topLeftCornerRadiusNormalizedGUIContent,
                roundedRectangle,
                "Set Top Left Corner Radius Normalized",
                ActionSetTopLeftCornerRadiusNormalized,
                FuncGetTopLeftCornerRadius,
                halfMinSide,
                _topLeftCornerRadius,
                _topLeftCornerRadiusGUIContent
            );
            NormalizedFloatValueGUI(
                ref _showTopRightCorner,
                _topRightCornerGUIContent,
                _topRightCornerRadiusNormalized,
                _topRightCornerRadiusNormalizedGUIContent,
                roundedRectangle,
                "Set Top Right Corner Radius Normalized",
                ActionSetTopRightCornerRadiusNormalized,
                FuncGetTopRightCornerRadius,
                halfMinSide,
                _topRightCornerRadius,
                _topRightCornerRadiusGUIContent
            );
            NormalizedFloatValueGUI(
                ref _showBottomLeftCorner,
                _bottomLeftCornerGUIContent,
                _bottomLeftCornerRadiusNormalized,
                _bottomLeftCornerRadiusNormalizedGUIContent,
                roundedRectangle,
                "Set Bottom Left Corner Radius Normalized",
                ActionSetBottomLeftCornerRadiusNormalized,
                FuncGetBottomLeftCornerRadius,
                halfMinSide,
                _bottomLeftCornerRadius,
                _bottomLeftCornerRadiusGUIContent
            );
            NormalizedFloatValueGUI(
                ref _showBottomRightCorner,
                _bottomRightCornerGUIContent,
                _bottomRightCornerRadiusNormalized,
                _bottomRightCornerRadiusNormalizedGUIContent,
                roundedRectangle,
                "Set Bottom Right Corner Radius Normalized",
                ActionSetBottomRightCornerRadiusNormalized,
                FuncGetBottomRightCornerRadius,
                halfMinSide,
                _bottomRightCornerRadius,
                _bottomRightCornerRadiusGUIContent
            );
        }

        private static void NormalizedFloatValueGUI(
            ref bool                       show,
            GUIContent                     guiContent,
            SerializedProperty             normalized,
            GUIContent                     normalizedGUIContent,
            RoundedRectangle               roundedRectangle,
            string                         undoName,
            Action<RoundedRectangle, bool> actionSetNormalized,
            Func<RoundedRectangle, float>  funcGetValue,
            float                          halfMinSide,
            SerializedProperty             value,
            GUIContent                     valueGUIContent)
        {
            if (show = EditorGUILayout.Foldout(show, guiContent))
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
                    {
                        EditorGUILayout.PropertyField(normalized, normalizedGUIContent);
                        if (changeCheckScope.changed)
                        {
                            Undo.RecordObject(roundedRectangle, undoName);
                            actionSetNormalized(roundedRectangle, normalized.boolValue);
                            value.floatValue = funcGetValue(roundedRectangle);
                            EditorUtility.SetDirty(roundedRectangle);
                        }
                    }
                    const float leftValue = 0;
                    var rightValue = normalized.boolValue switch
                    {
                        false => halfMinSide,
                        true  => 1
                    };
                    EditorGUILayout.Slider(value, leftValue, rightValue, valueGUIContent);
                }
            }
        }

        private static void SetTopLeftCornerRadiusNormalized(RoundedRectangle roundedRectangle, bool normalized)
        {
            roundedRectangle.TopLeftCornerRadiusNormalized = normalized;
        }

        private static float GetTopLeftCornerRadius(RoundedRectangle roundedRectangle)
        {
            return roundedRectangle.TopLeftCornerRadius;
        }

        private static void SetTopRightCornerRadiusNormalized(RoundedRectangle roundedRectangle, bool normalized)
        {
            roundedRectangle.TopRightCornerRadiusNormalized = normalized;
        }

        private static float GetTopRightCornerRadius(RoundedRectangle roundedRectangle)
        {
            return roundedRectangle.TopRightCornerRadius;
        }

        private static void SetBottomLeftCornerRadiusNormalized(RoundedRectangle roundedRectangle, bool normalized)
        {
            roundedRectangle.BottomLeftCornerRadiusNormalized = normalized;
        }

        private static float GetBottomLeftCornerRadius(RoundedRectangle roundedRectangle)
        {
            return roundedRectangle.BottomLeftCornerRadius;
        }

        private static void SetBottomRightCornerRadiusNormalized(RoundedRectangle roundedRectangle, bool normalized)
        {
            roundedRectangle.BottomRightCornerRadiusNormalized = normalized;
        }

        private static float GetBottomRightCornerRadius(RoundedRectangle roundedRectangle)
        {
            return roundedRectangle.BottomRightCornerRadius;
        }
    }
}
