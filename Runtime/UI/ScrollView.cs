using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Collections;
using Aurora.Diagnostics;
using Aurora.Interpolations;
using Aurora.Unity.PlayerLoop;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 滚动视图。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(ScrollRect))]
    public abstract class ScrollView : MonoBehaviour,
                                       IPointerDownHandler,
                                       IPointerUpHandler,
                                       IBeginDragHandler,
                                       IDragHandler,
                                       IEndDragHandler,
                                       IPlayerLoopItem
    {
        private const string NotInitialized =
            "The instance has not been initialized. Ensure " + nameof(Initialize) + " has been called.";

        private const string ControllerUnset = "The controller has not been set. Call " +
                                               nameof(SetControllerAndReload) + " before using this method.";

        private static readonly ParameterizedPredicate<ScrollViewItem, ScrollViewItem> AreIdentifierEqual =
            (a, b) => a.identifier == b.identifier;

        private static readonly ParameterizedPredicate<ScrollViewItem, int> IndexEqualTo = (scrollViewItem, index) =>
            scrollViewItem.index == index;

        [SerializeField]
        internal RectOffset padding = new RectOffset();

        [SerializeField]
        [Min(0)]
        internal float spacing;

        [SerializeField]
        internal bool itemForceExpand;

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
        /// 控制中止补间动画的行为。
        /// </summary>
        public ScrollerStopTween stopTween;

        /// <summary>
        /// 自动吸附的触发条件。
        /// </summary>
        public ScrollViewSnapTrigger snapTrigger;

        [Range(0, 1)]
        public float snapFindNormalizedPosition = 0.5f;

        [Range(0, 1)]
        public float snapJumpToNormalizedPosition = 0.5f;

        [Range(0, 1)]
        public float snapNormalizedCellPosition = 0.5f;

        public bool snapIncludingPaddingOrSpacingBeforeAndAfterCell;

        /// <summary>
        /// 如何计算自动吸附的耗时。
        /// </summary>
        public ScrollViewSnapDurationMode snapDurationMode;

        /// <summary>
        /// 自动吸附的耗时。
        /// </summary>
        /// <remarks>当 <see cref="snapDurationMode"/> 为 <see cref="ScrollViewSnapDurationMode.Fixed"/> 时，将使用此值。</remarks>
        [Min(0)]
        public float snapDuration;

        /// <summary>
        /// 自动吸附的速度。
        /// </summary>
        /// <remarks>当 <see cref="snapDurationMode"/> 为 <see cref="ScrollViewSnapDurationMode.Dynamic"/> 时，将使用此值。</remarks>
        [Min(0)]
        public float snapSpeed;

        [SerializeField]
        internal ScrollbarVisibility scrollbarVisibility = ScrollbarVisibility.OnlyIfNeeded;

        private bool _initialized;

        private bool _enabled;

        private bool _eventSubscribed;

        private RectTransform _rectTransform;

        private RectTransform _inactiveContainer;

        private ScrollRect _scrollRect;

        private RectTransform _contentRectTransform;

        private LayoutElement _leadingPlaceholder;

        private LayoutElement _trailingPlaceholder;

        private HorizontalOrVerticalLayoutGroup _contentHorizontalOrVerticalLayoutGroup;

        private Scrollbar _scrollbar;

        private IScrollViewController _controller;

        private CancellationTokenSource _tweenTokenSource;

        private bool _dragging;

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

        /// <summary>
        /// 获取一个值，这个值指示此实例是否已初始化。
        /// </summary>
        public bool Initialized => _initialized;

        /// <summary>
        /// 获取此 <see cref="ScrollView"/> 的控制器。
        /// </summary>
        /// <exception cref="InvalidOperationException">此实例未初始化。</exception>
        public IScrollViewController Controller =>
            !_initialized ? throw new InvalidOperationException(NotInitialized) : _controller;

        /// <summary>
        /// 获取此 <see cref="ScrollView"/> 的大小。
        /// </summary>
        /// <exception cref="InvalidOperationException">此实例未初始化。</exception>
        public float Size => !_initialized ? throw new InvalidOperationException(NotInitialized) : GetSize();

        /// <summary>
        /// 获取此 <see cref="ScrollView"/> 的内容大小。
        /// </summary>
        /// <exception cref="InvalidOperationException">此实例未初始化。</exception>
        public float ContentSize => !_initialized
                                        ? throw new InvalidOperationException(NotInitialized)
                                        : GetContentSize();

        /// <summary>
        /// 获取此 <see cref="ScrollView"/> 的内容超出此 <see cref="ScrollView"/> 的大小。
        /// </summary>
        /// <exception cref="InvalidOperationException">此实例未初始化。</exception>
        public float OverflowedContentSize => !_initialized
                                                  ? throw new InvalidOperationException(NotInitialized)
                                                  : GetOverflowedContentSize();

        /// <summary>
        /// 获取或设置此 <see cref="ScrollView"/> 的内容位置。
        /// </summary>
        /// <exception cref="InvalidOperationException">此实例未初始化。</exception>
        public float ContentPosition
        {
            get => !_initialized ? throw new InvalidOperationException(NotInitialized) : GetContentPosition();
            set
            {
                if (!_initialized)
                {
                    throw new InvalidOperationException(NotInitialized);
                }
                SetContentPosition(value);
            }
        }

        /// <summary>
        /// 获取或设置此 <see cref="ScrollView"/> 的内容的内边距。
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> 有至少一个分量为负数。</exception>
        public RectOffset Padding
        {
            get => padding;
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
                // 直接赋值，不做相等性检查，因为 RectOffset 是引用类型，可以在实例不变的情况下修改其成员
                padding = value;
                if (_initialized)
                {
                    _contentHorizontalOrVerticalLayoutGroup.padding = padding;
                    InternalReloadWithNormalizedScrollPosition(NormalizedScrollPosition);
                }
            }
        }

        /// <summary>
        /// 获取或设置此 <see cref="ScrollView"/> 中各项的间距。
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> 不满足“大于或等于 0”。</exception>
        public float Spacing
        {
            get => spacing;
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
                if (_initialized)
                {
                    _contentHorizontalOrVerticalLayoutGroup.spacing = spacing;
                    InternalReloadWithNormalizedScrollPosition(NormalizedScrollPosition);
                }
            }
        }

        /// <summary>
        /// 获取或设置此 <see cref="ScrollView"/> 的标准化滚动位置。
        /// </summary>
        /// <exception cref="InvalidOperationException">此实例未初始化。</exception>
        public float NormalizedScrollPosition
        {
            get => !_initialized
                       ? throw new InvalidOperationException(NotInitialized)
                       : InternalConvertContentPositionToNormalizedScrollPosition(GetContentPosition());
            set
            {
                if (!_initialized)
                {
                    throw new InvalidOperationException(NotInitialized);
                }
                SetContentPosition(InternalConvertNormalizedScrollPositionToContentPosition(value));
            }
        }

        /// <summary>
        /// 获取一个值，这个值表示此 <see cref="ScrollView"/> 是否正在被拖拽。
        /// </summary>
        public bool Dragging => _dragging;

        /// <summary>
        /// 获取一个值，这个值表示此 <see cref="ScrollView"/> 是否正在播放补间动画。
        /// </summary>
        public bool Tweening => _tweenTokenSource != null;

        /// <summary>
        /// 获取第一个活动项的索引。如果没有活动项，则返回 -1。
        /// </summary>
        /// <exception cref="InvalidOperationException">此实例未初始化。</exception>
        public int FirstActiveIndex => !_initialized
                                           ? throw new InvalidOperationException(NotInitialized)
                                           : _activeItems.TryPeekFirst(out var item)
                                               ? item.index
                                               : -1;

        /// <summary>
        /// 获取第一个可见项的索引。如果没有可见项，则返回 -1.
        /// </summary>
        /// <exception cref="InvalidOperationException">此实例未初始化。</exception>
        public int FirstVisibleIndex => !_initialized
                                            ? throw new InvalidOperationException(NotInitialized)
                                            : InternalFindFirstIndex(ConvertNormalizedPositionToContentPosition(0));

        /// <summary>
        /// 获取最后一个可见项的索引。如果没有可见项，则返回 -1.
        /// </summary>
        /// <exception cref="InvalidOperationException">此实例未初始化。</exception>
        public int LastVisibleIndex => !_initialized
                                           ? throw new InvalidOperationException(NotInitialized)
                                           : InternalFindLastIndex(ConvertNormalizedPositionToContentPosition(1));

        /// <summary>
        /// 获取最后一个活动项的索引。如果没有活动项，则返回 -1。
        /// </summary>
        /// <exception cref="InvalidOperationException">此实例未初始化。</exception>
        public int LastActiveIndex => !_initialized
                                          ? throw new InvalidOperationException(NotInitialized)
                                          : _activeItems.TryPeekLast(out var item)
                                              ? item.index
                                              : -1;

        /// <summary>
        /// 项的数量。
        /// </summary>
        /// <exception cref="InvalidOperationException">此实例未初始化。</exception>
        public int ItemCount => !_initialized ? throw new InvalidOperationException(NotInitialized) : _itemCount;

        /// <summary>
        /// 获取位于指定索引处的项。
        /// </summary>
        /// <param name="index">索引。</param>
        /// <exception cref="InvalidOperationException">此实例未初始化。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> 小于 0，或者大于或等于 <see cref="ItemCount"/>。</exception>
        public ScrollViewItem this[int index]
        {
            get
            {
                if (!_initialized)
                {
                    throw new InvalidOperationException(NotInitialized);
                }
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
            get => scrollbarVisibility;
            set
            {
                if (scrollbarVisibility == value)
                {
                    return;
                }
                scrollbarVisibility = value;
                if (_initialized)
                {
                    RefreshScrollbarVisibility();
                }
            }
        }

        /// <summary>
        /// 返回应设置内容矩形变换的 <see cref="RectTransform.anchorMin"/> 为的值。
        /// </summary>
        protected abstract Vector2 ContentRectTransformAnchorMin { get; }

        /// <summary>
        /// 返回应设置内容矩形变换的 <see cref="RectTransform.anchorMax"/> 为的值。
        /// </summary>
        protected abstract Vector2 ContentRectTransformAnchorMax { get; }

        /// <summary>
        /// 返回应设置内容矩形变换的 <see cref="RectTransform.pivot"/> 为的值。
        /// </summary>
        protected abstract Vector2 ContentRectTransformPivot { get; }

        /// <summary>
        /// 返回要添加到内容游戏物体上的 <see cref="HorizontalOrVerticalLayoutGroup"/> 具体组件类型。
        /// </summary>
        protected abstract Type ContentHorizontalOrVerticalLayoutGroupType { get; }

        /// <summary>
        /// 返回应设置 <see cref="ScrollRect.horizontal"/> 为的值。
        /// </summary>
        protected abstract bool ScrollRectHorizontal { get; }

        /// <summary>
        /// 返回应设置 <see cref="ScrollRect.vertical"/> 为的值。
        /// </summary>
        protected abstract bool ScrollRectVertical { get; }

        /// <summary>
        /// 返回沿轴向的内边距。
        /// </summary>
        protected abstract float PaddingAlongAxis { get; }

        /// <summary>
        /// 返回沿轴向的前半部分内边距。
        /// </summary>
        protected abstract float FirstPaddingAlongAxis { get; }

        /// <summary>
        /// 返回沿轴向的后半部分内边距。
        /// </summary>
        protected abstract float LastPaddingAlongAxis { get; }

        /// <summary>
        /// 将 <paramref name="layoutElement"/> 的 <see cref="LayoutElement.minWidth"/> 或 <see cref="LayoutElement.minHeight"/> 设置为 <paramref name="size"/>。
        /// </summary>
        protected abstract void Set(LayoutElement layoutElement, float size);

        /// <summary>
        /// 返回 <paramref name="vector2"/> 的沿轴向分量。
        /// </summary>
        protected abstract float Get(Vector2 vector2);

        /// <summary>
        /// 将 <paramref name="vector2"/> 的沿轴向分量设置为 <paramref name="value"/>，然后返回设置后的值。
        /// </summary>
        protected abstract Vector2 Set(Vector2 vector2, float value);

        /// <summary>
        /// 是否允许扩展项的宽度。
        /// </summary>
        protected abstract bool CanExpandItemWidth { get; }

        /// <summary>
        /// 是否允许扩展项的高度。
        /// </summary>
        protected abstract bool CanExpandItemHeight { get; }

        /// <summary>
        /// 返回沿轴向的滚动条。
        /// </summary>
        protected abstract Scrollbar GetScrollbar(ScrollRect scrollRect);

        /// <summary>
        /// 设置沿轴向的滚动条。
        /// </summary>
        protected abstract void SetScrollbar(ScrollRect scrollRect, Scrollbar scrollbar);

        /// <summary>
        /// 初始化此 <see cref="ScrollView"/>。
        /// </summary>
        public void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _rectTransform = (RectTransform) transform;

            _scrollRect            = GetComponent<ScrollRect>();
            _scrollRect.horizontal = ScrollRectHorizontal;
            _scrollRect.vertical   = ScrollRectVertical;

            var inactiveContainerGameObject = new GameObject("Inactive Container (Generated)");
            inactiveContainerGameObject.SetActive(false);
            _inactiveContainer = inactiveContainerGameObject.AddComponent<RectTransform>();
            _inactiveContainer.SetParent(_rectTransform, false);
            _inactiveContainer.localPosition    = Vector3.zero;
            _inactiveContainer.localRotation    = Quaternion.identity;
            _inactiveContainer.localScale       = Vector3.one;
            _inactiveContainer.anchorMin        = new Vector2(0.5f, 0.5f);
            _inactiveContainer.anchorMax        = new Vector2(0.5f, 0.5f);
            _inactiveContainer.anchoredPosition = Vector2.zero;
            _inactiveContainer.sizeDelta        = Vector2.zero;
            _inactiveContainer.pivot            = new Vector2(0.5f, 0.5f);

            if (_scrollRect.content != null)
            {
                DestroyImmediate(_scrollRect.content.gameObject);
            }

            var contentGameObject = new GameObject("Content (Generated)");
            {
                _contentRectTransform = contentGameObject.AddComponent<RectTransform>();
                _contentRectTransform.SetParent(_rectTransform, false);
                _contentRectTransform.localPosition    = Vector3.zero;
                _contentRectTransform.localRotation    = Quaternion.identity;
                _contentRectTransform.localScale       = Vector3.one;
                _contentRectTransform.anchorMin        = ContentRectTransformAnchorMin;
                _contentRectTransform.anchorMax        = ContentRectTransformAnchorMax;
                _contentRectTransform.anchoredPosition = Vector2.zero;
                _contentRectTransform.sizeDelta        = Vector2.zero;
                _contentRectTransform.pivot            = ContentRectTransformPivot;

                _scrollRect.content = _contentRectTransform;

                {
                    var leadingPlaceholderGameObject = new GameObject("Leading Placeholder (Generated)");
                    leadingPlaceholderGameObject.SetActive(false);
                    var leadingPlaceholderRectTransform = leadingPlaceholderGameObject.AddComponent<RectTransform>();
                    leadingPlaceholderRectTransform.SetParent(_contentRectTransform, false);
                    leadingPlaceholderRectTransform.SetAsFirstSibling();
                    leadingPlaceholderRectTransform.localPosition = Vector3.zero;
                    leadingPlaceholderRectTransform.localRotation = Quaternion.identity;
                    leadingPlaceholderRectTransform.localScale = Vector3.one;
                    _leadingPlaceholder = leadingPlaceholderGameObject.AddComponent<LayoutElement>();
                    Set(_leadingPlaceholder, 0);

                    var trailingPlaceholderGameObject = new GameObject("Trailing Placeholder (Generated)");
                    trailingPlaceholderGameObject.SetActive(false);
                    var trailingPlaceholderRectTransform = trailingPlaceholderGameObject.AddComponent<RectTransform>();
                    trailingPlaceholderRectTransform.SetParent(_contentRectTransform, false);
                    trailingPlaceholderRectTransform.SetAsLastSibling();
                    trailingPlaceholderRectTransform.localPosition = Vector3.zero;
                    trailingPlaceholderRectTransform.localRotation = Quaternion.identity;
                    trailingPlaceholderRectTransform.localScale = Vector3.one;
                    _trailingPlaceholder = trailingPlaceholderGameObject.AddComponent<LayoutElement>();
                    Set(_trailingPlaceholder, 0);
                }
            }

            _contentHorizontalOrVerticalLayoutGroup =
                (HorizontalOrVerticalLayoutGroup) contentGameObject.AddComponent(
                    ContentHorizontalOrVerticalLayoutGroupType
                );
            _contentHorizontalOrVerticalLayoutGroup.padding                = padding;
            _contentHorizontalOrVerticalLayoutGroup.spacing                = spacing;
            _contentHorizontalOrVerticalLayoutGroup.childAlignment         = TextAnchor.UpperLeft;
            _contentHorizontalOrVerticalLayoutGroup.childForceExpandWidth  = itemForceExpand && CanExpandItemWidth;
            _contentHorizontalOrVerticalLayoutGroup.childForceExpandHeight = itemForceExpand && CanExpandItemHeight;

            _scrollbar = GetScrollbar(_scrollRect);
            if (_scrollbar)
            {
                switch (scrollbarVisibility)
                {
                    case ScrollbarVisibility.Never:
                    case ScrollbarVisibility.OnlyIfNeeded:
                        _scrollbar.gameObject.SetActive(false);
                        SetScrollbar(_scrollRect, null); // 防止 ScrollRect 设置 ScrollBar 为活动
                        break;
                    case ScrollbarVisibility.Always:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // 确保已经赋值，如果仍然存在错误，要确保错误不进入发布环境
            Assert.IsNotNull(_rectTransform);
            Assert.IsNotNull(_scrollRect);
            Assert.IsNotNull(_contentRectTransform);
            Assert.IsNotNull(_inactiveContainer);
            Assert.IsNotNull(_leadingPlaceholder);
            Assert.IsNotNull(_trailingPlaceholder);
            Assert.IsNotNull(_contentHorizontalOrVerticalLayoutGroup);

            _initialized = true;
            if (_enabled && !_eventSubscribed)
            {
                SubscribeEvent();
                _eventSubscribed = true;
            }
        }

        private float GetSize()
        {
            return Get(_rectTransform.rect.size);
        }

        private float GetContentSize()
        {
            return Get(_contentRectTransform.rect.size);
        }

        private float GetOverflowedContentSize()
        {
            return Mathf.Max(GetContentSize() - GetSize(), 0);
        }

        private float GetContentPosition()
        {
            return Get(_contentRectTransform.anchoredPosition);
        }

        private void SetContentPosition(float contentPosition)
        {
            _contentRectTransform.anchoredPosition = Set(_contentRectTransform.anchoredPosition, contentPosition);
        }

        /// <summary>
        /// 设置控制器，保持当前内容位置，重新加载。
        /// </summary>
        /// <param name="controller">控制器。</param>
        public void SetControllerAndReload(IScrollViewController controller)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException(NotInitialized);
            }
            _controller = controller;
            InternalReload();
        }

        /// <summary>
        /// 设置控制器，设置内容位置，重新加载。
        /// </summary>
        /// <param name="controller">控制器。</param>
        /// <param name="contentPosition">内容位置。</param>
        public void SetControllerAndReloadWithContentPosition(IScrollViewController controller, float contentPosition)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException(NotInitialized);
            }
            _controller = controller;
            InternalReloadWithContentPosition(contentPosition);
        }

        /// <summary>
        /// 设置控制器，设置标准化滚动位置，重新加载。
        /// </summary>
        /// <param name="controller">控制器。</param>
        /// <param name="normalizedScrollPosition">标准化滚动位置。</param>
        public void SetControllerAndReloadWithNormalizedScrollPosition(
            IScrollViewController controller,
            float                 normalizedScrollPosition)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException(NotInitialized);
            }
            _controller = controller;
            InternalReloadWithNormalizedScrollPosition(normalizedScrollPosition);
        }

        /// <summary>
        /// 保持当前内容位置，重新加载。
        /// </summary>
        /// <remarks>调用方应确保在模型数据改变后立即调用一次。</remarks>
        public void Reload()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException(NotInitialized);
            }
            InternalReload();
        }

        /// <summary>
        /// 设置内容位置，重新加载。
        /// </summary>
        /// <param name="contentPosition">内容位置。</param>
        /// <remarks>调用方应确保在模型数据改变后立即调用一次。</remarks>
        public void ReloadWithContentPosition(float contentPosition)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException(NotInitialized);
            }
            InternalReloadWithContentPosition(contentPosition);
        }

        /// <summary>
        /// 设置标准化滚动位置，重新加载。
        /// </summary>
        /// <param name="normalizedScrollPosition">标准化滚动位置。</param>
        /// <remarks>调用方应确保在模型数据改变后立即调用一次。</remarks>
        public void ReloadWithNormalizedScrollPosition(float normalizedScrollPosition)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException(NotInitialized);
            }
            InternalReloadWithNormalizedScrollPosition(normalizedScrollPosition);
        }

        private void InternalReload()
        {
            ReturnAllItems();
            ReloadItemCountAndBeginEndPositions();
            RefreshContentSize();
            ContentPosition = 0;
            AddItems();
            RefreshScrollbarVisibility();
        }

        private void InternalReloadWithContentPosition(float contentPosition)
        {
            ReturnAllItems();
            ReloadItemCountAndBeginEndPositions();
            RefreshContentSize();
            ContentPosition = contentPosition;
            AddItems();
            RefreshScrollbarVisibility();
        }

        private void InternalReloadWithNormalizedScrollPosition(float normalizedScrollPosition)
        {
            ReturnAllItems();
            ReloadItemCountAndBeginEndPositions();
            RefreshContentSize();
            NormalizedScrollPosition = normalizedScrollPosition;
            AddItems();
            RefreshScrollbarVisibility();
        }

        /// <summary>
        /// 回收超出范围的项、添加新进入范围的项。
        /// </summary>
        /// <remarks>此方法将在 <see cref="ScrollRect.onValueChanged"/> 事件引发时调用。</remarks>
        public void Refresh()
        {
            InternalRefresh();
        }

        private void InternalRefresh()
        {
            var beginActiveContentPosition = InternalConvertNormalizedPositionToContentPosition(0) -
                                             (leadingActiveOffset >= 0 ? leadingActiveOffset : 0);
            var endActiveContentPosition = InternalConvertNormalizedPositionToContentPosition(1) +
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
                for (var itemIndex = 0; itemIndex < _itemCount; itemIndex++)
                {
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

                    var beginPosition =
                        itemIndex == 0 ? FirstPaddingAlongAxis : _itemPositions[itemIndex * 2 - 1] + spacing;
                    _itemPositions.Add(beginPosition);

                    var endPosition = beginPosition + itemSize;
                    _itemPositions.Add(endPosition);
                }
            }
        }

        private void RefreshContentSize()
        {
            var contentSize = _itemCount == 0
                                  ? PaddingAlongAxis
                                  : _itemPositions[_itemCount * 2 - 1] + LastPaddingAlongAxis;
            _contentRectTransform.sizeDelta = Set(_contentRectTransform.sizeDelta, contentSize);
        }

        private void AddItems()
        {
            var beginActiveContentPosition = InternalConvertNormalizedPositionToContentPosition(0) -
                                             (leadingActiveOffset >= 0 ? leadingActiveOffset : 0);
            var endActiveContentPosition = InternalConvertNormalizedPositionToContentPosition(1) +
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
                leadingPlaceholderSize    = 0f;
                trailingPlaceholderActive = false;
                trailingPlaceholderSize   = 0f;
            }
            else
            {
                if (_activeItems.PeekFirst().index is var firstActiveIndex && firstActiveIndex == 0)
                {
                    leadingPlaceholderActive = false;
                    leadingPlaceholderSize   = 0f;
                }
                else
                {
                    leadingPlaceholderActive = true;
                    leadingPlaceholderSize   = _itemPositions[firstActiveIndex * 2] - _itemPositions[0] - spacing;
                }

                if (_activeItems.PeekLast().index is var lastActiveIndex && lastActiveIndex == _itemCount - 1)
                {
                    trailingPlaceholderActive = false;
                    trailingPlaceholderSize   = 0f;
                }
                else
                {
                    trailingPlaceholderActive = true;
                    trailingPlaceholderSize = _itemPositions[_itemCount * 2 - 1] -
                                              _itemPositions[lastActiveIndex * 2 + 1] - spacing;
                }
            }

            _leadingPlaceholder.gameObject.SetActive(leadingPlaceholderActive);
            Set(_leadingPlaceholder, leadingPlaceholderSize);
            _trailingPlaceholder.gameObject.SetActive(trailingPlaceholderActive);
            Set(_trailingPlaceholder, trailingPlaceholderSize);
        }

        private void RefreshScrollbarVisibility()
        {
            if (_scrollbar)
            {
                SetScrollbar(_scrollRect, _scrollbar);
                var scrollbarActive = scrollbarVisibility switch
                {
                    ScrollbarVisibility.Never        => false,
                    ScrollbarVisibility.OnlyIfNeeded => GetOverflowedContentSize() > 0,
                    ScrollbarVisibility.Always       => true,
                    _                                => throw new ArgumentOutOfRangeException()
                };
                _scrollbar.gameObject.SetActive(scrollbarActive);
                if (!scrollbarActive)
                {
                    SetScrollbar(_scrollRect, null); // 防止 ScrollRect 设置 ScrollBar 为活动
                }
            }
        }

        /// <summary>
        /// 终止补间动画。
        /// </summary>
        public void StopTween()
        {
            if (_tweenTokenSource == null)
            {
                return;
            }
            Log.V("终止补间动画");
            try
            {
                _tweenTokenSource.Cancel();
                _tweenTokenSource.Dispose();
            }
            finally
            {
                _tweenTokenSource = null;
            }
        }

        /// <summary>
        /// 吸附。
        /// </summary>
        public void Snap()
        {
            StopTween();
            InternalBeginSnap();
        }

        private void SnapIfTweenNotExists()
        {
            if (_tweenTokenSource == null)
            {
                InternalBeginSnap();
            }
        }

        private async void InternalBeginSnap()
        {
            Assert.IsNull(_tweenTokenSource);

            var contentPosition1 = ConvertNormalizedPositionToContentPosition(snapFindNormalizedPosition);

            var       exitToken        = UnityEnvironment.ExitToken;
            var       disableToken     = gameObject.GetDisableToken();
            using var tweenTokenSource = CancellationTokenSource.CreateLinkedTokenSource(exitToken, disableToken);
            var       tweenToken       = tweenTokenSource.Token;
            _tweenTokenSource = tweenTokenSource;
            try
            {
                await InternalSnapAsync(tweenToken);
            }
            catch (OperationCanceledException)
            {
                Log.V("补间动画已终止");
            }
            finally
            {
                if (_tweenTokenSource == tweenTokenSource)
                {
                    _tweenTokenSource = null;
                }
            }
        }

        private async Task InternalSnapAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var nearestIndex =
                InternalFindClosestIndex(ConvertNormalizedPositionToContentPosition(snapFindNormalizedPosition));
            if (nearestIndex < 0)
            {
                return;
            }

            // TODO
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
        /// 寻找最靠近指定的此 <see cref="ScrollView"/> 的内容位置的项的索引。
        /// </summary>
        /// <param name="contentPosition">内容位置。</param>
        /// <returns>如果项的数量大于 0，则为最靠近 <paramref name="contentPosition"/> 的项的索引；否则为 -1.</returns>
        /// <exception cref="InvalidOperationException">此实例未初始化。</exception>
        public int FindClosestIndex(float contentPosition)
        {
            if (!_initialized)
            {
                throw new InvalidOperationException(NotInitialized);
            }
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
                var item = Instantiate(prefab, _inactiveContainer, false); // 确保不活动
#if UNITY_EDITOR
                item.name = prefabName;
#endif
                item.gameObject.SetActive(false);
                item.transform.SetParent(_contentRectTransform, false);
                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;
                item.transform.localScale    = Vector3.one;

                item.scrollView = this;

                isNewCreated = true;
                return item;
            }
        }

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
            Set(
                item.TryGetComponent(out LayoutElement layoutElement)
                    ? layoutElement
                    : item.gameObject.AddComponent<LayoutElement>(),
                _itemPositions[index * 2 + 1] - _itemPositions[index * 2]
            );
            return item;
        }

        private void AddFirstItem(int index)
        {
            var item = GetItem(index, out var isNewCreated);
            item.transform.SetSiblingIndex(1); // after _leadingPlaceholder
            _activeItems.EnqueueFirst(item);
            item.gameObject.SetActive(true);
            item.OnGet(this, isNewCreated);
        }

        private void AddLastItem(int index)
        {
            var item = GetItem(index, out var isNewCreated);
            item.transform.SetSiblingIndex(item.transform.parent.childCount - 2); // before _trailingPlaceholder
            _activeItems.EnqueueLast(item);
            item.gameObject.SetActive(true);
            item.OnGet(this, isNewCreated);
        }

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
        /// 将此 <see cref="ScrollView"/> 的内容位置转换为此 <see cref="ScrollView"/> 的标准化位置。
        /// </summary>
        public float ConvertContentPositionToNormalizedPosition(float contentPosition)
        {
            return InternalConvertContentPositionToNormalizedPosition(contentPosition);
        }

        /// <summary>
        /// 将此 <see cref="ScrollView"/> 的标准化位置转换为此 <see cref="ScrollView"/> 的内容位置。
        /// </summary>
        public float ConvertNormalizedPositionToContentPosition(float normalizedPosition)
        {
            return InternalConvertNormalizedPositionToContentPosition(normalizedPosition);
        }

        /// <summary>
        /// 将此 <see cref="ScrollView"/> 的内容位置转换为此 <see cref="ScrollView"/> 的标准化滚动位置。
        /// </summary>
        public float ConvertContentPositionToNormalizedScrollPosition(float contentPosition)
        {
            return InternalConvertContentPositionToNormalizedScrollPosition(contentPosition);
        }

        /// <summary>
        /// 将此 <see cref="ScrollView"/> 的标准化滚动位置转换为此 <see cref="ScrollView"/> 的内容位置。
        /// </summary>
        public float ConvertNormalizedScrollPositionToContentPosition(float normalizedScrollPosition)
        {
            return InternalConvertNormalizedScrollPositionToContentPosition(normalizedScrollPosition);
        }

        private float InternalConvertContentPositionToNormalizedPosition(float contentPosition)
        {
            var begin = GetContentPosition();
            var end   = begin + GetSize();
            return (float) InterpolationUtility.InverseLinearInterpolate(begin, end, contentPosition);
        }

        private float InternalConvertNormalizedPositionToContentPosition(float normalizedPosition)
        {
            var begin = GetContentPosition();
            var end   = begin + GetSize();
            return (float) InterpolationUtility.LinearInterpolate(begin, end, normalizedPosition);
        }

        private float InternalConvertContentPositionToNormalizedScrollPosition(float contentPosition)
        {
            var overflowedContentSize = GetOverflowedContentSize();
            return overflowedContentSize == 0 ? 0 : contentPosition / overflowedContentSize;
        }

        private float InternalConvertNormalizedScrollPositionToContentPosition(float normalizedScrollPosition)
        {
            var overflowedContentSize = GetOverflowedContentSize();
            return overflowedContentSize == 0 ? 0 : overflowedContentSize * normalizedScrollPosition;
        }

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            if ((stopTween & ScrollerStopTween.OnPointerDown) != 0)
            {
                StopTween();
            }
        }

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            // TODO
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (!_dragging)
            {
                _dragging = true;

                if ((stopTween & ScrollerStopTween.OnBeginDrag) != 0)
                {
                    StopTween();
                }
            }
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            // 暂时没有逻辑。
            // 不过，为了让 OnBeginDrag 和 OnEndDrag 执行，必须实现 OnDrag，因此保留此实现是合理的，即使什么也不做。
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (_dragging)
            {
                _dragging = false;
            }
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
            }
        }

        private void LimitSpeed()
        {
            if (speedLimit > 0 && Get(_scrollRect.velocity) is var velocity // 速度
                && Mathf.Abs(velocity) is var speed                         // 速率
                && speed > speedLimit)
            {
                _scrollRect.velocity = Set(_scrollRect.velocity, speedLimit * Mathf.Sign(velocity));
                Log.V($"当前速率 {speed} 超过速率限制 {speedLimit}，已执行限速");
            }
        }

        private void RefreshItemsVisibleState()
        {
            if (_itemCount == 0 || _activeItems.Count == 0)
            {
                return;
            }

            var beginContentPosition = InternalConvertNormalizedPositionToContentPosition(0);
            var endContentPosition   = InternalConvertNormalizedPositionToContentPosition(1);
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

        /// <param name="value"><see cref="ScrollRect.normalizedPosition"/>。</param>
        /// <remarks><see cref="ScrollRect"/> 在 <see cref="ScrollRect.LateUpdate"/> 检测到任何更改时引发此事件。</remarks>
        private void OnScrollRectValueChanged(Vector2 value)
        {
            if ((snapTrigger & ScrollViewSnapTrigger.OnNormalizedScrollPositionChanged) != 0)
            {
                SnapIfTweenNotExists();
            }
            InternalRefresh();
        }

        private void OnDestroy()
        {
            while (_activeItems.TryDequeueLast(out var item))
            {
                ReturnItem(item, true);
            }
        }

        private void SubscribeEvent()
        {
            PlayerLoopUtility.AddPlayerLoopItem(this, PlayerLoopPhase.LateUpdating);
            PlayerLoopUtility.AddPlayerLoopItem(this, PlayerLoopPhase.LateUpdated);
            _scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
        }

        private void UnsubscribeEvent()
        {
            PlayerLoopUtility.RemovePlayerLoopItem(this, PlayerLoopPhase.LateUpdating);
            PlayerLoopUtility.RemovePlayerLoopItem(this, PlayerLoopPhase.LateUpdated);
            _scrollRect.onValueChanged.RemoveListener(OnScrollRectValueChanged);
        }

        private void OnEnable()
        {
            _enabled = true;
            if (_initialized && !_eventSubscribed)
            {
                SubscribeEvent();
                _eventSubscribed = true;
            }
        }

        private void OnDisable()
        {
            _enabled = false;
            if (_initialized && _eventSubscribed)
            {
                UnsubscribeEvent();
                _eventSubscribed = false;
            }
        }
    }
}
