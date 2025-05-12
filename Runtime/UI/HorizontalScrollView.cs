using System;
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
        protected override Vector2 ContentRectTransformAnchorMin => new Vector2(0f, 0f);

        /// <inheritdoc />
        protected override Vector2 ContentRectTransformAnchorMax => new Vector2(0f, 1f);

        /// <inheritdoc />
        protected override Vector2 ContentRectTransformPivot => new Vector2(0f, 0.5f);

        /// <inheritdoc />
        protected override Type ContentHorizontalOrVerticalLayoutGroupType => typeof(HorizontalLayoutGroup);

        /// <inheritdoc />
        protected override bool ScrollRectHorizontal => true;

        /// <inheritdoc />
        protected override bool ScrollRectVertical => false;

        /// <inheritdoc />
        protected override float PaddingAlongAxis => padding.horizontal;

        /// <inheritdoc />
        protected override float FirstPaddingAlongAxis => padding.left;

        /// <inheritdoc />
        protected override float LastPaddingAlongAxis => padding.right;

        /// <inheritdoc />
        protected override void Set(LayoutElement layoutElement, float size)
        {
            layoutElement.minWidth = size;
        }

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
        protected override bool CanExpandItemWidth => false;

        /// <inheritdoc />
        protected override bool CanExpandItemHeight => true;

        /// <inheritdoc />
        protected override Scrollbar GetScrollbar(ScrollRect scrollRect)
        {
            return scrollRect.horizontalScrollbar;
        }

        /// <inheritdoc />
        protected override void SetScrollbar(ScrollRect scrollRect, Scrollbar scrollbar)
        {
            scrollRect.horizontalScrollbar = scrollbar;
        }
    }
}
