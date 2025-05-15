using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Collections;
using Aurora.Diagnostics;
using Aurora.Interpolations;
using Aurora.Unity.CompilerServices;
using Aurora.Unity.PlayerLoop;
using Aurora.Unity.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 滚动视图。
    /// </summary>
    /// <remarks>借助 <see cref="UnityEngine.UI.ScrollRect"/> 实现功能。</remarks>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(ScrollRect))]
    public abstract class ScrollView : UIBehaviour,
                                       IPointerDownHandler,
                                       IPointerUpHandler,
                                       IBeginDragHandler,
                                       IDragHandler,
                                       IEndDragHandler,
                                       IScrollHandler,
                                       IPlayerLoopItem
    {
        private const string ControllerUnset = "The controller has not been set. Call " +
                                               nameof(SetControllerAndReload) + " before using this method.";

        private static readonly ParameterizedPredicate<ScrollViewItem, ScrollViewItem> AreIdentifierEqual =
            (a, b) => a.identifier == b.identifier;

        private static readonly ParameterizedPredicate<ScrollViewItem, int> IndexEqualTo = (scrollViewItem, index) =>
            scrollViewItem.index == index;

        private IScrollViewController _controller;

        [SerializeField]
        internal ScrollRect scrollRect;

        [SerializeField]
        internal Transform inactiveContainer;

        [SerializeField]
        internal RectTransform viewport;

        [SerializeField]
        internal RectTransform content;

        [SerializeField]
        internal HorizontalOrVerticalLayoutGroup contentLayoutGroup;

        [SerializeField]
        internal RectOffset padding = new RectOffset();

        [SerializeField]
        [Min(0)]
        internal float spacing;

        [SerializeField]
        internal bool childForceExpandSize;

        [SerializeField]
        internal LayoutElement leadingPlaceholder;

        [SerializeField]
        internal LayoutElement trailingPlaceholder;

        [SerializeField]
        internal Scrollbar scrollbar;

        [SerializeField]
        internal ScrollbarVisibility scrollbarVisibility = ScrollbarVisibility.OnlyIfNeeded;

        /// <summary>
        /// 前方活动偏移量。
        /// </summary>
        [Min(0)]
        public float leadingActiveOffset;

        /// <summary>
        /// 后方活动偏移量。
        /// </summary>
        [Min(0)]
        public float trailingActiveOffset;

        /// <summary>
        /// 速率限制。大于 0 时生效。
        /// </summary>
        [Min(0)]
        public float speedLimit;

        /// <summary>
        /// 自动吸附的触发条件。
        /// </summary>
        public ScrollViewSnapTrigger snapTrigger;

        /// <summary>
        /// 当标准化滚动位置改变时，如果速率小于这个值，将触发自动吸附。
        /// </summary>
        [Min(0)]
        public float snapSpeedThreshold = 300;

        /// <summary>
        /// 用于参与“自动吸附的目标位置”的计算，它表示寻找最靠近此标准化视口位置的项。
        /// </summary>
        [Range(0, 1)]
        public float snapFindNormalizedViewportPosition = 0.5f;

        /// <summary>
        /// 用于参与“自动吸附的目标位置”的计算，它表示在计算项的开始位置和结束位置时，是否还应该考虑项之前和之后的空白。
        /// <br/>
        /// 项之前或之后的空白，是指，如果项是第一项，那么项之前的空白是前内边距，否则是间距，如果项是最后一项，那么项之后的空白是后内边距，否则是间距。
        /// </summary>
        public bool snapIncludingSpacing;

        /// <summary>
        /// 用于参与“自动吸附的目标位置”的计算，它表示在寻找到要自动吸附的项时，对项的开始位置和结束位置使用此权重进行线性插值，以计算目标位置。
        /// </summary>
        [Range(0, 1)]
        public float snapNormalizedItemPosition = 0.5f;

        /// <summary>
        /// 用于参与“自动吸附的目标位置”的计算，它表示将目标位置逐渐“吸附”到此标准化视口位置。
        /// </summary>
        [Range(0, 1)]
        public float snapJumpNormalizedViewportPosition = 0.5f;

        /// <summary>
        /// 如何计算自动吸附的耗时。
        /// </summary>
        public ScrollViewSnapDurationMode snapDurationMode;

        /// <summary>
        /// 自动吸附的耗时。
        /// </summary>
        /// <remarks>当 <see cref="snapDurationMode"/> 为 <see cref="ScrollViewSnapDurationMode.Fixed"/> 时，将使用此值。</remarks>
        [Min(0)]
        public float snapDuration = 0.25f;

        /// <summary>
        /// 自动吸附的速度。
        /// </summary>
        /// <remarks>当 <see cref="snapDurationMode"/> 为 <see cref="ScrollViewSnapDurationMode.Dynamic"/> 时，将使用此值。</remarks>
        [Min(0)]
        public float snapSpeed = 900;

        /// <summary>
        /// 自动吸附过程所使用的插值类型。
        /// </summary>
        public Interpolation snapInterpolation = Interpolation.OutCubic;

        private CancellationTokenSource _tweenTokenSource;

        private bool _dragging;

        /// <summary>
        /// 在 <see cref="IScrollHandler.OnScroll"/>（<see cref="EventSystem.Update">EventSystem.Update</see>）中设为 <see langword="true"/>，
        /// <br/>
        /// 在 <see cref="OnScrollRectValueChanged"/>（<see cref="ScrollRect.LateUpdate">ScrollRect.LateUpdate</see>）中被使用，
        /// <br/>
        /// 在 <see cref="PlayerLoopPhase.LateUpdated"/> 中设为 <see langword="false"/>。
        /// </summary>
        [NonSerialized]
        private bool _scrolling;

        [NonSerialized]
        private ScrollRect.MovementType _scrollRectMovementTypeBeforeTween;

        [NonSerialized]
        private bool _scrollRectInertiaBeforeTween;

        private int _itemCount;

        /// <summary>
        /// 全体项的开始处和结尾处的内容位置。内部排列如下：
        /// <list type="bullet">
        /// <item><description>第一项的开始位置</description></item>
        /// <item><description>第一项的结束位置</description></item>
        /// <item><description>第二项的开始位置</description></item>
        /// <item><description>第二项的结束位置</description></item>
        /// <item><description>……（略）</description></item>
        /// <item><description>最后一项的开始位置</description></item>
        /// <item><description>最后一项的结束位置</description></item>
        /// </list>
        /// </summary>
        /// <remarks>长度为 <see cref="_itemCount"/> 的 2 倍。</remarks>
        private readonly List<float> _itemPositions = new List<float>();

        /// <summary>
        /// 活动的 <see cref="ScrollViewItem"/>。
        /// </summary>
        private readonly Deque<ScrollViewItem> _activeItems = new Deque<ScrollViewItem>();

        /// <summary>
        /// 已回收的 <see cref="ScrollViewItem"/>。
        /// </summary>
        private readonly List<ScrollViewItem> _recycledItems = new List<ScrollViewItem>();

        /// <remarks>当内容大小较大、<see cref="snapSpeedThreshold"/> 较小时，<see cref="ScrollRect.onValueChanged">ScrollRect.onValueChanged</see>不再每帧触发，不可靠，因此趁它还在触发的时候开启计数器，并且在速度降低到阈值之前持续刷新计数器，这样才可以实现当速度低于阈值时吸附（当 <see cref="snapTrigger"/> 定义了 <see cref="ScrollViewSnapTrigger.OnNormalizedScrollPositionChanged"/> 位时）。</remarks>
        private ICounter _valueChangeCounter;

        private ITimer _scrollTimer;

        private const double ScrollDelay = 0.3;

        /// <summary>
        /// 获取控制器。
        /// </summary>
        public IScrollViewController Controller
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _controller;
        }

        /// <summary>
        /// 获取滚动区域。
        /// </summary>
        public ScrollRect ScrollRect
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => scrollRect;
        }

        /// <summary>
        /// 获取视口大小。
        /// </summary>
        public float ViewportSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetViewportSize();
        }

        /// <summary>
        /// 获取内容大小。
        /// </summary>
        public float ContentSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetContentSize();
        }

        /// <summary>
        /// 获取内容超出视口的大小。
        /// </summary>
        /// <remarks>如果 <see cref="ContentSize"/> 小于或等于 <see cref="ViewportSize"/>，则为 0。</remarks>
        public float OverflowedContentSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetOverflowedContentSize();
        }

        /// <summary>
        /// 获取或设置内容位置。
        /// </summary>
        public float ContentPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetContentPosition();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetContentPosition(value);
        }

        /// <summary>
        /// 获取或设置标准化滚动位置。
        /// </summary>
        /// <remarks>
        /// 标准化滚动位置通常在 [0, 1] 范围内，使用 <see cref="double"/> 类型以提供更大的精度。
        /// <br/>
        /// 内部计算时，不会使用 <see cref="ScrollRect.normalizedPosition"/>，因为 <see cref="ScrollRect.normalizedPosition"/> 的精度不够。
        /// </remarks>
        public double NormalizedScrollPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetNormalizedScrollPosition();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetNormalizedScrollPosition(value);
        }

        /// <summary>
        /// 获取或设置内容的内边距。
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> 有至少一个分量为负数。</exception>
        public RectOffset Padding
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => padding;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                if (value.left < 0 || value.right < 0 || value.top < 0 || value.bottom < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
                // 由于 RectOffset 是引用类型，可以直接修改其内部成员，因此跳过相等性检查
                padding = value;
#if UNITY_EDITOR
                Undo.RecordObject(contentLayoutGroup, $"set {nameof(LayoutGroup.padding)}");
#endif
                contentLayoutGroup.padding.left   = padding.left;
                contentLayoutGroup.padding.right  = padding.right;
                contentLayoutGroup.padding.top    = padding.top;
                contentLayoutGroup.padding.bottom = padding.bottom;
                InternalReloadWithNormalizedScrollPosition(NormalizedScrollPosition);
            }
        }

        /// <summary>
        /// 返回滚动方向的内边距。
        /// </summary>
        protected abstract float PaddingAlongAxis { get; }

        /// <summary>
        /// 返回滚动方向的前半部分内边距。
        /// </summary>
        protected abstract float FirstPaddingAlongAxis { get; }

        /// <summary>
        /// 返回滚动方向的后半部分内边距。
        /// </summary>
        protected abstract float LastPaddingAlongAxis { get; }

        /// <summary>
        /// 获取或设置各项的间距。
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> 不满足“大于或等于 0”。</exception>
        public float Spacing
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => spacing;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (!(value >= 0))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
                if (spacing.Equals(value))
                {
                    return;
                }
                spacing = value;
