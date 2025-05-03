using UnityEngine;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 有且仅有一个布局元素的布局组。
    /// </summary>
    public abstract class OneElementLayoutGroup : LayoutGroup
    {
        /// <summary>
        /// 判断是否有且仅有一个布局元素。
        /// </summary>
        protected internal bool ContainsExactlyOneElement()
        {
            return rectChildren.Count == 1;
        }

        /// <inheritdoc />
        public sealed override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            if (ContainsExactlyOneElement())
            {
                CalculateSingleElementLayoutInputHorizontal(rectChildren[0]);
            }
            else
            {
                SetLayoutInputForAxis(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// 计算唯一布局元素的水平布局输入。
        /// </summary>
        /// <param name="element">唯一布局元素。</param>
        protected abstract void CalculateSingleElementLayoutInputHorizontal(RectTransform element);

        /// <inheritdoc />
        public sealed override void CalculateLayoutInputVertical()
        {
            if (ContainsExactlyOneElement())
            {
                CalculateSingleElementLayoutInputVertical(rectChildren[0]);
            }
            else
            {
                SetLayoutInputForAxis(0, 0, 0, 1);
            }
        }

        /// <summary>
        /// 计算唯一布局元素的垂直布局输入。
        /// </summary>
        /// <param name="element">唯一布局元素。</param>
        protected abstract void CalculateSingleElementLayoutInputVertical(RectTransform element);

        /// <inheritdoc />
        public sealed override void SetLayoutHorizontal()
        {
            if (ContainsExactlyOneElement())
            {
                var singleElement = rectChildren[0];
                SetSingleElementLayoutHorizontal(singleElement);
            }
        }

        /// <summary>
        /// 设置唯一布局元素的水平布局。
        /// </summary>
        /// <param name="element">唯一布局元素。</param>
        protected abstract void SetSingleElementLayoutHorizontal(RectTransform element);

        /// <inheritdoc />
        public sealed override void SetLayoutVertical()
        {
            if (ContainsExactlyOneElement())
            {
                var singleElement = rectChildren[0];
                SetSingleElementLayoutVertical(singleElement);
            }
        }

        /// <summary>
        /// 设置唯一布局元素的垂直布局。
        /// </summary>
        /// <param name="element">唯一布局元素。</param>
        protected abstract void SetSingleElementLayoutVertical(RectTransform element);
    }
}
