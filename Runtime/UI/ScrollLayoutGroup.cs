using System;
using UnityEngine;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 滚动布局组。
    /// </summary>
    [RequireComponent(typeof(RectMask2D))]
    public sealed class ScrollLayoutGroup : LayoutGroup
    {
        /// <summary>
        /// 是否让子物体沿水平方向排列？
        /// </summary>
        public bool horizontal;

        [SerializeField]
        private float centerIndex;

        private RectMask2D _rectMask2D;

        private Vector2 _maxMinSize;

        private Vector2 _maxPreferredSize;

        /// <summary>
        /// 获取或设置位于此布局组的中心的布局元素的索引。
        /// </summary>
        public float CenterIndex { get => centerIndex; set => SetCenterIndex(value); }

        /// <summary>
        /// 此组件所在游戏物体上的二维矩形遮罩。
        /// </summary>
        public RectMask2D RectMask2D => _rectMask2D != null ? _rectMask2D : _rectMask2D = GetComponent<RectMask2D>();

        /// <summary>
        /// 参与布局的子物体数量。
        /// </summary>
        public int LayoutChildrenCount => rectChildren.Count;

        private static void GetMinSizeAndPreferredSize(
            RectTransform child,
            int           axis,
            out float     minSize,
            out float     preferredSize)
        {
            minSize       = LayoutUtility.GetMinSize(child, axis);
            preferredSize = LayoutUtility.GetPreferredSize(child, axis);
        }

        /// <summary>
        /// 获取当前处于中间的子布局元素。
        /// </summary>
        public RectTransform CurrentCenter
        {
            get
            {
                try
                {
                    return rectChildren[Mathf.RoundToInt(centerIndex)];
                }
                catch
                {
                    return null;
                }
            }
        }

        private void SetCenterIndex(float value)
        {
            if (centerIndex == value)
            {
                return;
            }
            centerIndex = value;
            SetDirty();
        }

        /// <summary>
        /// 将指定的是布局元素的子物体居中。
        /// </summary>
        /// <param name="child">一个是布局元素的子物体。</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="child"/> 为 <see langword="null"/> .</exception>
        /// <exception cref="System.ArgumentException"><paramref name="child"/> 不是布局元素，或不是这个矩形变换的子物体。</exception>
        public void SetLayoutChildToCenter(RectTransform child)
        {
            SetCenterIndex(GetLayoutChildIndex(child));
        }

        /// <summary>
        /// 获取指定的是布局元素的子物体在此布局组的所有布局元素中的索引。
        /// </summary>
        /// <param name="child">一个是布局元素的子物体。</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="child"/> 为 <see langword="null"/> .</exception>
        /// <exception cref="System.ArgumentException"><paramref name="child"/> 不是布局元素，或不是这个矩形变换的子物体。</exception>
        public int GetLayoutChildIndex(RectTransform child)
        {
            if (child == null)
            {
                throw new ArgumentNullException(nameof(child));
            }
            var index = rectChildren.IndexOf(child);
            if (index == -1)
            {
                throw new ArgumentException(null, nameof(child));
            }
            return index;
        }

        /// <inheritdoc />
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            _maxMinSize       = Vector2.zero;
            _maxPreferredSize = Vector2.zero;
            foreach (var child in rectChildren)
            {
                for (var i = 0; i < 2; i++)
                {
                    GetMinSizeAndPreferredSize(child, i, out var minSize, out var preferredSize);
                    _maxMinSize[i]       = Mathf.Max(_maxMinSize[i],       minSize);
                    _maxPreferredSize[i] = Mathf.Max(_maxPreferredSize[i], preferredSize);
                }
            }
            SetLayoutInputForAxis(_maxMinSize[0] + padding.horizontal, _maxPreferredSize[0] + padding.horizontal, 0, 0);
        }

        /// <inheritdoc />
        public override void CalculateLayoutInputVertical()
        {
            SetLayoutInputForAxis(_maxMinSize[1] + padding.vertical, _maxPreferredSize[1] + padding.vertical, 0, 1);
        }

        /// <inheritdoc />
        public override void SetLayoutHorizontal()
        {
            if (horizontal)
            {
                var maxPreferredWidth = _maxPreferredSize[0];
                for (var i = 0; i < rectChildren.Count; i++)
                {
                    var child = rectChildren[i];
                    GetMinSizeAndPreferredSize(child, 0, out _, out var preferredSize);
                    var pos = GetStartOffset(0, preferredSize) +
                              (padding.horizontal + maxPreferredWidth) * (i - centerIndex);
                    SetChildAlongAxis(child, 0, pos, preferredSize);
                }
            }
            else
            {
                foreach (var rectChild in rectChildren)
                {
                    GetMinSizeAndPreferredSize(rectChild, 0, out _, out var preferredSize);
                    SetChildAlongAxis(rectChild, 0, GetStartOffset(0, preferredSize), preferredSize);
                }
            }
        }

        /// <inheritdoc />
        public override void SetLayoutVertical()
        {
            if (horizontal)
            {
                foreach (var rectChild in rectChildren)
                {
                    GetMinSizeAndPreferredSize(rectChild, 1, out _, out var preferredSize);
                    SetChildAlongAxis(rectChild, 1, GetStartOffset(1, preferredSize), preferredSize);
                }
            }
            else
            {
                var maxPreferredHeight = _maxPreferredSize[1];
                for (var i = 0; i < rectChildren.Count; i++)
                {
                    var child = rectChildren[i];
                    GetMinSizeAndPreferredSize(child, 1, out _, out var preferredSize);
                    var pos = GetStartOffset(1, preferredSize) +
                              (padding.vertical + maxPreferredHeight) * (i - centerIndex);
                    SetChildAlongAxis(child, 1, pos, preferredSize);
                }
            }
        }
    }
}
