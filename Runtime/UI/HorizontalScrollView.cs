using UnityEngine;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 水平滚动视图。
    /// </summary>
    public sealed class HorizontalScrollView : ScrollView
    {
        /// <inheritdoc />
        protected override float PaddingAlongAxis => padding.horizontal;

        /// <inheritdoc />
        protected override float FirstPaddingAlongAxis => padding.left;

        /// <inheritdoc />
        protected override float LastPaddingAlongAxis => padding.right;

        /// <inheritdoc />
        protected override float Get(Vector2 vector2)
        {
            return vector2.x;
        }

        /// <inheritdoc />
        protected override Vector2 Set(Vector2 vector2, float value)
        {
            vector2.x = value;
            return vector2;
        }

        /// <inheritdoc />
        protected override void SetMinSize(LayoutElement layoutElement, float minSize)
        {
            layoutElement.minWidth = minSize;
        }

        /// <inheritdoc />
        protected override void SetLayoutGroupChildForceExpandSize(
            HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup,
            bool                            forceExpand)
        {
            horizontalOrVerticalLayoutGroup.childForceExpandHeight = forceExpand;
        }

        /// <inheritdoc />
        protected override void SetScrollRectScrollbar(ScrollRect sr, Scrollbar sb)
        {
            sr.horizontalScrollbar = sb;
        }
    }
}
