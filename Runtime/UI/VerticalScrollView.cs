using UnityEngine;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 垂直滚动视图。
    /// </summary>
    public sealed class VerticalScrollView : ScrollView
    {
        /// <inheritdoc />
        protected override float PaddingAlongAxis => padding.vertical;

        /// <inheritdoc />
        protected override float FirstPaddingAlongAxis => padding.top;

        /// <inheritdoc />
        protected override float LastPaddingAlongAxis => padding.bottom;

        /// <inheritdoc />
        protected override float Get(Vector2 vector2)
        {
            return vector2.y;
        }

        /// <inheritdoc />
        protected override float GetContentPosition(Vector2 contentAnchoredPosition)
        {
            return contentAnchoredPosition.y;
        }

        /// <inheritdoc />
        protected override Vector2 Set(Vector2 vector2, float value)
        {
            vector2.y = value;
            return vector2;
        }

        /// <inheritdoc />
        protected override Vector2 SetContentPosition(Vector2 contentAnchoredPosition, float contentPosition)
        {
            contentAnchoredPosition.y = contentPosition;
            return contentAnchoredPosition;
        }

        /// <inheritdoc />
        protected override void SetMinSize(LayoutElement layoutElement, float minSize)
        {
            layoutElement.minHeight = minSize;
        }

        /// <inheritdoc />
        protected override void SetLayoutGroupChildForceExpandSize(
            HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup,
            bool                            forceExpand)
        {
            horizontalOrVerticalLayoutGroup.childForceExpandWidth = forceExpand;
        }

        /// <inheritdoc />
        protected override void SetScrollRectScrollbar(ScrollRect sr, Scrollbar sb)
        {
            sr.verticalScrollbar = sb;
        }
    }
}
