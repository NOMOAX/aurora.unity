using System;
using UnityEngine;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// A scroll layout group.
    /// </summary>
    [RequireComponent(typeof(RectMask2D))]
    public sealed class ScrollLayoutGroup : LayoutGroup
    {
        /// <summary>
        /// Whether to arrange child objects along the horizontal axis?
        /// </summary>
        public bool horizontal;

        [SerializeField]
        private float centerIndex;

        private RectMask2D _rectMask2D;

        private Vector2 _maxMinSize;

        private Vector2 _maxPreferredSize;

        /// <summary>
        /// Gets or sets the index of the layout element at the center of this layout group.
        /// </summary>
        public float CenterIndex { get => centerIndex; set => SetCenterIndex(value); }

        /// <summary>
        /// The 2D rectangular mask on the game object this component belongs to.
        /// </summary>
        public RectMask2D RectMask2D => _rectMask2D ??= GetComponent<RectMask2D>();

        /// <summary>
        /// The number of child objects participating in layout.
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
        /// Gets the child layout element currently in the middle.
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
        /// Centers the specified child layout element.
        /// </summary>
        /// <param name="child">A child object that is a layout element.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="child"/> is <see langword="null"/> .</exception>
        /// <exception cref="System.ArgumentException"><paramref name="child"/> is not a layout element, or is not a child of this rectangular transform.</exception>
        public void SetLayoutChildToCenter(RectTransform child)
        {
            SetCenterIndex(GetLayoutChildIndex(child));
        }

        /// <summary>
        /// Gets the index of the specified child layout element among all layout elements of this layout group.
        /// </summary>
        /// <param name="child">A child object that is a layout element.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="child"/> is <see langword="null"/> .</exception>
        /// <exception cref="System.ArgumentException"><paramref name="child"/> is not a layout element, or is not a child of this rectangular transform.</exception>
        public int GetLayoutChildIndex(RectTransform child)
        {
            if (!child)
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
