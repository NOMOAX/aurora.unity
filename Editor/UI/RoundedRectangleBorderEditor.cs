using System;
using Aurora.Unity.UI;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace Aurora.UnityEditor.UI
{
    [CustomEditor(typeof(RoundedRectangleBorder))]
    internal sealed class RoundedRectangleBorderEditor : GraphicEditor
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

        private SerializedProperty _thicknessNormalized;

        private SerializedProperty _thickness;

        private SerializedProperty _useExactRaycastLocation;

        private GUIContent _textureGUIContent;

        private GUIContent _segmentsGUIContent;

        private GUIContent _topLeftCornerGUIContent;

        private GUIContent _topRightCornerGUIContent;

        private GUIContent _bottomLeftCornerGUIContent;

        private GUIContent _bottomRightCornerGUIContent;

        private GUIContent _thicknessGUIContent;

        private GUIContent _topLeftCornerRadiusNormalizedGUIContent;

        private GUIContent _topLeftCornerRadiusGUIContent;

        private GUIContent _topRightCornerRadiusNormalizedGUIContent;

        private GUIContent _topRightCornerRadiusGUIContent;

        private GUIContent _bottomLeftCornerRadiusNormalizedGUIContent;

        private GUIContent _bottomLeftCornerRadiusGUIContent;

        private GUIContent _bottomRightCornerRadiusNormalizedGUIContent;

        private GUIContent _bottomRightCornerRadiusGUIContent;

        private GUIContent _thicknessNormalizedGUIContents;

        private GUIContent _thicknessValueGUIContent;

        private GUIContent _useExactRaycastLocationGUIContent;

        private GUIStyle _rightToLeftLabelGUIStyle;

        private GUIStyle _rightToLeftNumberFieldGUIStyle;

        private static bool _showTopLeftCorner = true;

        private static bool _showTopRightCorner = true;

        private static bool _showBottomLeftCorner = true;

        private static bool _showBottomRightCorner = true;

        private static bool _showThickness = true;

        private static readonly Action<RoundedRectangleBorder, bool> ActionSetTopLeftCornerRadiusNormalized =
            SetTopLeftCornerRadiusNormalized;

        private static readonly Func<RoundedRectangleBorder, float> FuncGetTopLeftCornerRadius = GetTopLeftCornerRadius;

        private static readonly Action<RoundedRectangleBorder, bool> ActionSetTopRightCornerRadiusNormalized =
            SetTopRightCornerRadiusNormalized;

        private static readonly Func<RoundedRectangleBorder, float> FuncGetTopRightCornerRadius =
            GetTopRightCornerRadius;

        private static readonly Action<RoundedRectangleBorder, bool> ActionSetBottomLeftCornerRadiusNormalized =
            SetBottomLeftCornerRadiusNormalized;

        private static readonly Func<RoundedRectangleBorder, float> FuncGetBottomLeftCornerRadius =
            GetBottomLeftCornerRadius;

        private static readonly Action<RoundedRectangleBorder, bool> ActionSetBottomRightCornerRadiusNormalized =
            SetBottomRightCornerRadiusNormalized;

        private static readonly Func<RoundedRectangleBorder, float> FuncGetBottomRightCornerRadius =
            GetBottomRightCornerRadius;

        private static readonly Action<RoundedRectangleBorder, bool> ActionSetThicknessNormalized =
            SetThicknessNormalized;

        private static readonly Func<RoundedRectangleBorder, float> FuncGetThickness = GetThickness;

        private const string TextureName = nameof(RoundedRectangleBorder.texture);

        private const string SegmentsName = nameof(RoundedRectangleBorder.segments);

        private const string TopLeftCornerRadiusNormalizedName =
            nameof(RoundedRectangleBorder.topLeftCornerRadiusNormalized);

        private const string TopLeftCornerRadiusName = nameof(RoundedRectangleBorder.topLeftCornerRadius);

        private const string TopRightCornerRadiusNormalizedName =
            nameof(RoundedRectangleBorder.topRightCornerRadiusNormalized);

        private const string TopRightCornerRadiusName = nameof(RoundedRectangleBorder.topRightCornerRadius);

        private const string BottomLeftCornerRadiusNormalizedName =
            nameof(RoundedRectangleBorder.bottomLeftCornerRadiusNormalized);

        private const string BottomLeftCornerRadiusName = nameof(RoundedRectangleBorder.bottomLeftCornerRadius);

        private const string BottomRightCornerRadiusNormalizedName =
            nameof(RoundedRectangleBorder.bottomRightCornerRadiusNormalized);

        private const string BottomRightCornerRadiusName = nameof(RoundedRectangleBorder.bottomRightCornerRadius);

        private const string ThicknessNormalizedName = nameof(RoundedRectangleBorder.thicknessNormalized);

        private const string ThicknessName = nameof(RoundedRectangleBorder.thickness);

        private const string UseExactRaycastLocationName = nameof(RoundedRectangleBorder.useExactRaycastLocation);

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
            _thicknessNormalized = serializedObject.FindProperty(ThicknessNormalizedName);
            _thickness = serializedObject.FindProperty(ThicknessName);
            _useExactRaycastLocation = serializedObject.FindProperty(UseExactRaycastLocationName);
            _textureGUIContent = new GUIContent("Texture", "纹理");
            _segmentsGUIContent = new GUIContent("Segments", "边数");
            _topLeftCornerGUIContent = new GUIContent("Top Left Corner", "左上圆角");
            _topRightCornerGUIContent = new GUIContent("Top Right Corner", "右上圆角");
            _bottomLeftCornerGUIContent = new GUIContent("Bottom Left Corner", "左下圆角");
            _bottomRightCornerGUIContent = new GUIContent("Bottom Right Corner", "右下圆角");
            _thicknessGUIContent = new GUIContent("Thickness", "粗细");
            _topLeftCornerRadiusNormalizedGUIContent = new GUIContent("Normalized", "是否使用标准化长度表示左上圆角半径");
            _topLeftCornerRadiusGUIContent = new GUIContent("Radius", "左上圆角半径");
            _topRightCornerRadiusNormalizedGUIContent = new GUIContent("Normalized", "是否使用标准化长度表示右上圆角半径");
            _topRightCornerRadiusGUIContent = new GUIContent("Radius", "右上圆角半径");
            _bottomLeftCornerRadiusNormalizedGUIContent = new GUIContent("Normalized", "是否使用标准化长度表示左下圆角半径");
            _bottomLeftCornerRadiusGUIContent = new GUIContent("Radius", "左下圆角半径");
            _bottomRightCornerRadiusNormalizedGUIContent = new GUIContent("Normalized", "是否使用标准化长度表示右下圆角半径");
            _bottomRightCornerRadiusGUIContent = new GUIContent("Radius", "右下圆角半径");
            _thicknessNormalizedGUIContents = new GUIContent("Normalized", "是否使用标准化长度表示粗细");
            _thicknessValueGUIContent = new GUIContent("Value", "粗细");
            _useExactRaycastLocationGUIContent = new GUIContent("Use Exact Raycast Location", "使用精确点击区域");
            SetShowNativeSize(_texture.objectReferenceValue, true);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_texture, _textureGUIContent);
            AppearanceControlsGUI();
            EditorGUILayout.PropertyField(_segments, _segmentsGUIContent);
            CornersAndThicknessGUI();
            RaycastControlsGUI();
            MaskableControlsGUI();
            EditorGUILayout.PropertyField(_useExactRaycastLocation, _useExactRaycastLocationGUIContent);
            SetShowNativeSize(_texture.objectReferenceValue, false);
            NativeSizeButtonGUI();
            serializedObject.ApplyModifiedProperties();
        }

        private void CornersAndThicknessGUI()
        {
            var roundedRectangleBorder = (RoundedRectangleBorder) target;
            var pixelAdjustedRect      = roundedRectangleBorder.GetPixelAdjustedRect();
            var halfMinSide            = Mathf.Min(pixelAdjustedRect.width, pixelAdjustedRect.height) * 0.5f;
            NormalizedFloatValueGUI(
                ref _showTopLeftCorner,
                _topLeftCornerGUIContent,
                _topLeftCornerRadiusNormalized,
                _topLeftCornerRadiusNormalizedGUIContent,
                roundedRectangleBorder,
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
                roundedRectangleBorder,
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
                roundedRectangleBorder,
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
                roundedRectangleBorder,
                "Set Bottom Right Corner Radius Normalized",
                ActionSetBottomRightCornerRadiusNormalized,
                FuncGetBottomRightCornerRadius,
                halfMinSide,
                _bottomRightCornerRadius,
                _bottomRightCornerRadiusGUIContent
            );
            NormalizedFloatValueGUI(
                ref _showThickness,
                _thicknessGUIContent,
                _thicknessNormalized,
                _thicknessNormalizedGUIContents,
                roundedRectangleBorder,
                "Set Thickness Normalized",
                ActionSetThicknessNormalized,
                FuncGetThickness,
                halfMinSide,
                _thickness,
                _thicknessValueGUIContent
            );
        }

        private static void NormalizedFloatValueGUI(
            ref bool                             show,
            GUIContent                           guiContent,
            SerializedProperty                   normalized,
            GUIContent                           normalizedGUIContent,
            RoundedRectangleBorder               roundedRectangle,
            string                               undoName,
            Action<RoundedRectangleBorder, bool> actionSetNormalized,
            Func<RoundedRectangleBorder, float>  funcGetValue,
            float                                halfMinSide,
            SerializedProperty                   value,
            GUIContent                           valueGUIContent)
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

        private static void SetTopLeftCornerRadiusNormalized(
            RoundedRectangleBorder roundedRectangleBorder,
            bool                   normalized)
        {
            roundedRectangleBorder.TopLeftCornerRadiusNormalized = normalized;
        }

        private static float GetTopLeftCornerRadius(RoundedRectangleBorder roundedRectangleBorder)
        {
            return roundedRectangleBorder.TopLeftCornerRadius;
        }

        private static void SetTopRightCornerRadiusNormalized(
            RoundedRectangleBorder roundedRectangleBorder,
            bool                   normalized)
        {
            roundedRectangleBorder.TopRightCornerRadiusNormalized = normalized;
        }

        private static float GetTopRightCornerRadius(RoundedRectangleBorder roundedRectangleBorder)
        {
            return roundedRectangleBorder.TopRightCornerRadius;
        }

        private static void SetBottomLeftCornerRadiusNormalized(
            RoundedRectangleBorder roundedRectangleBorder,
            bool                   normalized)
        {
            roundedRectangleBorder.BottomLeftCornerRadiusNormalized = normalized;
        }

        private static float GetBottomLeftCornerRadius(RoundedRectangleBorder roundedRectangleBorder)
        {
            return roundedRectangleBorder.BottomLeftCornerRadius;
        }

        private static void SetBottomRightCornerRadiusNormalized(
            RoundedRectangleBorder roundedRectangleBorder,
            bool                   normalized)
        {
            roundedRectangleBorder.BottomRightCornerRadiusNormalized = normalized;
        }

        private static float GetBottomRightCornerRadius(RoundedRectangleBorder roundedRectangleBorder)
        {
            return roundedRectangleBorder.BottomRightCornerRadius;
        }

        private static void SetThicknessNormalized(RoundedRectangleBorder roundedRectangleBorder, bool normalized)
        {
            roundedRectangleBorder.ThicknessNormalized = normalized;
        }

        private static float GetThickness(RoundedRectangleBorder roundedRectangleBorder)
        {
            return roundedRectangleBorder.thickness;
        }
    }
}
