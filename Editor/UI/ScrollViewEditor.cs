// @formatter:max_line_length 10000

using System.Globalization;
using Aurora.Unity.UI;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Aurora.UnityEditor.UI
{
    [CustomEditor(typeof(ScrollView), true)]
    internal class ScrollViewEditor : Editor
    {
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
            EditorApplication.update += RepaintIfDirty;

            _scrollView = (ScrollView) target;

            _scrollRect                         = serializedObject.FindProperty(nameof(ScrollView.scrollRect));
            _inactiveContainer                  = serializedObject.FindProperty(nameof(ScrollView.inactiveContainer));
            _viewport                           = serializedObject.FindProperty(nameof(ScrollView.viewport));
            _content                            = serializedObject.FindProperty(nameof(ScrollView.content));
            _contentLayoutGroup                 = serializedObject.FindProperty(nameof(ScrollView.contentLayoutGroup));
            _padding                            = serializedObject.FindProperty(nameof(ScrollView.padding));
            _paddingLeft                        = _padding.FindPropertyRelative("m_Left");
            _paddingRight                       = _padding.FindPropertyRelative("m_Right");
            _paddingTop                         = _padding.FindPropertyRelative("m_Top");
            _paddingBottom                      = _padding.FindPropertyRelative("m_Bottom");
            _spacing                            = serializedObject.FindProperty(nameof(ScrollView.spacing));
            _childForceExpandSize               = serializedObject.FindProperty(nameof(ScrollView.childForceExpandSize));
            _leadingPlaceholder                 = serializedObject.FindProperty(nameof(ScrollView.leadingPlaceholder));
            _trailingPlaceholder                = serializedObject.FindProperty(nameof(ScrollView.trailingPlaceholder));
            _scrollbar                          = serializedObject.FindProperty(nameof(ScrollView.scrollbar));
            _scrollbarVisibility                = serializedObject.FindProperty(nameof(ScrollView.scrollbarVisibility));
            _leadingActiveOffset                = serializedObject.FindProperty(nameof(ScrollView.leadingActiveOffset));
            _trailingActiveOffset               = serializedObject.FindProperty(nameof(ScrollView.trailingActiveOffset));
            _speedLimit                         = serializedObject.FindProperty(nameof(ScrollView.speedLimit));
            _snapTrigger                        = serializedObject.FindProperty(nameof(ScrollView.snapTrigger));
            _snapSpeedThreshold                 = serializedObject.FindProperty(nameof(ScrollView.snapSpeedThreshold));
            _snapFindNormalizedViewportPosition = serializedObject.FindProperty(nameof(ScrollView.snapFindNormalizedViewportPosition));
            _snapIncludingSpacing               = serializedObject.FindProperty(nameof(ScrollView.snapIncludingSpacing));
            _snapNormalizedItemPosition         = serializedObject.FindProperty(nameof(ScrollView.snapNormalizedItemPosition));
            _snapJumpNormalizedViewportPosition = serializedObject.FindProperty(nameof(ScrollView.snapJumpNormalizedViewportPosition));
            _snapDurationMode                   = serializedObject.FindProperty(nameof(ScrollView.snapDurationMode));
            _snapSpeed                          = serializedObject.FindProperty(nameof(ScrollView.snapSpeed));
            _snapDuration                       = serializedObject.FindProperty(nameof(ScrollView.snapDuration));
            _snapInterpolation                  = serializedObject.FindProperty(nameof(ScrollView.snapInterpolation));
            _scrollSnapDelay                    = serializedObject.FindProperty(nameof(ScrollView.scrollSnapDelay));

            _showSnapSpeedThreshold = new AnimBool(RepaintIfDirty);
            SetAnimBool(true);
        }

        private void OnDisable()
        {
            EditorApplication.update -= RepaintIfDirty;
        }

        private void RepaintIfDirty()
        {
            if (!_scrollView)
            {
                return;
            }
            if (_scrollView.Dirty)
            {
                _scrollView.Dirty = false;
                Repaint();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SetAnimBool(false);

            if (EditorApplication.isPlaying)
            {
                var controller = _scrollView.Controller;
                if (controller is MonoBehaviour controllerBehaviour && controllerBehaviour)
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.ObjectField(nameof(ScrollView.Controller), controllerBehaviour, typeof(MonoBehaviour), true);
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
                    DrawReadonlyValueGui("Viewport Size", _scrollView.ViewportSize);

                    DrawReadonlyValueGui("Content Size", _scrollView.ContentSize);
                    if (_scrollView.ContentSize >= 10000000)
                    {
                        EditorGUILayout.HelpBox("Avoid making the content size too large!", MessageType.Warning);
                    }

                    DrawReadonlyValueGui("Overflowed Content Size", _scrollView.OverflowedContentSize);

                    DrawReadonlyValueGui("Content Position", _scrollView.ContentPosition);

                    DrawReadonlyValueGui("Normalized Scroll Position", _scrollView.NormalizedScrollPosition);

                    EditorGUILayout.Toggle(nameof(ScrollView.Dragging), _scrollView.Dragging);

                    EditorGUILayout.Toggle(nameof(ScrollView.Tweening), _scrollView.Tweening);

                    DrawReadonlyValueGui("Item Count", _scrollView.ItemCount);

                    DrawScrollViewItemGui("First Active Item", _scrollView, _scrollView.FirstActiveIndex);

                    DrawScrollViewItemGui("First Visible Item", _scrollView, _scrollView.FirstVisibleIndex);

                    DrawScrollViewItemGui("Item Closest to Center", _scrollView, _scrollView.FindClosestIndex(_scrollView.ConvertNormalizedViewportPositionToContentPosition(0.5f)));

                    DrawScrollViewItemGui("Last Visible Item", _scrollView, _scrollView.LastVisibleIndex);

                    DrawScrollViewItemGui("Last Active Item", _scrollView, _scrollView.LastActiveIndex);
                }
            }
        }

        private static void DrawReadonlyValueGui(string label, int value)
        {
            var rect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), EditorGUIUtility.TrTempContent(label));

            EditorGUI.SelectableLabel(rect, value.ToString(NumberFormatInfo.InvariantInfo));
        }

        private static void DrawReadonlyValueGui(string label, float value)
        {
            var rect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), EditorGUIUtility.TrTempContent(label));

            EditorGUI.SelectableLabel(rect, value.ToString("g9", NumberFormatInfo.InvariantInfo));
        }

        private static void DrawReadonlyValueGui(string label, double value)
        {
            var rect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), EditorGUIUtility.TrTempContent(label));

            EditorGUI.SelectableLabel(rect, value.ToString("g17", NumberFormatInfo.InvariantInfo));
        }

        private static void DrawScrollViewItemGui(string label, ScrollView scrollView, int scrollViewItemIndex)
        {
            const float controlLabelWidth = 34; // 4-digits integer string max width of control label
            const float spacing           = 3;

            var rect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), EditorGUIUtility.TrTextContent(label));

            var width = rect.width;
            rect.width = controlLabelWidth;
            EditorGUI.SelectableLabel(rect, scrollViewItemIndex.ToString(NumberFormatInfo.InvariantInfo));

            rect.x     += controlLabelWidth + spacing;
            rect.width =  width - (controlLabelWidth + spacing);
            var scrollViewItem = scrollViewItemIndex >= 0 ? scrollView[scrollViewItemIndex] : null;
            EditorGUI.ObjectField(rect, scrollViewItem, typeof(ScrollViewItem), true);
        }

        private void SetAnimBool(bool instant)
        {
            SetAnimBool(_showSnapSpeedThreshold, ((ScrollViewSnapTrigger) _snapTrigger.intValue & ScrollViewSnapTrigger.OnNormalizedScrollPositionChanged) != 0, instant);
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
