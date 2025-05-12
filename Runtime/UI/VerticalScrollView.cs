using System;
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
        protected override Vector2 ContentRectTransformAnchorMin => new Vector2(0f, 1f);

        /// <inheritdoc />
        protected override Vector2 ContentRectTransformAnchorMax => new Vector2(1f, 1f);

        /// <inheritdoc />
        protected override Vector2 ContentRectTransformPivot => new Vector2(0.5f, 1f);

        /// <inheritdoc />
        protected override Type ContentHorizontalOrVerticalLayoutGroupType => typeof(VerticalLayoutGroup);

        /// <inheritdoc />
        protected override bool ScrollRectHorizontal => false;

        /// <inheritdoc />
        protected override bool ScrollRectVertical => true;

        /// <inheritdoc />
        protected override float PaddingAlongAxis => padding.vertical;

        /// <inheritdoc />
        protected override float FirstPaddingAlongAxis => padding.top;

        /// <inheritdoc />
        protected override float LastPaddingAlongAxis => padding.bottom;

        /// <inheritdoc />
        protected override void Set(LayoutElement layoutElement, float size)
        {
            layoutElement.minHeight = size;
        }

        /// <inheritdoc />
        protected override float Get(Vector2 vector2)
        {
            return vector2.y;
        }

        /// <inheritdoc />
        protected override Vector2 Set(Vector2 vector2, float value)
        {
            vector2.y = value;
            return vector2;
        }

        /// <inheritdoc />
        protected override bool CanExpandItemWidth => true;

        /// <inheritdoc />
        protected override bool CanExpandItemHeight => false;

        /// <inheritdoc />
        protected override Scrollbar GetScrollbar(ScrollRect scrollRect)
        {
            return scrollRect.verticalScrollbar;
        }

        /// <inheritdoc />
        protected override void SetScrollbar(ScrollRect scrollRect, Scrollbar scrollbar)
        {
            scrollRect.verticalScrollbar = scrollbar;
        }
    }
}
