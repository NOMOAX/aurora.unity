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
            _texture = serializedObject.FindProperty(TextureName);
            _segments = serializedObject.FindProperty(SegmentsName);
            _topLeftCornerRadiusNormalized = serializedObject.FindProperty(TopLeftCornerRadiusNormalizedName);
            _topLeftCornerRadius = serializedObject.FindProperty(TopLeftCornerRadiusName);
            _topRightCornerRadiusNormalized = serializedObject.FindProperty(TopRightCornerRadiusNormalizedName);
            _topRightCornerRadius = serializedObject.FindProperty(TopRightCornerRadiusName);
            _bottomLeftCornerRadiusNormalized = serializedObject.FindProperty(BottomLeftCornerRadiusNormalizedName);
            _bottomLeftCornerRadius = serializedObject.FindProperty(BottomLeftCornerRadiusName);
            _bottomRightCornerRadiusNormalized = serializedObject.FindProperty(BottomRightCornerRadiusNormalizedName);
            _bottomRightCornerRadius = serializedObject.FindProperty(BottomRightCornerRadiusName);
            _useExactRaycastLocation = serializedObject.FindProperty(UseExactRaycastLocationName);
            _textureGUIContent = new GUIContent("Texture", "纹理");
            _segmentsGUIContent = new GUIContent("Segments", "边数");
            _topLeftCornerGUIContent = new GUIContent("Top Left Corner", "左上圆角");
            _topRightCornerGUIContent = new GUIContent("Top Right Corner", "右上圆角");
            _bottomLeftCornerGUIContent = new GUIContent("Bottom Left Corner", "左下圆角");
            _bottomRightCornerGUIContent = new GUIContent("Bottom Right Corner", "右下圆角");
            _topLeftCornerRadiusNormalizedGUIContent = new GUIContent("Normalized", "是否使用标准化长度表示左上圆角半径");
            _topLeftCornerRadiusGUIContent = new GUIContent("Radius", "左上圆角半径");
            _topRightCornerRadiusNormalizedGUIContent = new GUIContent("Normalized", "是否使用标准化长度表示右上圆角半径");
            _topRightCornerRadiusGUIContent = new GUIContent("Radius", "右上圆角半径");
            _bottomLeftCornerRadiusNormalizedGUIContent = new GUIContent("Normalized", "是否使用标准化长度表示左下圆角半径");
            _bottomLeftCornerRadiusGUIContent = new GUIContent("Radius", "左下圆角半径");
            _bottomRightCornerRadiusNormalizedGUIContent = new GUIContent("Normalized", "是否使用标准化长度表示右下圆角半径");
            _bottomRightCornerRadiusGUIContent = new GUIContent("Radius", "右下圆角半径");
            _useExactRaycastLocationGUIContent = new GUIContent("Use Exact Raycast Location", "使用精确点击区域");
            SetShowNativeSize(_texture.objectReferenceValue != null, true);
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
            SetShowNativeSize(_texture.objectReferenceValue != null, false);
            NativeSizeButtonGUI();
            serializedObject.ApplyModifiedProperties();
        }

        private void CornersGUI()
        {
            var roundedRectangle  = (RoundedRectangle) target;
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
                    const float leftValue = 0f;
                    var rightValue = normalized.boolValue switch
                    {
                        false => halfMinSide,
                        true  => 1f
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
