using UnityEngine;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 保持指定的宽高比，有且仅有一个布局元素的布局组。
    /// </summary>
    public sealed class AspectRatioLayoutGroup : OneElementLayoutGroup
    {
        [SerializeField]
        internal float aspectRatio = 1f;

        /// <summary>
        /// 获取或设置宽高比。
        /// </summary>
        /// <remarks>请确保值在 [0.001, 1000] 范围内。</remarks>
        public float AspectRatio
        {
            get => aspectRatio;
            set
            {
                if (aspectRatio == value)
                {
                    return;
                }
                aspectRatio = value;
                SetDirty();
            }
        }

        /// <inheritdoc />
        protected override void CalculateSingleElementLayoutInputHorizontal(RectTransform element)
        {
            var elementMinSize = new Vector2(
                LayoutUtility.GetMinSize(element, 0),
                LayoutUtility.GetMinSize(element, 1)
            );
            var elementPreferredSize = new Vector2(
                LayoutUtility.GetPreferredSize(element, 0),
                LayoutUtility.GetPreferredSize(element, 1)
            );
            var paddingSize = new Vector2(padding.horizontal, padding.vertical);

            var totalMinSize       = elementMinSize + paddingSize;
            var totalPreferredSize = elementPreferredSize + paddingSize;

            AdjustSize(ref totalMinSize);
            AdjustSize(ref totalPreferredSize);

            SetLayoutInputForAxis(totalMinSize[0], totalPreferredSize[0], 0, 0);
            SetLayoutInputForAxis(totalMinSize[1], totalPreferredSize[1], 0, 1);
        }

        private void AdjustSize(ref Vector2 size)
        {
            if (size[0] < size[1] * aspectRatio)
            {
                size[0] = size[1] * aspectRatio;
            }
            else
            {
                size[1] = size[0] / aspectRatio;
            }
        }

        /// <inheritdoc />
        protected override void CalculateSingleElementLayoutInputVertical(RectTransform element)
        {
        }

        /// <inheritdoc />
        protected override void SetSingleElementLayoutHorizontal(RectTransform element)
        {
            SetSingleElementLayout(element, 0);
        }

        /// <inheritdoc />
        protected override void SetSingleElementLayoutVertical(RectTransform element)
        {
            SetSingleElementLayout(element, 1);
        }

        private void SetSingleElementLayout(RectTransform singleElement, int axis)
        {
            var childSize = Mathf.Lerp(
                LayoutUtility.GetMinSize(singleElement, axis),
                LayoutUtility.GetPreferredSize(singleElement, axis),
                Mathf.InverseLerp(GetTotalMinSize(axis), GetTotalPreferredSize(axis), rectTransform.rect.size[axis])
            );
            var pos = GetStartOffset(axis, childSize);
            SetChildAlongAxis(singleElement, axis, pos, childSize);
        }

#if UNITY_EDITOR
        /// <inheritdoc />
        protected override void OnValidate()
        {
            base.OnValidate();
            aspectRatio = !float.IsNaN(aspectRatio) ? Mathf.Clamp(aspectRatio, 0.001f, 1000f) : 1f;
        }
#endif
    }
}
