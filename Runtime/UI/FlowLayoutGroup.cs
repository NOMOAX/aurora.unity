using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 流式布局组。
    /// </summary>
    public sealed class FlowLayoutGroup : LayoutGroup
    {
        [SerializeField]
        private RectTransform.Axis axis;

        [SerializeField]
        private float preferredSizeAloneAxis;

        [SerializeField]
        private Vector2 spacing;

        private readonly Dictionary<RectTransform, Vector2> _dictionary = new Dictionary<RectTransform, Vector2>(
            UnityEngineObjectEqualityComparer.Instance
        );

        private readonly List<List<RectTransform>> _lines = new List<List<RectTransform>>();

        /// <summary>
        /// 主轴。
        /// </summary>
        public RectTransform.Axis Axis { get => axis; set => SetProperty(ref axis, value); }

        /// <summary>
        /// 沿主轴方向的首选尺寸。
        /// </summary>
        public float PreferredSizeAloneAxis
        {
            get => preferredSizeAloneAxis;
            set => SetProperty(ref preferredSizeAloneAxis, value);
        }

        /// <summary>
        /// 间距。
        /// </summary>
        public Vector2 Spacing { get => spacing; set => SetProperty(ref spacing, value); }

        /// <summary>
        /// 行（或列）数。
        /// </summary>
        public int LineCount => _lines.Count;

        /// <summary>
        /// 获取指定的子布局元素的索引。
        /// </summary>
        /// <param name="layoutChild">子布局元素。</param>
        /// <param name="indexAloneAxis"><paramref name="layoutChild"/> 沿 <see cref="Axis"/> 轴的索引。</param>
        /// <param name="indexAloneOtherAxis"><paramref name="layoutChild"/> 沿另一轴的索引。</param>
        /// <returns>指定的子布局元素的索引。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="layoutChild"/> 为 <see langword="null"/>。</exception>
        public bool TryGetIndexOf(RectTransform layoutChild, out int indexAloneAxis, out int indexAloneOtherAxis)
        {
            if (layoutChild == null)
            {
                throw new ArgumentNullException(nameof(layoutChild));
            }
            var lineCount = _lines.Count;
            for (indexAloneAxis = 0; indexAloneAxis < lineCount; indexAloneAxis++)
            {
                var line                   = _lines[indexAloneAxis];
                var layoutChildCountOfLine = line.Count;
                for (indexAloneOtherAxis = 0; indexAloneOtherAxis < layoutChildCountOfLine; indexAloneOtherAxis++)
                {
                    var element = line[indexAloneOtherAxis];
                    if (element == layoutChild)
                    {
                        return true;
                    }
                }
            }
            indexAloneAxis      = -1;
            indexAloneOtherAxis = -1;
            return false;
        }

        /// <summary>
        /// 获取位于指定行（或列）的所有子布局元素。
        /// </summary>
        /// <param name="lineIndex">行（或列）索引。</param>
        /// <param name="results">用于存放结果的列表。</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="lineIndex"/> 小于 0，或大于等于行（或列）数。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="results"/> 为 <see langword="null"/>。</exception>
        public void GetLayoutChildrenOfLine(int lineIndex, List<RectTransform> results)
        {
            if (lineIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(lineIndex), lineIndex, null);
            }
            var lineCount = _lines.Count;
            if (lineIndex >= lineCount)
            {
                throw new ArgumentOutOfRangeException(nameof(lineIndex), lineIndex, null);
            }
            if (results is null)
            {
                throw new ArgumentNullException(nameof(results));
            }
            results.AddRange(_lines[lineIndex]);
        }

        private void GetChildPreferredSizes()
        {
            _dictionary.Clear();
            foreach (var child in rectChildren)
            {
                var preferredSize = Vector2.zero;
                for (var i = 0; i < 2; i++)
                {
                    preferredSize[i] = LayoutUtility.GetPreferredSize(child, i);
                }
                _dictionary[child] = preferredSize;
            }
        }

        private void GetLines()
        {
            _lines.Clear();
            var availableSize = rectTransform.rect.size[(int) axis] -
                                ((int) axis == 0 ? padding.horizontal : padding.vertical);
            var line     = new List<RectTransform>();
            var lineSize = 0f;
            foreach (var child in rectChildren)
            {
                _dictionary.TryGetValue(child, out var preferredSize);
                if (line.Count == 0)
                {
                    line.Add(child);
                    lineSize = preferredSize[(int) axis];
                }
                else
                {
                    var newLineSize = lineSize + spacing[(int) axis] + preferredSize[(int) axis];
                    if (newLineSize <= availableSize)
                    {
                        line.Add(child);
                        lineSize = newLineSize;
                    }
                    else
                    {
                        // 该行结束
                        _lines.Add(line);
                        // 新起一行
                        line = new List<RectTransform>
                        {
                            child
                        };
                        lineSize = preferredSize[(int) axis];
                    }
                }
            }
            if (line.Count > 0)
            {
                _lines.Add(line);
            }
        }

        /// <inheritdoc />
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            GetChildPreferredSizes();
            GetLines();
            float totalPreferredSize;
            switch (axis)
            {
                case RectTransform.Axis.Horizontal:
                    totalPreferredSize = preferredSizeAloneAxis;
                    break;
                case RectTransform.Axis.Vertical:
                    totalPreferredSize = padding.horizontal;
                    for (var i = 0; i < _lines.Count; i++)
                    {
                        if (i > 0)
                        {
                            totalPreferredSize += spacing[0];
                        }
                        var line             = _lines[i];
                        var maxPreferredSize = 0f;
                        foreach (var child in line)
                        {
                            _dictionary.TryGetValue(child, out var preferredSize);
                            maxPreferredSize = Mathf.Max(preferredSize[0], maxPreferredSize);
                        }
                        totalPreferredSize += maxPreferredSize;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SetLayoutInputForAxis(-1f, totalPreferredSize, -1f, 0);
        }

        /// <inheritdoc />
        public override void CalculateLayoutInputVertical()
        {
            float totalPreferredSize;
            switch (axis)
            {
                case RectTransform.Axis.Horizontal:
                    totalPreferredSize = padding.vertical;
                    for (var i = 0; i < _lines.Count; i++)
                    {
                        if (i > 0)
                        {
                            totalPreferredSize += spacing[1];
                        }
                        var line             = _lines[i];
                        var maxPreferredSize = 0f;
                        foreach (var child in line)
                        {
                            _dictionary.TryGetValue(child, out var preferredSize);
                            maxPreferredSize = Mathf.Max(preferredSize[1], maxPreferredSize);
                        }
                        totalPreferredSize += maxPreferredSize;
                    }
                    break;
                case RectTransform.Axis.Vertical:
                    totalPreferredSize = preferredSizeAloneAxis;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SetLayoutInputForAxis(-1f, totalPreferredSize, -1f, 1);
        }

        /// <inheritdoc />
        public override void SetLayoutHorizontal()
        {
            switch (axis)
            {
                case RectTransform.Axis.Horizontal:
                {
                    foreach (var line in _lines)
                    {
                        var position = (float) padding.left;
                        for (var i = 0; i < line.Count; i++)
                        {
                            var child = line[i];
                            _dictionary.TryGetValue(child, out var preferredSize);
                            if (i > 0)
                            {
                                position += spacing[0];
                            }
                            SetChildAlongAxis(
                                child,
                                0,
                                position,
                                Mathf.Min(
                                    preferredSize[0],
                                    Mathf.Max(rectTransform.rect.size[0] - padding.horizontal, 0f)
                                )
                            );
                            position += preferredSize[0];
                        }
                    }
                    break;
                }
                case RectTransform.Axis.Vertical:
                {
                    var position = (float) padding.left;
                    for (var i = 0; i < _lines.Count; i++)
                    {
                        if (i > 0)
                        {
                            position += spacing[0];
                        }
                        var line             = _lines[i];
                        var maxPreferredSize = 0f;
                        foreach (var child in line)
                        {
                            _dictionary.TryGetValue(child, out var preferredSize);
                            SetChildAlongAxis(child, 0, position, preferredSize[0]);
                            maxPreferredSize = Mathf.Max(preferredSize[0], maxPreferredSize);
                        }
                        position += maxPreferredSize;
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <inheritdoc />
        public override void SetLayoutVertical()
        {
            switch (axis)
            {
                case RectTransform.Axis.Horizontal:
                {
                    var position = (float) padding.top;
                    for (var i = 0; i < _lines.Count; i++)
                    {
                        if (i > 0)
                        {
                            position += spacing[1];
                        }
                        var line             = _lines[i];
                        var maxPreferredSize = 0f;
                        foreach (var child in line)
                        {
                            _dictionary.TryGetValue(child, out var preferredSize);
                            SetChildAlongAxis(child, 1, position, preferredSize[1]);
                            maxPreferredSize = Mathf.Max(preferredSize[1], maxPreferredSize);
                        }
                        position += maxPreferredSize;
                    }
                    break;
                }
                case RectTransform.Axis.Vertical:
                {
                    foreach (var line in _lines)
                    {
                        var position = (float) padding.top;
                        for (var i = 0; i < line.Count; i++)
                        {
                            var child = line[i];
                            _dictionary.TryGetValue(child, out var preferredSize);
                            if (i > 0)
                            {
                                position += spacing[1];
                            }
                            SetChildAlongAxis(
                                child,
                                1,
                                position,
                                Mathf.Min(
                                    preferredSize[1],
                                    Mathf.Max(rectTransform.rect.size[1] - padding.vertical, 0f)
                                )
                            );
                            position += preferredSize[1];
                        }
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
