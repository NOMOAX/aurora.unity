using Aurora.Unity.UI;
using UnityEditor;
using UnityEngine;

namespace Aurora.UnityEditor.UI
{
    [CustomEditor(typeof(ScrollView), true)]
    internal class ScrollViewEditor : Editor
    {
        private int _frameCount;

        private ScrollView _scrollView;

        private SerializedProperty _spacing;

        private SerializedProperty _itemForceExpand;

        private SerializedProperty _leadingActiveOffset;

        private SerializedProperty _trailingActiveOffset;

        private SerializedProperty _speedLimit;

        private SerializedProperty _stopTween;

        private SerializedProperty _snapTrigger;

        private SerializedProperty _snapDurationMode;

        private SerializedProperty _snapDuration;

        private SerializedProperty _snapSpeed;

        private SerializedProperty _scrollbarVisibility;

        private static bool _showPadding = true;

        private void OnEnable()
        {
            EditorApplication.update += RepaintIfFrameCountDifferent;

            _scrollView = (ScrollView) target;

            _spacing              = serializedObject.FindProperty(nameof(ScrollView.spacing));
            _itemForceExpand      = serializedObject.FindProperty(nameof(ScrollView.itemForceExpand));
            _leadingActiveOffset  = serializedObject.FindProperty(nameof(ScrollView.leadingActiveOffset));
            _trailingActiveOffset = serializedObject.FindProperty(nameof(ScrollView.trailingActiveOffset));
            _speedLimit           = serializedObject.FindProperty(nameof(ScrollView.speedLimit));
            _stopTween            = serializedObject.FindProperty(nameof(ScrollView.stopTween));
            _snapTrigger          = serializedObject.FindProperty(nameof(ScrollView.snapTrigger));
            _snapDurationMode     = serializedObject.FindProperty(nameof(ScrollView.snapDurationMode));
            _snapDuration         = serializedObject.FindProperty(nameof(ScrollView.snapDuration));
            _snapSpeed            = serializedObject.FindProperty(nameof(ScrollView.snapSpeed));
            _scrollbarVisibility  = serializedObject.FindProperty(nameof(ScrollView.scrollbarVisibility));
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
            _frameCount = Time.frameCount;

            var initialized = _scrollView.Initialized;

            serializedObject.Update();

            // 由于 RectOffset 的特殊性，要限制它的值，需要手动绘制
            if (_showPadding = EditorGUILayout.Foldout(_showPadding, nameof(ScrollView.Padding)))
            {
                var padding = _scrollView.padding ?? new RectOffset();
                EditorGUI.BeginChangeCheck();
                EditorGUI.indentLevel++;
                padding.left   = Mathf.Max(EditorGUILayout.IntField("Left",   padding.left),   0);
                padding.right  = Mathf.Max(EditorGUILayout.IntField("Right",  padding.right),  0);
                padding.top    = Mathf.Max(EditorGUILayout.IntField("Top",    padding.top),    0);
                padding.bottom = Mathf.Max(EditorGUILayout.IntField("Bottom", padding.bottom), 0);
                EditorGUI.indentLevel--;
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_scrollView, $"Set {nameof(ScrollView.Padding)}");
                    _scrollView.Padding = padding;
                    EditorUtility.SetDirty(_scrollView);
                }
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_spacing);
            if (EditorGUI.EndChangeCheck())
            {
                _scrollView.Spacing = _spacing.floatValue;
            }

            using (new EditorGUI.DisabledScope(initialized))
            {
                EditorGUILayout.PropertyField(_itemForceExpand);
            }

            EditorGUILayout.PropertyField(_leadingActiveOffset);

            EditorGUILayout.PropertyField(_trailingActiveOffset);

            EditorGUILayout.PropertyField(_speedLimit);

            EditorGUILayout.PropertyField(_stopTween);

            EditorGUILayout.PropertyField(_snapTrigger);
            if ((ScrollViewSnapTrigger) _snapTrigger.intValue == ScrollViewSnapTrigger.AllowWhileDragging)
            {
                EditorGUILayout.HelpBox(
                    $"Has no effect when use {nameof(ScrollViewSnapTrigger.AllowWhileDragging)} alone.",
                    MessageType.Warning,
                    EditorGUIUtility.wideMode
                );
            }

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

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_scrollbarVisibility);
            if (EditorGUI.EndChangeCheck())
            {
                _scrollView.ScrollbarVisibility = (ScrollbarVisibility) _scrollbarVisibility.intValue;
            }

            serializedObject.ApplyModifiedProperties();

            if (EditorApplication.isPlaying)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.Toggle(nameof(ScrollView.Initialized), initialized);
                    if (initialized)
                    {
                        EditorGUILayout.FloatField("Content Position",           _scrollView.ContentPosition);
                        EditorGUILayout.FloatField("Normalized Scroll Position", _scrollView.NormalizedScrollPosition);

                        EditorGUILayout.Space();

                        EditorGUILayout.FloatField(nameof(ScrollView.ItemCount), _scrollView.ItemCount);

                        var firstActiveIndex = _scrollView.FirstActiveIndex;
                        DrawScrollViewItemGui("First Active Item", firstActiveIndex);

                        var firstVisibleIndex = _scrollView.FirstVisibleIndex;
                        DrawScrollViewItemGui("First Visible Item", firstVisibleIndex);

                        var closestToCenterIndex = _scrollView.FindClosestIndex(
                            _scrollView.ConvertNormalizedPositionToContentPosition(0.5f)
                        );
                        DrawScrollViewItemGui("Item Closest to Center", closestToCenterIndex);

                        var lastVisibleIndex = _scrollView.LastVisibleIndex;
                        DrawScrollViewItemGui("Last Visible Item", lastVisibleIndex);

                        var lastActiveIndex = _scrollView.LastActiveIndex;
                        DrawScrollViewItemGui("Last Active Item", lastActiveIndex);

                        EditorGUILayout.Toggle(nameof(ScrollView.Dragging), _scrollView.Dragging);
                        EditorGUILayout.Toggle(nameof(ScrollView.Tweening), _scrollView.Tweening);
                    }
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
    }
}
