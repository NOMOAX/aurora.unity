using Aurora.Unity.UI;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Aurora.UnityEditor.UI
{
    [CustomEditor(typeof(ScrollView), true)]
    internal class ScrollViewEditor : Editor
    {
        private const string ScrollRectName = nameof(ScrollView.scrollRect);

        private const string InactiveContainerName = nameof(ScrollView.inactiveContainer);

        private const string ViewportName = nameof(ScrollView.viewport);

        private const string ContentName = nameof(ScrollView.content);

        private const string ContentLayoutGroupName = nameof(ScrollView.contentLayoutGroup);

        private const string PaddingName = nameof(ScrollView.padding);

        private const string RectOffsetLeftName = "m_Left";

        private const string RectOffsetRightName = "m_Right";

        private const string RectOffsetTopName = "m_Top";

        private const string RectOffsetBottomName = "m_Bottom";

        private const string SpacingName = nameof(ScrollView.spacing);

        private const string ChildForceExpandSizeName = nameof(ScrollView.childForceExpandSize);

        private const string LeadingPlaceholderName = nameof(ScrollView.leadingPlaceholder);

        private const string TrailingPlaceholderName = nameof(ScrollView.trailingPlaceholder);

        private const string ScrollbarName = nameof(ScrollView.scrollbar);

        private const string ScrollbarVisibilityName = nameof(ScrollView.scrollbarVisibility);

        private const string LeadingActiveOffsetName = nameof(ScrollView.leadingActiveOffset);

        private const string TrailingActiveOffsetName = nameof(ScrollView.trailingActiveOffset);

        private const string SpeedLimitName = nameof(ScrollView.speedLimit);

        private const string SnapTriggerName = nameof(ScrollView.snapTrigger);

        private const string SnapSpeedThresholdName = nameof(ScrollView.snapSpeedThreshold);

        private const string SnapFindNormalizedViewportPositionName =
            nameof(ScrollView.snapFindNormalizedViewportPosition);

        private const string SnapIncludingSpacingName = nameof(ScrollView.snapIncludingSpacing);

        private const string SnapNormalizedItemPositionName = nameof(ScrollView.snapNormalizedItemPosition);

        private const string SnapJumpNormalizedViewportPositionName =
            nameof(ScrollView.snapJumpNormalizedViewportPosition);

        private const string SnapDurationModeName = nameof(ScrollView.snapDurationMode);

        private const string SnapSpeedName = nameof(ScrollView.snapSpeed);

        private const string SnapDurationName = nameof(ScrollView.snapDuration);

        private const string SnapInterpolationName = nameof(ScrollView.snapInterpolation);

        private const string ScrollSnapDelayName = nameof(ScrollView.scrollSnapDelay);

        private int _frameCount;

        private ScrollView _scrollView;

        private SerializedProperty _scrollRect;

        private SerializedProperty _inactiveContainer;

        private SerializedProperty _viewport;

        private SerializedProperty _content;

        private SerializedProperty _contentLayoutGroup;

        private SerializedProperty _padding;

        private SerializedProperty _paddingLeft;

        private SerializedProperty _paddingRight;

        private SerializedProperty _paddingTop;

        private SerializedProperty _paddingBottom;

        private SerializedProperty _spacing;

        private SerializedProperty _childForceExpandSize;

        private SerializedProperty _leadingPlaceholder;

        private SerializedProperty _trailingPlaceholder;

        private SerializedProperty _scrollbar;

        private SerializedProperty _scrollbarVisibility;

        private SerializedProperty _leadingActiveOffset;

        private SerializedProperty _trailingActiveOffset;

        private SerializedProperty _speedLimit;

        private SerializedProperty _snapTrigger;

        private SerializedProperty _snapSpeedThreshold;

        private SerializedProperty _snapFindNormalizedViewportPosition;

        private SerializedProperty _snapIncludingSpacing;

        private SerializedProperty _snapNormalizedItemPosition;

        private SerializedProperty _snapJumpNormalizedViewportPosition;

        private SerializedProperty _snapDurationMode;

        private SerializedProperty _snapSpeed;

        private SerializedProperty _snapDuration;

        private SerializedProperty _snapInterpolation;

        private SerializedProperty _scrollSnapDelay;

        private AnimBool _showSnapSpeedThreshold;

        private void OnEnable()
        {
            EditorApplication.update += RepaintIfFrameCountDifferent;

            _scrollView = (ScrollView) target;

            _scrollRect                         = serializedObject.FindProperty(ScrollRectName);
            _inactiveContainer                  = serializedObject.FindProperty(InactiveContainerName);
            _viewport                           = serializedObject.FindProperty(ViewportName);
            _content                            = serializedObject.FindProperty(ContentName);
            _contentLayoutGroup                 = serializedObject.FindProperty(ContentLayoutGroupName);
            _padding                            = serializedObject.FindProperty(PaddingName);
            _paddingLeft                        = _padding.FindPropertyRelative(RectOffsetLeftName);
            _paddingRight                       = _padding.FindPropertyRelative(RectOffsetRightName);
            _paddingTop                         = _padding.FindPropertyRelative(RectOffsetTopName);
            _paddingBottom                      = _padding.FindPropertyRelative(RectOffsetBottomName);
            _spacing                            = serializedObject.FindProperty(SpacingName);
            _childForceExpandSize               = serializedObject.FindProperty(ChildForceExpandSizeName);
            _leadingPlaceholder                 = serializedObject.FindProperty(LeadingPlaceholderName);
            _trailingPlaceholder                = serializedObject.FindProperty(TrailingPlaceholderName);
            _scrollbar                          = serializedObject.FindProperty(ScrollbarName);
            _scrollbarVisibility                = serializedObject.FindProperty(ScrollbarVisibilityName);
            _leadingActiveOffset                = serializedObject.FindProperty(LeadingActiveOffsetName);
            _trailingActiveOffset               = serializedObject.FindProperty(TrailingActiveOffsetName);
            _speedLimit                         = serializedObject.FindProperty(SpeedLimitName);
            _snapTrigger                        = serializedObject.FindProperty(SnapTriggerName);
            _snapSpeedThreshold                 = serializedObject.FindProperty(SnapSpeedThresholdName);
            _snapFindNormalizedViewportPosition = serializedObject.FindProperty(SnapFindNormalizedViewportPositionName);
            _snapIncludingSpacing               = serializedObject.FindProperty(SnapIncludingSpacingName);
            _snapNormalizedItemPosition         = serializedObject.FindProperty(SnapNormalizedItemPositionName);
            _snapJumpNormalizedViewportPosition = serializedObject.FindProperty(SnapJumpNormalizedViewportPositionName);
            _snapDurationMode                   = serializedObject.FindProperty(SnapDurationModeName);
            _snapSpeed                          = serializedObject.FindProperty(SnapSpeedName);
            _snapDuration                       = serializedObject.FindProperty(SnapDurationName);
            _snapInterpolation                  = serializedObject.FindProperty(SnapInterpolationName);
            _scrollSnapDelay                    = serializedObject.FindProperty(ScrollSnapDelayName);

            _showSnapSpeedThreshold = new AnimBool(RepaintIfFrameCountDifferent);
            SetAnimBool(true);
        }

        private void OnDisable()
        {
            EditorApplication.update -= RepaintIfFrameCountDifferent;
        }

        private void RepaintIfFrameCountDifferent()
        {
            if (_frameCount != Time.frameCount)
            {
                _frameCount = Time.frameCount;
                Repaint();
            }
        }

        public override void OnInspectorGUI()
        {
            SetAnimBool(false);

            _frameCount = Time.frameCount;

            serializedObject.Update();

            if (EditorApplication.isPlaying)
            {
                var controller = _scrollView.Controller;
                if (controller is MonoBehaviour controllerBehaviour && controllerBehaviour)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.ObjectField(
                            nameof(ScrollView.Controller),
                            controllerBehaviour,
                            typeof(MonoBehaviour),
                            true
                        );
                    }
                }
            }

            EditorGUILayout.PropertyField(_scrollRect);

            EditorGUILayout.PropertyField(_inactiveContainer);

            EditorGUILayout.PropertyField(_viewport);

            EditorGUILayout.PropertyField(_content);

            EditorGUILayout.PropertyField(_contentLayoutGroup);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_padding);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_scrollView, $"set {nameof(ScrollView.Padding)}");
                var left   = _paddingLeft.intValue = Mathf.Max(_paddingLeft.intValue,     0);
                var right  = _paddingRight.intValue = Mathf.Max(_paddingRight.intValue,   0);
                var top    = _paddingTop.intValue = Mathf.Max(_paddingTop.intValue,       0);
                var bottom = _paddingBottom.intValue = Mathf.Max(_paddingBottom.intValue, 0);
                _scrollView.Padding = new RectOffset(left, right, top, bottom);
                EditorUtility.SetDirty(_scrollView);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_spacing);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_scrollView, $"set {nameof(ScrollView.Spacing)}");
                _scrollView.Spacing = _spacing.floatValue;
                EditorUtility.SetDirty(_scrollView);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_childForceExpandSize);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_scrollView, $"set {nameof(ScrollView.ChildForceExpandSize)}");
                _scrollView.ChildForceExpandSize = _childForceExpandSize.boolValue;
                EditorUtility.SetDirty(_scrollView);
            }

            EditorGUILayout.PropertyField(_leadingPlaceholder);

            EditorGUILayout.PropertyField(_trailingPlaceholder);

            EditorGUILayout.PropertyField(_scrollbar);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_scrollbarVisibility);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_scrollView, $"set {nameof(ScrollView.ScrollbarVisibility)}");
                _scrollView.ScrollbarVisibility = (ScrollbarVisibility) _scrollbarVisibility.intValue;
                EditorUtility.SetDirty(_scrollView);
            }

            EditorGUILayout.PropertyField(_leadingActiveOffset);

            EditorGUILayout.PropertyField(_trailingActiveOffset);

            EditorGUILayout.PropertyField(_speedLimit);

            EditorGUILayout.PropertyField(_snapTrigger);

            if (EditorGUILayout.BeginFadeGroup(_showSnapSpeedThreshold.faded))
            {
                EditorGUILayout.PropertyField(_snapSpeedThreshold);
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.PropertyField(_snapFindNormalizedViewportPosition);

            EditorGUILayout.PropertyField(_snapIncludingSpacing);

            EditorGUILayout.PropertyField(_snapNormalizedItemPosition);

            EditorGUILayout.PropertyField(_snapJumpNormalizedViewportPosition);

            EditorGUILayout.PropertyField(_snapDurationMode);

            switch ((ScrollViewSnapDurationMode) _snapDurationMode.intValue)
            {
                case ScrollViewSnapDurationMode.Fixed:
                    EditorGUILayout.PropertyField(_snapDuration);
                    break;
                case ScrollViewSnapDurationMode.Dynamic:
                    EditorGUILayout.PropertyField(_snapSpeed);
                    break;
                default:
                    EditorGUILayout.PropertyField(_snapDuration);

                    EditorGUILayout.PropertyField(_snapSpeed);
                    break;
            }

            EditorGUILayout.PropertyField(_snapInterpolation);

            EditorGUILayout.PropertyField(_scrollSnapDelay);

            serializedObject.ApplyModifiedProperties();

            if (EditorApplication.isPlaying)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.FloatField("Viewport Size", _scrollView.ViewportSize);

                    if (EditorGUILayout.FloatField("Content Size", _scrollView.ContentSize) >= 10000000)
                    {
                        EditorGUILayout.HelpBox("Avoid making the content size too large!", MessageType.Warning);
                    }

                    EditorGUILayout.FloatField("Overflowed Content Size", _scrollView.OverflowedContentSize);

                    EditorGUILayout.FloatField("Content Position", _scrollView.ContentPosition);

                    EditorGUILayout.DoubleField("Normalized Scroll Position", _scrollView.NormalizedScrollPosition);

                    EditorGUILayout.Toggle(nameof(ScrollView.Dragging), _scrollView.Dragging);

                    EditorGUILayout.Toggle(nameof(ScrollView.Tweening), _scrollView.Tweening);

                    EditorGUILayout.Space();

                    EditorGUILayout.FloatField("Item Count", _scrollView.ItemCount);

                    var firstActiveIndex = _scrollView.FirstActiveIndex;
                    DrawScrollViewItemGui("First Active Item", firstActiveIndex);

                    var firstVisibleIndex = _scrollView.FirstVisibleIndex;
                    DrawScrollViewItemGui("First Visible Item", firstVisibleIndex);

                    var closestToCenterIndex = _scrollView.FindClosestIndex(
                        _scrollView.ConvertNormalizedViewportPositionToContentPosition(0.5f)
                    );
                    DrawScrollViewItemGui("Item Closest to Center", closestToCenterIndex);

                    var lastVisibleIndex = _scrollView.LastVisibleIndex;
                    DrawScrollViewItemGui("Last Visible Item", lastVisibleIndex);

                    var lastActiveIndex = _scrollView.LastActiveIndex;
                    DrawScrollViewItemGui("Last Active Item", lastActiveIndex);
                }
            }
        }

        private void DrawScrollViewItemGui(string label, int scrollViewItemIndex)
        {
            const float numFieldWidth = 38; // 4-digits number field max width
            const float spacing       = 2;

            var rect = EditorGUILayout.GetControlRect();

            rect = EditorGUI.PrefixLabel(rect, EditorGUIUtility.TrTextContent(label));

            var width = rect.width;
            rect.width = numFieldWidth;
            EditorGUI.IntField(rect, scrollViewItemIndex);

            rect.x     += numFieldWidth + spacing;
            rect.width =  width - (numFieldWidth + spacing);
            var scrollViewItem = scrollViewItemIndex >= 0 ? _scrollView[scrollViewItemIndex] : null;
            EditorGUI.ObjectField(rect, scrollViewItem, typeof(ScrollViewItem), true);
        }

        private void SetAnimBool(bool instant)
        {
            SetAnimBool(
                _showSnapSpeedThreshold,
                ((ScrollViewSnapTrigger) _snapTrigger.intValue &
                 ScrollViewSnapTrigger.OnNormalizedScrollPositionChanged) != 0,
                instant
            );
        }

        private static void SetAnimBool(AnimBool animBool, bool value, bool instant)
        {
            if (instant)
            {
                animBool.value = value;
            }
            else
            {
                animBool.target = value;
            }
        }
    }
}