#if UNITY_EDITOR
                Undo.RecordObject(contentLayoutGroup, $"set {nameof(HorizontalOrVerticalLayoutGroup.spacing)}");
#endif
                contentLayoutGroup.spacing = spacing;
                InternalReloadWithNormalizedScrollPosition(NormalizedScrollPosition);
            }
        }

        /// <summary>
        /// 获取或设置一个值，这个值指示是否要在非滚动方向上让项占满内容。
        /// </summary>
        /// <remarks>如果是水平滚动视图，则让项的高度占满内容的高度；如果是垂直滚动视图，则让项的宽度占满内容的宽度。</remarks>
        public bool ChildForceExpandSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => childForceExpandSize;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (childForceExpandSize == value)
                {
                    return;
                }
                childForceExpandSize = value;
                SetLayoutGroupChildForceExpandSize(contentLayoutGroup, childForceExpandSize);
            }
        }

        /// <summary>
        /// 获取第一个活动项的索引。如果没有活动项，则返回 -1。
        /// </summary>
        public int FirstActiveIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _activeItems.TryPeekFirst(out var item) ? item.index : -1;
        }

        /// <summary>
        /// 获取第一个可见项的索引。如果没有可见项，则返回 -1.
        /// </summary>
        public int FirstVisibleIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => InternalFindFirstIndex(ConvertNormalizedViewportPositionToContentPosition(0));
        }

        /// <summary>
        /// 获取最后一个可见项的索引。如果没有可见项，则返回 -1.
        /// </summary>
        public int LastVisibleIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => InternalFindLastIndex(ConvertNormalizedViewportPositionToContentPosition(1));
        }

        /// <summary>
        /// 获取最后一个活动项的索引。如果没有活动项，则返回 -1。
        /// </summary>
        public int LastActiveIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _activeItems.TryPeekLast(out var item) ? item.index : -1;
        }

        /// <summary>
        /// 获取项的数量。
        /// </summary>
        public int ItemCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _itemCount;
        }

        /// <summary>
        /// 获取位于指定索引处的项。
        /// </summary>
        /// <param name="index">索引。</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> 小于 0，或者大于或等于 <see cref="ItemCount"/>。</exception>
        public ScrollViewItem this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index < 0 || index >= _itemCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
                }
                return _activeItems.Find(IndexEqualTo, index);
            }
        }

        /// <summary>
        /// 获取或设置滚动条的可见性。
        /// </summary>
        public ScrollbarVisibility ScrollbarVisibility
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => scrollbarVisibility;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (scrollbarVisibility == value)
                {
                    return;
                }
                scrollbarVisibility = value;
                RefreshScrollbarVisibility();
            }
        }

        /// <summary>
        /// 是否正在被拖拽。
        /// </summary>
        public bool Dragging
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dragging;
        }

        /// <summary>
        /// 是否正在播放补间动画。
        /// </summary>
        public bool Tweening
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsTweening();
        }

        /// <summary>
        /// 返回 <paramref name="vector2"/> 的滚动方向分量。
        /// </summary>
        protected abstract float Get(Vector2 vector2);

        /// <summary>
        /// 将 <paramref name="vector2"/> 的滚动方向分量设置为 <paramref name="value"/>，然后返回设置后的值。
        /// </summary>
        protected abstract Vector2 Set(Vector2 vector2, float value);

        /// <summary>
        /// 设置 <paramref name="layoutElement"/> 在滚动方向上的最小大小。
        /// </summary>
        protected abstract void SetMinSize(LayoutElement layoutElement, float minSize);

        /// <summary>
        /// 设置是否要在非滚动方向上让项占满内容。
        /// </summary>
        protected abstract void SetLayoutGroupChildForceExpandSize(
            HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup,
            bool                            forceExpand);

        /// <summary>
        /// 设置滚动条。
        /// </summary>
        /// <remarks>执行此操作通常是为了防止 <see cref="UnityEngine.UI.ScrollRect"/> 修改滚动条的可见性。</remarks>
        protected abstract void SetScrollRectScrollbar(ScrollRect rect, Scrollbar bar);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetViewportSize()
        {
            return Get(viewport.rect.size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetContentSize()
        {
            return Get(content.rect.size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetOverflowedContentSize()
        {
            return Mathf.Max(GetContentSize() - GetViewportSize(), 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetContentPosition()
        {
            return Get(content.anchoredPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetContentPosition(float contentPosition)
        {
            content.anchoredPosition = Set(content.anchoredPosition, contentPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double GetNormalizedScrollPosition()
        {
            return InternalConvertContentPositionToNormalizedScrollPosition(GetContentPosition());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetNormalizedScrollPosition(double normalizedScrollPosition)
        {
            SetContentPosition(InternalConvertNormalizedScrollPositionToContentPosition(normalizedScrollPosition));
        }

        /// <summary>
        /// 获取位于指定索引处的项的开始（内容）位置。
        /// </summary>
        /// <param name="index">索引。</param>
        /// <returns>项的开始（内容）位置。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> 小于 0，或者大于或等于 <see cref="ItemCount"/>。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetItemBeginPosition(int index)
        {
            if (index < 0 || index >= _itemCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }
            return InternalGetItemBeginPosition(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float InternalGetItemBeginPosition(int index)
        {
            return _itemPositions[index * 2];
        }

        /// <summary>
        /// 获取位于指定索引处的项的结束（内容）位置。
        /// </summary>
        /// <param name="index">索引。</param>
        /// <returns>项的结束（内容）位置。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> 小于 0，或者大于或等于 <see cref="ItemCount"/>。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetItemEndPosition(int index)
        {
            if (index < 0 || index >= _itemCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }
            return InternalGetItemEndPosition(index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float InternalGetItemEndPosition(int index)
        {
            return _itemPositions[index * 2 + 1];
        }

        /// <summary>
        /// 设置控制器，保持当前内容位置，重新加载。
        /// </summary>
        /// <param name="controller">控制器。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetControllerAndReload(IScrollViewController controller)
        {
            _controller = controller;
            InternalReload();
        }

        /// <summary>
        /// 设置控制器，设置内容位置，重新加载。
        /// </summary>
        /// <param name="controller">控制器。</param>
        /// <param name="contentPosition">内容位置。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetControllerAndReloadWithContentPosition(IScrollViewController controller, float contentPosition)
        {
            _controller = controller;
            InternalReloadWithContentPosition(contentPosition);
        }

        /// <summary>
        /// 设置控制器，设置标准化滚动位置，重新加载。
        /// </summary>
        /// <param name="controller">控制器。</param>
        /// <param name="normalizedScrollPosition">标准化滚动位置。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetControllerAndReloadWithNormalizedScrollPosition(
            IScrollViewController controller,
            float                 normalizedScrollPosition)
        {
            _controller = controller;
            InternalReloadWithNormalizedScrollPosition(normalizedScrollPosition);
        }

        /// <summary>
        /// 保持当前内容位置，重新加载。
        /// </summary>
        /// <remarks>调用方应确保在模型数据改变后立即调用一次。</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reload()
        {
            InternalReload();
        }

        /// <summary>
        /// 设置内容位置，重新加载。
        /// </summary>
        /// <param name="contentPosition">内容位置。</param>
        /// <remarks>调用方应确保在模型数据改变后立即调用一次。</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReloadWithContentPosition(float contentPosition)
        {
            InternalReloadWithContentPosition(contentPosition);
        }

        /// <summary>
        /// 设置标准化滚动位置，重新加载。
        /// </summary>
        /// <param name="normalizedScrollPosition">标准化滚动位置。</param>
        /// <remarks>调用方应确保在模型数据改变后立即调用一次。</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReloadWithNormalizedScrollPosition(float normalizedScrollPosition)
        {
            InternalReloadWithNormalizedScrollPosition(normalizedScrollPosition);
        }

        private void InternalReload()
        {
            InternalStopTween();
            ReturnAllItems();
            ReloadItemCountAndBeginEndPositions();
            RefreshContentSize();
            SetContentPosition(0);
            AddItems();
            RefreshScrollbarVisibility();
        }

        private void InternalReloadWithContentPosition(float contentPosition)
        {
            InternalStopTween();
            ReturnAllItems();
            ReloadItemCountAndBeginEndPositions();
            RefreshContentSize();
            SetContentPosition(contentPosition);
            AddItems();
            RefreshScrollbarVisibility();
        }

        private void InternalReloadWithNormalizedScrollPosition(double normalizedScrollPosition)
        {
            InternalStopTween();
            ReturnAllItems();
            ReloadItemCountAndBeginEndPositions();
            RefreshContentSize();
            SetNormalizedScrollPosition(normalizedScrollPosition);
            AddItems();
            RefreshScrollbarVisibility();
        }

        /// <summary>
        /// 回收超出范围的项、添加新进入范围的项。
        /// </summary>
        /// <remarks>此方法将在 <see cref="ScrollRect.onValueChanged"/> 事件引发时调用。</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Refresh()
        {
            InternalRefresh();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InternalRefresh()
        {
            var beginActiveContentPosition = InternalConvertNormalizedViewportPositionToContentPosition(0) -
                                             (leadingActiveOffset >= 0 ? leadingActiveOffset : 0);
            var endActiveContentPosition = InternalConvertNormalizedViewportPositionToContentPosition(1) +
                                           (trailingActiveOffset >= 0 ? trailingActiveOffset : 0);
            var firstActiveIndex = InternalFindFirstIndex(beginActiveContentPosition);
            var lastActiveIndex  = InternalFindLastIndex(endActiveContentPosition);
            if (firstActiveIndex >= 0 && lastActiveIndex >= 0 && firstActiveIndex <= lastActiveIndex)
            {
                if (ReturnItemsBefore(firstActiveIndex) | ReturnItemsAfter(lastActiveIndex) |
                    AddItems(firstActiveIndex, lastActiveIndex))
                {
                    RefreshPlaceholders();
                }
            }
            else
            {
                if (ReturnAllItems())
                {
                    RefreshPlaceholders();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ReturnAllItems()
        {
            if (_activeItems.TryDequeueLast(out var item))
            {
                do
                {
                    ReturnItem(item, false);
                } while (_activeItems.TryDequeueLast(out item));
                return true;
            }
            return false;
        }

        private void ReloadItemCountAndBeginEndPositions()
        {
            if (_controller == null)
            {
                _itemCount = 0;
                _itemPositions.Clear();
            }
            else
            {
                var itemCount = _controller.GetItemCount(this);
                if (itemCount < 0)
                {
                    throw new InvalidOperationException(
                        $"{nameof(IScrollViewController)}.{nameof(IScrollViewController.GetItemCount)}(scrollView) 的返回值为负数"
                    );
                }
                _itemCount = itemCount;
                _itemPositions.Clear();
                /*
                 * 虽然保存的值的类型是 float，但在累加时使用 double 以减少精度损失
                 * 别小看了这里的作用，如果每次计算新的位置的时候都使用列表中最后一项作为基础，当项的数量很多、在编辑器中拖动修改间距的时候，将产生明显的抖动现象
                 */
                double position = 0;
                for (var itemIndex = 0; itemIndex < _itemCount; itemIndex++)
                {
                    _itemPositions.Add((float) (position += itemIndex == 0 ? FirstPaddingAlongAxis : spacing));

                    float itemSize;
                    {
                        var itemSizeFromController = _controller.GetItemSize(this, itemIndex);
                        if (itemSizeFromController >= 0)
                        {
                            itemSize = itemSizeFromController;
                        }
                        else
                        {
                            itemSize = 0;
                            Debug.LogWarning(
                                $"{nameof(IScrollViewController)}.{nameof(IScrollViewController.GetItemSize)}(scrollView, {itemIndex}) 的返回值不满足 >= 0"
                            );
                        }
                    }
                    _itemPositions.Add((float) (position += itemSize));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RefreshContentSize()
        {
            var contentSize = _itemCount == 0
                                  ? PaddingAlongAxis
                                  : InternalGetItemEndPosition(_itemCount - 1) + LastPaddingAlongAxis;
            content.sizeDelta = Set(content.sizeDelta, contentSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddItems()
        {
            var beginActiveContentPosition = InternalConvertNormalizedViewportPositionToContentPosition(0) -
                                             (leadingActiveOffset >= 0 ? leadingActiveOffset : 0);
            var endActiveContentPosition = InternalConvertNormalizedViewportPositionToContentPosition(1) +
                                           (trailingActiveOffset >= 0 ? trailingActiveOffset : 0);
            var firstActiveIndex = InternalFindFirstIndex(beginActiveContentPosition);
            var lastActiveIndex  = InternalFindLastIndex(endActiveContentPosition);
            if (firstActiveIndex >= 0 && lastActiveIndex >= 0 && firstActiveIndex <= lastActiveIndex)
            {
                AddItems(firstActiveIndex, lastActiveIndex);
                RefreshPlaceholders();
            }
        }

        private bool AddItems(int firstActiveIndex, int lastActiveIndex)
        {
            if (_activeItems.Count == 0)
            {
                for (var index = firstActiveIndex; index <= lastActiveIndex; index++)
                {
                    AddLastItem(index);
                }
                return true;
            }
            var oldFirstActiveIndex = _activeItems.PeekFirst().index;
            for (var index = oldFirstActiveIndex - 1; index >= firstActiveIndex; index--)
            {
                AddFirstItem(index);
            }
            var oldLastActiveIndex = _activeItems.PeekLast().index;
            for (var index = oldLastActiveIndex + 1; index <= lastActiveIndex; index++)
            {
                AddLastItem(index);
            }
            return firstActiveIndex < oldFirstActiveIndex || lastActiveIndex > oldLastActiveIndex;
        }

        private void RefreshPlaceholders()
        {
            bool  leadingPlaceholderActive;
            float leadingPlaceholderSize;
            bool  trailingPlaceholderActive;
            float trailingPlaceholderSize;

            if (_activeItems.Count == 0)
            {
                leadingPlaceholderActive  = false;
                leadingPlaceholderSize    = 0;
                trailingPlaceholderActive = false;
                trailingPlaceholderSize   = 0;
            }
            else
            {
                if (_activeItems.PeekFirst().index is var firstActiveIndex && firstActiveIndex == 0)
                {
                    leadingPlaceholderActive = false;
                    leadingPlaceholderSize   = 0;
                }
                else
                {
                    leadingPlaceholderActive = true;
                    leadingPlaceholderSize = InternalGetItemBeginPosition(firstActiveIndex) -
                                             InternalGetItemBeginPosition(0) - spacing;
                }

                if (_activeItems.PeekLast().index is var lastActiveIndex && lastActiveIndex == _itemCount - 1)
                {
                    trailingPlaceholderActive = false;
                    trailingPlaceholderSize   = 0;
                }
                else
                {
                    trailingPlaceholderActive = true;
                    trailingPlaceholderSize = InternalGetItemEndPosition(_itemCount - 1) -
                                              InternalGetItemEndPosition(lastActiveIndex) - spacing;
                }
            }

            leadingPlaceholder.gameObject.SetActive(leadingPlaceholderActive);
            SetMinSize(leadingPlaceholder, leadingPlaceholderSize);
            trailingPlaceholder.gameObject.SetActive(trailingPlaceholderActive);
            SetMinSize(trailingPlaceholder, trailingPlaceholderSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RefreshScrollbarVisibility()
        {
            if (!scrollbar)
            {
                return;
            }
            SetScrollRectScrollbar(scrollRect, scrollbar);
            var scrollbarActive = scrollbarVisibility switch
            {
                ScrollbarVisibility.Never        => false,
                ScrollbarVisibility.OnlyIfNeeded => GetOverflowedContentSize() > 0,
                ScrollbarVisibility.Always       => true,
                _                                => throw new ArgumentOutOfRangeException()
            };
            scrollbar.gameObject.SetActive(scrollbarActive);
            if (!scrollbarActive)
            {
                SetScrollRectScrollbar(scrollRect, null); // 防止 ScrollRect 设置 ScrollBar 为活动
            }
        }

        /// <summary>
        /// 终止补间动画。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopTween()
        {
            InternalStopTween();
            if (!IsActive())
            {
                return;
            }
            DisableValueChangeCounter();
            DisableScrollTimer();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InternalStopTween()
        {
            if (!IsTweening())
            {
                return;
            }
            if (this)
            {
                scrollRect.movementType = _scrollRectMovementTypeBeforeTween;
                scrollRect.inertia      = _scrollRectInertiaBeforeTween;
            }
            if (UnityEnvironment.IsPlaying)
            {
                _tweenTokenSource.Cancel();
            }
            _tweenTokenSource.Dispose();
            _tweenTokenSource = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsTweening()
        {
            return _tweenTokenSource != null;
        }

        /// <summary>
        /// 吸附。
        /// </summary>
        /// <seealso cref="snapFindNormalizedViewportPosition"/>
        /// <seealso cref="snapIncludingSpacing"/>
        /// <seealso cref="snapNormalizedItemPosition"/>
        /// <seealso cref="snapJumpNormalizedViewportPosition"/>
        /// <seealso cref="snapDurationMode"/>
        /// <seealso cref="snapSpeed"/>
        /// <seealso cref="snapDuration"/>
        /// <seealso cref="snapInterpolation"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Snap()
        {
            InternalStopTween();
            if (!IsActive())
            {
                return;
            }
            DisableValueChangeCounter();
            DisableScrollTimer();
            InternalBeginSnap();
        }

        private async void InternalBeginSnap()
        {
            Assert.IsFalse(IsTweening());

            var index = InternalFindClosestIndex(
                ConvertNormalizedViewportPositionToContentPosition(snapFindNormalizedViewportPosition)
            );
            if (index < 0)
            {
                Log.D(index);
                return;
            }
            var overflowedContentSize = GetOverflowedContentSize();
            // 如果“内容大小”小于或等于“滚动视图自身大小”，且内容超出滚动视图时会受到严格限制或弹性限制，执行吸附没有意义
            if (overflowedContentSize == 0 && scrollRect.movementType != ScrollRect.MovementType.Unrestricted)
            {
                Log.D($"{index} {overflowedContentSize} {scrollRect.movementType}");
                return;
            }
            var contentPositionBegin = GetContentPosition();
            var itemBeginPosition    = InternalGetItemBeginPosition(index);
            var itemEndPosition      = InternalGetItemEndPosition(index);
            if (snapIncludingSpacing)
            {
                itemBeginPosition -= index == 0 ? FirstPaddingAlongAxis : spacing;
                itemEndPosition   += index == _itemCount - 1 ? LastPaddingAlongAxis : spacing;
            }
            // 限制内容位置的值，确保内容不会超出滚动视图
            var contentPositionEnd = Mathf.Clamp(
                (float) InterpolationUtility.LinearInterpolate(
                    itemBeginPosition,
                    itemEndPosition,
                    snapNormalizedItemPosition
                ) - snapJumpNormalizedViewportPosition * GetViewportSize(),
                0,
                overflowedContentSize
            );
            var duration = snapDurationMode switch
            {
                ScrollViewSnapDurationMode.Fixed => snapDuration,
                ScrollViewSnapDurationMode.Dynamic =>
                    Mathf.Abs((contentPositionEnd - contentPositionBegin) / snapSpeed),
                _ => throw new ArgumentOutOfRangeException()
            };
            if (duration > 0 && duration != float.PositiveInfinity)
            {
                var       exitToken        = UnityEnvironment.ExitToken;
                var       disableToken     = gameObject.GetDisableToken();
                using var tweenTokenSource = CancellationTokenSource.CreateLinkedTokenSource(exitToken, disableToken);
                var       tweenToken       = tweenTokenSource.Token;
                _tweenTokenSource = tweenTokenSource;
                try
                {
                    if (scrollRect.movementType == ScrollRect.MovementType.Elastic &&
                        GetNormalizedScrollPosition() is var normalizedScrollPosition &&
                        (normalizedScrollPosition < 0 || normalizedScrollPosition > 1))
                    {
                        scrollRect.velocity = Set(scrollRect.velocity, 0);
                    }

                    _scrollRectMovementTypeBeforeTween = scrollRect.movementType;
                    if (scrollRect.movementType == ScrollRect.MovementType.Elastic)
                    {
                        scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
                    }

                    _scrollRectInertiaBeforeTween = scrollRect.inertia;
                    scrollRect.inertia            = false;

                    await InternalSnapAsync(
                        contentPositionBegin,
                        contentPositionEnd,
                        duration,
                        snapInterpolation,
                        tweenToken
                    );

                    scrollRect.movementType = _scrollRectMovementTypeBeforeTween;
                    scrollRect.inertia      = _scrollRectInertiaBeforeTween;

                    _tweenTokenSource = null;
                }
                catch (OperationCanceledException)
                {
                }
            }
            else
            {
                SetContentPosition(contentPositionEnd);
                scrollRect.velocity = Set(scrollRect.velocity, 0);
            }
        }

        private async Task InternalSnapAsync(
            float             contentPositionBegin,
            float             contentPositionEnd,
            float             duration,
            Interpolation     interpolation,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var timeBegin = Time.timeAsDouble;
            var timeEnd   = Time.timeAsDouble + duration;
            do
            {
                await new DelayFrameAwaitable(1, PlayerLoopPhase.LateUpdating, cancellationToken);
                var deltaTime = Time.deltaTime;
                if (deltaTime > 0)
                {
                    var timeCurrent = Time.timeAsDouble;
                    var weight = Clamp01(
                        InterpolationUtility.InverseLinearInterpolate(timeBegin, timeEnd, timeCurrent)
                    );
                    SetContentPosition(
                        (float) InterpolationUtility.Interpolate(
                            contentPositionBegin,
                            contentPositionEnd,
                            weight,
                            interpolation
                        )
                    );
                }
            } while (Time.timeAsDouble < timeEnd);

            SetContentPosition(contentPositionEnd);
            scrollRect.velocity = Set(scrollRect.velocity, 0);
            // 占据任务到 ScrollRect.LateUpdate 之后，防止在 OnScrollRectValueChange 中再次触发自动吸附
            await new PlayerLoopPhaseAwaitable(PlayerLoopPhase.LateUpdated, cancellationToken);

            static double Clamp01(double value)
            {
                return value < 0
                           ? 0
                           : value > 1
                               ? 1
                               : value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnableValueChangeCounter()
        {
            _valueChangeCounter.Change(1, -1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DisableValueChangeCounter()
        {
            _valueChangeCounter.Change(-1, -1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnableScrollTimer()
        {
            _scrollTimer.Change(TimeSpan.FromSeconds(ScrollDelay), Timeout.InfiniteTimeSpan);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DisableScrollTimer()
        {
            _scrollTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// 寻找第一个开始（内容）位置大于或等于指定内容位置的项的索引。
        /// </summary>
        /// <param name="contentPosition">内容位置。</param>
        /// <returns>如果找到了第一个开始（内容）位置大于或等于指定内容位置的项，则为该项的索引；否则为 -1.</returns>
        public int FindFirstIndex(float contentPosition)
        {
            return InternalFindFirstIndex(contentPosition);
        }

        private int InternalFindFirstIndex(float contentPosition)
        {
            var positionCount = _itemCount * 2;
            Assert.AreEqual(positionCount, _itemPositions.Count);
            if (positionCount == 0)
            {
                return -1;
            }
            var comparer           = Comparer<float>.Default;
            var matchPositionIndex = _itemPositions.BinarySearch(0, positionCount, contentPosition, comparer);
            // 精确匹配，但由于可能会存在相同的位置，需要继续寻找第一个
            if (matchPositionIndex >= 0)
            {
                // 向前查找相等的位置
                while (matchPositionIndex > 0 && comparer.Compare(
                           _itemPositions[matchPositionIndex - 1],
                           contentPosition
                       ) == 0)
                {
                    matchPositionIndex--;
                }
                return matchPositionIndex / 2;
            }
            var insertPositionIndex = ~matchPositionIndex;
            return insertPositionIndex != positionCount ? insertPositionIndex / 2 : -1;
        }

        /// <summary>
        /// 寻找第一个结束（内容）位置大于或等于指定内容位置的项的索引。
        /// </summary>
        /// <param name="contentPosition">内容位置。</param>
        /// <returns>如果找到了第一个结束（内容）位置大于或等于指定内容位置的项，则为该项的索引；否则为 -1.</returns>
        public int FindLastIndex(float contentPosition)
        {
            return InternalFindLastIndex(contentPosition);
        }

        private int InternalFindLastIndex(float contentPosition)
        {
            var positionCount = _itemCount * 2;
            Assert.AreEqual(positionCount, _itemPositions.Count);
            if (positionCount == 0)
            {
                return -1;
            }
            var comparer           = Comparer<float>.Default;
            var matchPositionIndex = _itemPositions.BinarySearch(0, positionCount, contentPosition, comparer);
            // 精确匹配，但由于可能会存在相同的位置，需要继续寻找最后一个
            if (matchPositionIndex >= 0)
            {
                // 向后查找相等的值
                while (matchPositionIndex < positionCount - 1 && comparer.Compare(
                           contentPosition,
                           _itemPositions[matchPositionIndex + 1]
                       ) == 0)
                {
                    matchPositionIndex++;
                }
                return matchPositionIndex / 2;
            }
            var insertPositionIndex = ~matchPositionIndex;
            return insertPositionIndex != 0 ? (insertPositionIndex - 1) / 2 : -1;
        }

        /// <summary>
        /// 寻找最靠近指定内容位置的项的索引。
        /// </summary>
        /// <param name="contentPosition">内容位置。</param>
        /// <returns>如果项的数量大于 0，则为最靠近 <paramref name="contentPosition"/> 的项的索引；否则为 -1.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int FindClosestIndex(float contentPosition)
        {
            return InternalFindClosestIndex(contentPosition);
        }

        private int InternalFindClosestIndex(float contentPosition)
        {
            var positionCount = _itemCount * 2;
            Assert.AreEqual(positionCount, _itemPositions.Count);
            if (positionCount == 0)
            {
                return -1;
            }
            var comparer           = Comparer<float>.Default;
            var matchPositionIndex = _itemPositions.BinarySearch(0, positionCount, contentPosition, comparer);
            // 精确匹配，虽然可能会存在相同的位置，但这里既不偏向于寻找第一个，也不偏向于寻找最后一个，所以就使用当前二分查找的结果吧
            if (matchPositionIndex >= 0)
            {
                return matchPositionIndex / 2;
            }
            var insertPositionIndex = ~matchPositionIndex;
            // 插入位置位于开头，所以第一项是最近的项
            if (insertPositionIndex == 0)
            {
                return 0;
            }
            // 插入位置位于末尾，所以最后一项是最近的项
            if (insertPositionIndex == positionCount)
            {
                return positionCount / 2 - 1;
            }
            /*
             * _itemPositions 中，“开始位置”“结束位置”依次排列。
             * 所有“开始位置”的索引都是偶数，所有“结束位置”的索引都是奇数。
             * 如果插入位置是偶数，说明位于前一个项的结束位置与后一个项的开始位置之间，通过比较到它们的距离即可得出结果；
             * 如果插入位置是奇数，说明位于某一个项的开始位置和结束位置之间，自然就是该项。
             */
            return (insertPositionIndex & 1) == 0
                       ? comparer.Compare(
                             contentPosition - _itemPositions[insertPositionIndex - 1],
                             _itemPositions[insertPositionIndex] - contentPosition
                         ) <= 0
                             ? insertPositionIndex / 2 - 1
                             : insertPositionIndex / 2
                       : insertPositionIndex / 2;
        }

        /// <summary>
        /// 获取一个已回收的项，或实例化 <paramref name="prefab"/> 以创建一个新项。
        /// </summary>
        /// <param name="prefab">一个预制体。如果在已回收的项中没有寻找到与 <paramref name="prefab"/> 的 <see cref="ScrollViewItem.identifier"/> 相等的项，将会实例化该预制体以创建一个新项。</param>
        /// <param name="isNewCreated">项是否是新创建的。</param>
        /// <returns>从已回收的项中获取的或者新创建的项。</returns>
        /// <remarks>此方法仅由 <see cref="IScrollViewController.GetItem"/> 的实现调用。</remarks>
        public ScrollViewItem GetRecycledOrCreateNewItem(ScrollViewItem prefab, out bool isNewCreated)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            if (_recycledItems.FindLastIndex(AreIdentifierEqual, prefab) is var index && index >= 0)
            {
                var item = _recycledItems[index];
                _recycledItems.RemoveAt(index);

                isNewCreated = false;
                return item;
            }
            {
#if UNITY_EDITOR
                var prefabName = prefab.name;
#endif
                var item = Instantiate(prefab, inactiveContainer, false); // 确保不活动
#if UNITY_EDITOR
                item.name = prefabName;
#endif
                item.gameObject.SetActive(false);
                item.transform.SetParent(content, false);
                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;
                item.transform.localScale    = Vector3.one;

                item.scrollView = this;

                isNewCreated = true;
                return item;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ReturnItemsBefore(int index)
        {
            if (_activeItems.TryPeekFirst(out var item) && item.index < index)
            {
                do
                {
                    _activeItems.DequeueFirst();
                    ReturnItem(item, false);
                } while (_activeItems.TryPeekFirst(out item) && item.index < index);
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ReturnItemsAfter(int index)
        {
            if (_activeItems.TryPeekLast(out var item) && item.index > index)
            {
                do
                {
                    _activeItems.DequeueLast();
                    ReturnItem(item, false);
                } while (_activeItems.TryPeekLast(out item) && item.index > index);
                return true;
            }
            return false;
        }

        private ScrollViewItem GetItem(int index, out bool isNewCreated)
        {
            if (_controller == null)
            {
                throw new InvalidOperationException(ControllerUnset);
            }

            var item = _controller.GetItem(this, index, out isNewCreated);
#if UNITY_EDITOR
            if (GetItemNameSafe(item) is { } itemName)
            {
                item.name = $"{itemName} (Index = {index})";
            }
#endif
            item.index = index;
            SetMinSize(
                item.TryGetComponent(out LayoutElement layoutElement)
                    ? layoutElement
                    : item.gameObject.AddComponent<LayoutElement>(),
                _itemPositions[index * 2 + 1] - _itemPositions[index * 2]
            );
            return item;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddFirstItem(int index)
        {
            var item = GetItem(index, out var isNewCreated);
            item.transform.SetSiblingIndex(1); // after _leadingPlaceholder
            _activeItems.EnqueueFirst(item);
            item.gameObject.SetActive(true);
            item.OnGet(this, isNewCreated);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddLastItem(int index)
        {
            var item = GetItem(index, out var isNewCreated);
            item.transform.SetSiblingIndex(item.transform.parent.childCount - 2); // before _trailingPlaceholder
            _activeItems.EnqueueLast(item);
            item.gameObject.SetActive(true);
            item.OnGet(this, isNewCreated);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReturnItem(ScrollViewItem item, bool calledFromOnDestroy)
        {
            if (item.visible)
            {
                item.visible = false;
                item.OnInvisible(this);
            }
            item.OnReturn(this);
            item.index = -1;
            if (calledFromOnDestroy)
            {
                return;
            }
            item.gameObject.SetActive(false);
            _recycledItems.Add(item);
#if UNITY_EDITOR
            if (GetItemNameSafe(item) is { } itemName)
            {
                item.name = $"{itemName} (Recycled)";
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string GetItemNameSafe(ScrollViewItem item)
        {
            if (!item)
            {
                return null;
            }
            try
            {
                return _controller.GetItemName(item);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 将内容位置转换为标准化视口位置。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ConvertContentPositionToNormalizedViewportPosition(float contentPosition)
        {
            return InternalConvertContentPositionToNormalizedViewportPosition(contentPosition);
        }

        /// <summary>
        /// 将标准化视口位置转换为内容位置。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ConvertNormalizedViewportPositionToContentPosition(float normalizedViewportPosition)
        {
            return InternalConvertNormalizedViewportPositionToContentPosition(normalizedViewportPosition);
        }

        /// <summary>
        /// 将内容位置转换为标准化滚动位置。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ConvertContentPositionToNormalizedScrollPosition(float contentPosition)
        {
            return InternalConvertContentPositionToNormalizedScrollPosition(contentPosition);
        }

        /// <summary>
        /// 将标准化滚动位置转换为内容位置。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ConvertNormalizedScrollPositionToContentPosition(double normalizedScrollPosition)
        {
            return InternalConvertNormalizedScrollPositionToContentPosition(normalizedScrollPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float InternalConvertContentPositionToNormalizedViewportPosition(float contentPosition)
        {
            var begin = GetContentPosition();
            var end   = begin + GetViewportSize();
            return (float) InterpolationUtility.InverseLinearInterpolate(begin, end, contentPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float InternalConvertNormalizedViewportPositionToContentPosition(float normalizedViewportPosition)
        {
            var begin = GetContentPosition();
            var end   = begin + GetViewportSize();
            return (float) InterpolationUtility.LinearInterpolate(begin, end, normalizedViewportPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double InternalConvertContentPositionToNormalizedScrollPosition(float contentPosition)
        {
            var overflowedContentSize = GetOverflowedContentSize();
            return overflowedContentSize == 0 ? 0 : contentPosition / overflowedContentSize;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float InternalConvertNormalizedScrollPositionToContentPosition(double normalizedScrollPosition)
        {
            var overflowedContentSize = GetOverflowedContentSize();
            return (float) (overflowedContentSize == 0 ? 0 : overflowedContentSize * normalizedScrollPosition);
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            InternalStopTween();
            DisableValueChangeCounter();
            DisableScrollTimer();
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (!_dragging && (snapTrigger & ScrollViewSnapTrigger.OnPointerUpWithLowSpeed) != 0 && !IsTweening() &&
                Mathf.Abs(Get(scrollRect.velocity)) is var speed && speed < 1)
            {
                DisableValueChangeCounter();
                DisableScrollTimer();
                InternalBeginSnap();
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !IsActive())
            {
                return;
            }

            _dragging = true;

            InternalStopTween();
            DisableValueChangeCounter();
            DisableScrollTimer();
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            // if (!_dragging || eventData.button != PointerEventData.InputButton.Left || !IsActive())
            // {
            //     return;
            // }

            /*
             * 这里暂时没有逻辑
             * 不过，为了让 OnBeginDrag 和 OnEndDrag 执行，必须实现 OnDrag
             */
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || !IsActive())
            {
                return;
            }

            _dragging = false;

            if ((snapTrigger & ScrollViewSnapTrigger.OnEndDrag) != 0 && !IsTweening())
            {
                DisableValueChangeCounter();
                DisableScrollTimer();
                InternalBeginSnap();
            }
        }

        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            if (!IsActive() || !eventData.IsScrolling())
            {
                return;
            }

            _scrolling = true;

            InternalStopTween();
            DisableValueChangeCounter();
            EnableScrollTimer();
        }

        void IPlayerLoopItem.Run(PlayerLoopPhase playerLoopPhase)
        {
            if (playerLoopPhase == PlayerLoopPhase.LateUpdating)
            {
                /*
                 * ScrollRect 在 ScrollRect.LateUpdate 中处理速度
                 * 在那之前限速
                 */
                LimitSpeed();
            }
            else if (playerLoopPhase == PlayerLoopPhase.LateUpdated)
            {
                /*
                 * ScrollRect 在 ScrollRect.LateUpdate 中引发 onValueChanged 事件
                 * 我们向 ScrollRect.onValueChanged 注册了回调（通过 _scrollRectWrapper.OnValueChanged）
                 * 在回调方法（OnScrollRectValueChanged）里，可能会创建新项，但是我们只是将新项设为活动的，并没有处理可见性
                 * 由于在其它脚本的 LateUpdate 中，可能会修改此滚动视图的位置，导致那些刚刚创建的新项的可见性发生变化
                 * 所以，不如我们在一个单独的地方处理新项的可见性，以及已有项的可见性
                 * 注意，在回收项的时候，它们依然会立即设为不可见，然后设为不活动
                 */
                RefreshItemsVisibleState();
                _scrolling = false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LimitSpeed()
        {
            if (speedLimit > 0 && Get(scrollRect.velocity) is var velocity // 速度
                && Mathf.Abs(velocity) is var speed                        // 速率
                && speed > speedLimit)
            {
                scrollRect.velocity = Set(scrollRect.velocity, speedLimit * Mathf.Sign(velocity));
            }
        }

        private void RefreshItemsVisibleState()
        {
            if (_itemCount == 0 || _activeItems.Count == 0)
            {
                return;
            }

            var beginContentPosition = InternalConvertNormalizedViewportPositionToContentPosition(0);
            var endContentPosition   = InternalConvertNormalizedViewportPositionToContentPosition(1);
            var firstVisibleIndex    = InternalFindFirstIndex(beginContentPosition);
            var lastVisibleIndex     = InternalFindLastIndex(endContentPosition);
            if (firstVisibleIndex >= 0 && lastVisibleIndex >= 0 && firstVisibleIndex <= lastVisibleIndex)
            {
                foreach (var item in _activeItems)
                {
                    var index = item.index;
                    if (index >= firstVisibleIndex && index <= lastVisibleIndex)
                    {
                        if (!item.visible)
                        {
                            item.visible = true;
                            item.OnVisible(this);
                        }
                    }
                    else
                    {
                        if (item.visible)
                        {
                            item.visible = false;
                            item.OnInvisible(this);
                        }
                    }
                }
            }
            else
            {
                foreach (var item in _activeItems)
                {
                    if (item.visible)
                    {
                        item.visible = false;
                        item.OnInvisible(this);
                    }
                }
            }
        }

        private static void OnValueChangeCounterTriggered(ICounter counter, object state)
        {
            var scrollView = (ScrollView) state;

            Assert.IsFalse(scrollView.IsTweening());

            if (!scrollView._dragging &&
                (scrollView.snapTrigger & ScrollViewSnapTrigger.OnNormalizedScrollPositionChanged) != 0 &&
                scrollView.snapSpeedThreshold > 0 &&
                Mathf.Abs(scrollView.Get(scrollView.scrollRect.velocity)) is var speed)
            {
                if (speed < scrollView.snapSpeedThreshold)
                {
                    scrollView.InternalBeginSnap();
                }
                else
                {
                    scrollView.EnableValueChangeCounter();
                }
            }
        }

        private static void OnScrollTimerTriggered(ITimer timer, object state)
        {
            var scrollView = (ScrollView) state;

            Assert.IsFalse(scrollView.IsTweening());

            if (!scrollView._dragging &&
                (scrollView.snapTrigger & ScrollViewSnapTrigger.OnNormalizedScrollPositionChanged) != 0 &&
                scrollView.snapSpeedThreshold > 0 &&
                Mathf.Abs(scrollView.Get(scrollView.scrollRect.velocity)) is var speed &&
                speed < scrollView.snapSpeedThreshold)
            {
                scrollView.InternalBeginSnap();
            }
        }

        /// <param name="value"><see cref="ScrollRect.normalizedPosition"/>。</param>
        /// <remarks><see cref="ScrollRect"/> 在 <see cref="ScrollRect.LateUpdate"/> 检测到任何更改时引发此事件。</remarks>
        private void OnScrollRectValueChanged(Vector2 value)
        {
            if (!_dragging && !_scrolling && !IsTweening())
            {
                if (scrollbar && scrollbar.IsActive() && SelectableUtility.IsPressed(scrollbar))
                {
                    DisableValueChangeCounter();
                    EnableScrollTimer();
                }
                else if ((snapTrigger & ScrollViewSnapTrigger.OnNormalizedScrollPositionChanged) != 0 &&
                         snapSpeedThreshold > 0 && Mathf.Abs(Get(scrollRect.velocity)) is var speed)
                {
                    if (!(speed < snapSpeedThreshold))
                    {
                        EnableValueChangeCounter();
                        DisableScrollTimer();
                    }
                }
            }
            InternalRefresh();
        }

        protected override void OnDestroy()
        {
            while (_activeItems.TryDequeueLast(out var item))
            {
                ReturnItem(item, true);
            }

            base.OnDestroy();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            PlayerLoopUtility.AddPlayerLoopItem(this, PlayerLoopPhase.LateUpdating);
            PlayerLoopUtility.AddPlayerLoopItem(this, PlayerLoopPhase.LateUpdated);
            scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);

            _valueChangeCounter = new UnityFrameCountPlayerLoopCounter(
                OnValueChangeCounterTriggered,
                this,
                PlayerLoopPhase.LateUpdating
            );
            _scrollTimer = new UnityUnscaledTimePlayerLoopTimer(
                OnScrollTimerTriggered,
                this,
                PlayerLoopPhase.LateUpdating
            );
        }

        protected override void OnDisable()
        {
            PlayerLoopUtility.RemovePlayerLoopItem(this, PlayerLoopPhase.LateUpdating);
            PlayerLoopUtility.RemovePlayerLoopItem(this, PlayerLoopPhase.LateUpdated);
            scrollRect.onValueChanged.RemoveListener(OnScrollRectValueChanged);

            _valueChangeCounter.Dispose();
            _valueChangeCounter = null;
            _scrollTimer.Dispose();
            _scrollTimer = null;

            InternalStopTween();

            base.OnDisable();
        }
    }
}
