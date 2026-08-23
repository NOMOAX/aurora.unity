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
    /// A scroll view.
    /// </summary>
    /// <remarks>Features are implemented with the help of <see cref="UnityEngine.UI.ScrollRect"/>.</remarks>
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
        private const string ControllerUnset = "The controller has not been set.";

        private static readonly ParameterizedPredicate<ScrollViewItem, ScrollViewItem> AreIdentifierEqual =
            (a, b) => a.identifier == b.identifier;

        private static readonly ParameterizedPredicate<ScrollViewItem, int> HasIndex = (scrollViewItem, index) =>
            scrollViewItem.index == index;

        private static readonly CounterTriggerCallback OnValueChangeCounterTriggerCallback =
            (_, state) => ((ScrollView)state).OnValueChangeCounterTriggered();

        private static readonly TimerTriggerCallback OnScrollTimerTriggerCallback =
            (_, state) => ((ScrollView)state).OnScrollTimerTriggered();

#if UNITY_EDITOR
        private float _contentPosition;

        private double _normalizedScrollPosition;

        internal bool Dirty;
#endif

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
        internal RectOffset padding = new();

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
        /// The leading active (content position) offset. Used for preloading.
        /// </summary>
        /// <remarks>Set to a value greater than or equal to 0. After setting, it takes effect after the next refresh.</remarks>
        [Min(0)]
        public float leadingActiveOffset;

        /// <summary>
        /// The trailing active (content position) offset. Used for preloading.
        /// </summary>
        /// <remarks>Set to a value greater than or equal to 0. After setting, it takes effect after the next refresh.</remarks>
        [Min(0)]
        public float trailingActiveOffset;

        /// <summary>
        /// The speed limit. Takes effect when greater than 0.
        /// </summary>
        [Min(0)]
        public float speedLimit;

        /// <summary>
        /// The trigger conditions for auto-snap.
        /// </summary>
        public ScrollViewSnapTrigger snapTrigger;

        /// <summary>
        /// When the normalized scroll position changes, if the velocity is less than this value, auto-snap is triggered.
        /// </summary>
        [Min(0)]
        public float snapSpeedThreshold = 300;

        /// <summary>
        /// Used in computing the "auto-snap target position"; it indicates searching for the item closest to this normalized viewport position.
        /// </summary>
        [Range(0, 1)]
        public float snapFindNormalizedViewportPosition = 0.5f;

        /// <summary>
        /// Used in computing the "auto-snap target position"; it indicates whether the whitespace before and after an item should also be considered when computing the start and end positions of the item.
        /// <br/>
        /// The whitespace before or after an item means: if the item is the first item, the whitespace before it is the leading padding, otherwise it is the spacing; if the item is the last item, the whitespace after it is the trailing padding, otherwise it is the spacing.
        /// </summary>
        public bool snapIncludingSpacing;

        /// <summary>
        /// Used in computing the "auto-snap target position"; it indicates that when the item to snap to is found, this weight is used to linearly interpolate between the start and end positions of the item to compute the target position.
        /// </summary>
        [Range(0, 1)]
        public float snapNormalizedItemPosition = 0.5f;

        /// <summary>
        /// Used in computing the "auto-snap target position"; it indicates that the target position is gradually "snapped" to this normalized viewport position.
        /// </summary>
        [Range(0, 1)]
        public float snapJumpNormalizedViewportPosition = 0.5f;

        /// <summary>
        /// How the duration of auto-snap is computed.
        /// </summary>
        public ScrollViewSnapDurationMode snapDurationMode;

        /// <summary>
        /// The duration of auto-snap.
        /// </summary>
        /// <remarks>This value is used when <see cref="snapDurationMode"/> is <see cref="ScrollViewSnapDurationMode.Fixed"/>.</remarks>
        [Min(0)]
        public float snapDuration = 0.25f;

        /// <summary>
        /// The speed of auto-snap.
        /// </summary>
        /// <remarks>This value is used when <see cref="snapDurationMode"/> is <see cref="ScrollViewSnapDurationMode.Dynamic"/>.</remarks>
        [Min(0)]
        public float snapSpeed = 900;

        /// <summary>
        /// The interpolation type used during auto-snap.
        /// </summary>
        public Interpolation snapInterpolation = Interpolation.OutCubic;

        /// <summary>
        /// When <see cref="snapTrigger"/> defines the <see cref="ScrollViewSnapTrigger.OnNormalizedScrollPositionChanged"/> bit, if the normalized scroll position change is caused by operating the mouse wheel or the scrollbar, snap is performed after this delay instead of immediately every frame.
        /// </summary>
        [Range(0.2f, 0.4f)]
        public float scrollSnapDelay = 0.3f;

        private CancellationTokenSource _tweenTokenSource;

        private bool _dragging;

        /// <summary>
        /// Set to <see langword="true"/> in <see cref="IScrollHandler.OnScroll"/> (<see cref="EventSystem.Update">EventSystem.Update</see>),
        /// <br/>
        /// used in <see cref="OnScrollRectValueChanged"/> (<see cref="ScrollRect.LateUpdate">ScrollRect.LateUpdate</see>),
        /// <br/>
        /// set to <see langword="false"/> in <see cref="PlayerLoopPhase.LateUpdated"/>.
        /// </summary>
        [NonSerialized]
        private bool _scrolling;

        [NonSerialized]
        private ScrollRect.MovementType _scrollRectMovementTypeBeforeTween;

        [NonSerialized]
        private bool _scrollRectInertiaBeforeTween;

        private int _itemCount;

        /// <summary>
        /// The content positions at the start and end of all items. The internal arrangement is as follows:
        /// <list type="bullet">
        /// <item><description>The start position of the first item</description></item>
        /// <item><description>The end position of the first item</description></item>
        /// <item><description>The start position of the second item</description></item>
        /// <item><description>The end position of the second item</description></item>
        /// <item><description>... (omitted)</description></item>
        /// <item><description>The start position of the last item</description></item>
        /// <item><description>The end position of the last item</description></item>
        /// </list>
        /// </summary>
        /// <remarks>The length is 2 times <see cref="_itemCount"/>.</remarks>
        private readonly List<float> _itemPositions = new();

        /// <summary>
        /// The active <see cref="ScrollViewItem"/>s.
        /// </summary>
        private readonly Deque<ScrollViewItem> _activeItems = new();

        /// <summary>
        /// The recycled <see cref="ScrollViewItem"/>s.
        /// </summary>
        private readonly List<ScrollViewItem> _recycledItems = new();

        /// <remarks>When the content size is large and <see cref="snapSpeedThreshold"/> is small, <see cref="ScrollRect.onValueChanged">ScrollRect.onValueChanged</see> no longer triggers every frame and is unreliable, so the counter is enabled while it is still triggering, and the counter is continuously refreshed until the velocity drops below the threshold, so that snapping can be achieved when the velocity is below the threshold (when <see cref="snapTrigger"/> defines the <see cref="ScrollViewSnapTrigger.OnNormalizedScrollPositionChanged"/> bit).</remarks>
        private ICounter _valueChangeCounter;

        private ITimer _scrollTimer;

        /// <summary>
        /// Gets the controller.
        /// </summary>
        public IScrollViewController Controller
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _controller;
        }

        /// <summary>
        /// Gets the scroll area.
        /// </summary>
        public ScrollRect ScrollRect
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => scrollRect;
        }

        /// <summary>
        /// Gets the viewport size.
        /// </summary>
        public float ViewportSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetViewportSize();
        }

        /// <summary>
        /// Gets the content size.
        /// </summary>
        public float ContentSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetContentSize();
        }

        /// <summary>
        /// Gets the size by which the content exceeds the viewport.
        /// </summary>
        /// <remarks>If <see cref="ContentSize"/> is less than or equal to <see cref="ViewportSize"/>, it is 0.</remarks>
        public float OverflowedContentSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetOverflowedContentSize();
        }

        /// <summary>
        /// Gets or sets the content position.
        /// </summary>
        public float ContentPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetContentPosition();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetContentPosition(value);
        }

        /// <summary>
        /// Gets or sets the normalized scroll position.
        /// </summary>
        /// <remarks>
        /// The normalized scroll position is usually in the [0, 1] range; the <see cref="double"/> type is used to provide greater precision.
        /// <br/>
        /// Internally, <see cref="ScrollRect.normalizedPosition"/> is not used because its precision is insufficient.
        /// </remarks>
        public double NormalizedScrollPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetNormalizedScrollPosition();
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => SetNormalizedScrollPosition(value);
        }

        /// <summary>
        /// Gets or sets the padding of the content.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
        /// <exception cref="NotSupportedException">At least one component of <paramref name="value"/> is negative.</exception>
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
                    throw new NotSupportedException();
                }
                // Since RectOffset is a reference type, its internal members can be modified directly, so the equality check is skipped
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
        /// Returns the padding along the scrolling direction.
        /// </summary>
        protected abstract float PaddingAlongAxis { get; }

        /// <summary>
        /// Returns the first half of the padding along the scrolling direction.
        /// </summary>
        protected abstract float FirstPaddingAlongAxis { get; }

        /// <summary>
        /// Returns the last half of the padding along the scrolling direction.
        /// </summary>
        protected abstract float LastPaddingAlongAxis { get; }

        /// <summary>
        /// Gets or sets the spacing among items.
        /// </summary>
        /// <exception cref="NotSupportedException"><paramref name="value"/> does not satisfy "greater than or equal to 0".</exception>
        public float Spacing
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => spacing;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (!(value >= 0))
                {
                    throw new NotSupportedException();
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
        /// Gets or sets a value indicating whether items should fill the content in the non-scrolling direction.
        /// </summary>
        /// <remarks>For a horizontal scroll view, the item height fills the content height; for a vertical scroll view, the item width fills the content width.</remarks>
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
#if UNITY_EDITOR
                Dirty = true;
#endif
            }
        }

        /// <summary>
        /// Gets the index of the first active item. If there is no active item, returns -1.
        /// </summary>
        public int FirstActiveIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _activeItems.TryPeekFirst(out var item) ? item.index : -1;
        }

        /// <summary>
        /// Gets the index of the first visible item. If there is no visible item, returns -1.
        /// </summary>
        public int FirstVisibleIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var contentBeginPosition = InternalConvertNormalizedViewportPositionToContentPosition(0);
                var firstVisibleIndex    = InternalFindFirstIndex(contentBeginPosition);

                if (firstVisibleIndex < 0)
                {
                    return firstVisibleIndex;
                }

                var contentEndPosition = InternalConvertNormalizedViewportPositionToContentPosition(1);
                var lastVisibleIndex   = InternalFindLastIndex(contentEndPosition);

                return lastVisibleIndex >= 0 && firstVisibleIndex <= lastVisibleIndex ? firstVisibleIndex : -1;
            }
        }

        /// <summary>
        /// Gets the index of the last visible item. If there is no visible item, returns -1.
        /// </summary>
        public int LastVisibleIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var contentEndPosition = InternalConvertNormalizedViewportPositionToContentPosition(1);
                var lastVisibleIndex   = InternalFindLastIndex(contentEndPosition);

                if (lastVisibleIndex < 0)
                {
                    return lastVisibleIndex;
                }

                var contentBeginPosition = InternalConvertNormalizedViewportPositionToContentPosition(0);
                var firstVisibleIndex    = InternalFindFirstIndex(contentBeginPosition);

                return firstVisibleIndex >= 0 && firstVisibleIndex <= lastVisibleIndex ? lastVisibleIndex : -1;
            }
        }

        /// <summary>
        /// Gets the index of the last active item. If there is no active item, returns -1.
        /// </summary>
        public int LastActiveIndex
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _activeItems.TryPeekLast(out var item) ? item.index : -1;
        }

        /// <summary>
        /// Gets the number of items.
        /// </summary>
        public int ItemCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _itemCount;
        }

        /// <summary>
        /// Gets or sets the visibility of the scrollbar.
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
#if UNITY_EDITOR
                Dirty = true;
#endif
            }
        }

        /// <summary>
        /// Whether it is currently being dragged.
        /// </summary>
        public bool Dragging
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _dragging;
        }

        /// <summary>
        /// Whether a tween animation is currently playing.
        /// </summary>
        public bool Tweening
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsTweening();
        }

        /// <summary>
        /// Returns the scrolling-direction component of <paramref name="vector2"/>.
        /// </summary>
        protected abstract float Get(Vector2 vector2);

        /// <summary>
        /// Returns the content position.
        /// </summary>
        /// <param name="contentAnchoredPosition">The <see cref="RectTransform.anchoredPosition"/> of <see cref="content"/>.</param>
        /// <remarks>When the direction is horizontal, the sign is inverted.</remarks>
        protected abstract float GetContentPosition(Vector2 contentAnchoredPosition);

        /// <summary>
        /// Sets the scrolling-direction component of <paramref name="vector2"/> to <paramref name="value"/>, then returns the set value.
        /// </summary>
        protected abstract Vector2 Set(Vector2 vector2, float value);

        /// <summary>
        /// Sets the content position.
        /// </summary>
        /// <param name="contentAnchoredPosition">The <see cref="RectTransform.anchoredPosition"/> of <see cref="content"/>.</param>
        /// <param name="contentPosition">The content position.</param>
        /// <remarks>When the direction is horizontal, the sign is inverted.</remarks>
        protected abstract Vector2 SetContentPosition(Vector2 contentAnchoredPosition, float contentPosition);

        /// <summary>
        /// Sets the minimum size of <paramref name="layoutElement"/> along the scrolling direction.
        /// </summary>
        protected abstract void SetMinSize(LayoutElement layoutElement, float minSize);

        /// <summary>
        /// Sets whether items should fill the content in the non-scrolling direction.
        /// </summary>
        protected abstract void SetLayoutGroupChildForceExpandSize(
            HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup,
            bool                            forceExpand);

        /// <summary>
        /// Sets the scrollbar.
        /// </summary>
        /// <remarks>This is usually done to prevent <see cref="UnityEngine.UI.ScrollRect"/> from modifying the scrollbar visibility.</remarks>
        protected abstract void SetScrollRectScrollbar(ScrollRect sr, Scrollbar sb);

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
            return GetContentPosition(content.anchoredPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetContentPosition(float contentPosition)
        {
            content.anchoredPosition = SetContentPosition(content.anchoredPosition, contentPosition);
#if UNITY_EDITOR
            Dirty = true;
#endif
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
        /// Gets the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item if the item at the specified index is active; otherwise <see langword="null"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0, or greater than or equal to <see cref="ItemCount"/>.</exception>
        /// <remarks>Active items are refreshed in the callback registered by this class to <see cref="ScrollRect.onValueChanged">ScrollRect.onValueChanged</see>; if you need the latest active items immediately after calling <see cref="set_ContentPosition"/> or <see cref="set_NormalizedScrollPosition"/>, use <see cref="Refresh"/> to refresh manually.</remarks>
        public ScrollViewItem this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index < 0 || index >= _itemCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
                }
                return _activeItems.Find(HasIndex, index);
            }
        }

        /// <summary>
        /// Gets all active items of the specified type and stores them into the specified collection.
        /// </summary>
        /// <param name="results">The collection used to hold the results.</param>
        /// <typeparam name="T">The type of items to get.</typeparam>
        public void GetActiveItems<T>(ICollection<T> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }
            if (results.IsReadOnly)
            {
                throw new ArgumentException();
            }
            foreach (var item in _activeItems)
            {
                if (item is T t)
                {
                    results.Add(t);
                }
            }
        }

        /// <summary>
        /// Gets the begin (content) position of the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The begin (content) position of the item.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0, or greater than or equal to <see cref="ItemCount"/>.</exception>
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
        /// Gets the end (content) position of the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The end (content) position of the item.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than 0, or greater than or equal to <see cref="ItemCount"/>.</exception>
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
        /// Sets the controller, keeps the current content position, and reloads.
        /// </summary>
        /// <param name="controller">The controller.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetControllerAndReload(IScrollViewController controller)
        {
            _controller = controller;
            InternalReload();
        }

        /// <summary>
        /// Sets the controller, sets the content position, and reloads.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="contentPosition">The content position.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetControllerAndReloadWithContentPosition(IScrollViewController controller, float contentPosition)
        {
            _controller = controller;
            InternalReloadWithContentPosition(contentPosition);
        }

        /// <summary>
        /// Sets the controller, sets the normalized scroll position, and reloads.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <param name="normalizedScrollPosition">The normalized scroll position.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetControllerAndReloadWithNormalizedScrollPosition(
            IScrollViewController controller,
            float                 normalizedScrollPosition)
        {
            _controller = controller;
            InternalReloadWithNormalizedScrollPosition(normalizedScrollPosition);
        }

        /// <summary>
        /// Keeps the current content position and reloads.
        /// </summary>
        /// <remarks>The caller should ensure this is called once immediately after model data changes.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reload()
        {
            InternalReload();
        }

        /// <summary>
        /// Sets the content position and reloads.
        /// </summary>
        /// <param name="contentPosition">The content position.</param>
        /// <remarks>The caller should ensure this is called once immediately after model data changes.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReloadWithContentPosition(float contentPosition)
        {
            InternalReloadWithContentPosition(contentPosition);
        }

        /// <summary>
        /// Sets the normalized scroll position and reloads.
        /// </summary>
        /// <param name="normalizedScrollPosition">The normalized scroll position.</param>
        /// <remarks>The caller should ensure this is called once immediately after model data changes.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReloadWithNormalizedScrollPosition(double normalizedScrollPosition)
        {
            InternalReloadWithNormalizedScrollPosition(normalizedScrollPosition);
        }

        private void InternalReload()
        {
            InternalStopTween();
            ReturnAllItems();
            ReloadItemCountAndBeginEndPositions();
            RefreshContentSize();
            SetContentPosition(GetContentPosition());
            AddItems();
            RefreshScrollbarVisibility();
#if UNITY_EDITOR
            Dirty = true;
#endif
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
#if UNITY_EDITOR
            Dirty = true;
#endif
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
#if UNITY_EDITOR
            Dirty = true;
#endif
        }

        /// <summary>
        /// Recycles items out of range, adds items newly entering the range, and refreshes the scrollbar visibility.
        /// </summary>
        /// <remarks>This method is called when the <see cref="ScrollRect.onValueChanged"/> event is raised.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Refresh()
        {
            InternalRefresh();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InternalRefresh()
        {
            var activeContentBeginPosition = InternalConvertNormalizedViewportPositionToContentPosition(0) -
                                             (leadingActiveOffset >= 0 ? leadingActiveOffset : 0);
            var activeContentEndPosition = InternalConvertNormalizedViewportPositionToContentPosition(1) +
                                           (trailingActiveOffset >= 0 ? trailingActiveOffset : 0);
            var firstActiveIndex = InternalFindFirstIndex(activeContentBeginPosition);
            var lastActiveIndex  = InternalFindLastIndex(activeContentEndPosition);
            if (firstActiveIndex >= 0 && lastActiveIndex >= 0 && firstActiveIndex <= lastActiveIndex)
            {
                if (ReturnItemsBefore(firstActiveIndex) | ReturnItemsAfter(lastActiveIndex) |
                    AddItems(firstActiveIndex, lastActiveIndex))
                {
                    RefreshPlaceholders();
#if UNITY_EDITOR
                    Dirty = true;
#endif
                }
            }
            else
            {
                if (ReturnAllItems())
                {
                    RefreshPlaceholders();
#if UNITY_EDITOR
                    Dirty = true;
#endif
                }
            }
            RefreshScrollbarVisibility();
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
            else if (_controller.GetItemCount(this) is var itemCount && itemCount < 0)
            {
                Log.E($"{nameof(IScrollViewController)}.{nameof(IScrollViewController.GetItemCount)} returned < 0");

                _itemCount = 0;
                _itemPositions.Clear();
            }
            else
            {
                _itemCount = itemCount;
                _itemPositions.Clear();
                /*
                 * Although the stored value type is float, double is used during accumulation to reduce precision loss
                 * Do not underestimate the effect here: if the last item in the list is used as the base every time a new position is computed, when there are many items and spacing is dragged in the editor, noticeable jitter will occur
                 */
                double itemPosition = 0;
                for (var itemIndex = 0; itemIndex < _itemCount; itemIndex++)
                {
                    _itemPositions.Add((float)(itemPosition += itemIndex == 0 ? FirstPaddingAlongAxis : spacing));

                    float itemSize;
                    {
                        var itemSizeFromController = _controller.GetItemSize(this, itemIndex);
                        if (itemSizeFromController >= 0)
                        {
                            itemSize = itemSizeFromController;
                        }
                        else
                        {
                            // @formatter:max_line_length 10000
                            Log.W($"{nameof(IScrollViewController)}.{nameof(IScrollViewController.GetItemSize)} returned not >= 0");
                            // @formatter:max_line_length restore

                            itemSize = 0;
                        }
                    }
                    _itemPositions.Add((float)(itemPosition += itemSize));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RefreshContentSize()
        {
            content.sizeDelta = Set(
                content.sizeDelta,
                _itemCount == 0 ? PaddingAlongAxis : InternalGetItemEndPosition(_itemCount - 1) + LastPaddingAlongAxis
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddItems()
        {
            var activeContentBeginPosition = InternalConvertNormalizedViewportPositionToContentPosition(0) -
                                             (leadingActiveOffset >= 0 ? leadingActiveOffset : 0);
            var activeContentEndPosition = InternalConvertNormalizedViewportPositionToContentPosition(1) +
                                           (trailingActiveOffset >= 0 ? trailingActiveOffset : 0);
            var firstActiveIndex = InternalFindFirstIndex(activeContentBeginPosition);
            var lastActiveIndex  = InternalFindLastIndex(activeContentEndPosition);
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
                SetScrollRectScrollbar(scrollRect, null); // prevent ScrollRect from setting the ScrollBar active
            }
        }

        /// <summary>
        /// Stops the tween animation.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void StopTween()
        {
            if (IsActive())
            {
                DisableValueChangeCounter();
                DisableScrollTimer();
            }
            InternalStopTween();
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
#if UNITY_EDITOR
            Dirty = true;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsTweening()
        {
            return _tweenTokenSource != null;
        }

        /// <summary>
        /// Snaps.
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
            if (IsActive())
            {
                DisableValueChangeCounter();
                DisableScrollTimer();
            }
            InternalStopTween();
            if (IsActive())
            {
                BeginSnap();
            }
        }

        private async void BeginSnap()
        {
            Assert.IsFalse(IsTweening());

            var index = InternalFindClosestIndex(
                InternalConvertNormalizedViewportPositionToContentPosition(snapFindNormalizedViewportPosition)
            );
            if (index < 0)
            {
                return;
            }
            var overflowedContentSize = GetOverflowedContentSize();
            // If the "content size" is less than or equal to the "scroll view itself size", and the content is strictly or elastically restricted when it exceeds the scroll view, snapping is meaningless
            if (overflowedContentSize == 0 && scrollRect.movementType != ScrollRect.MovementType.Unrestricted)
            {
                return;
            }
            var contentBeginPosition = GetContentPosition();
            var itemBeginPosition    = InternalGetItemBeginPosition(index);
            var itemEndPosition      = InternalGetItemEndPosition(index);
            if (snapIncludingSpacing)
            {
                itemBeginPosition -= index == 0 ? FirstPaddingAlongAxis : spacing;
                itemEndPosition   += index == _itemCount - 1 ? LastPaddingAlongAxis : spacing;
            }
            // Clamp the content position value to ensure the content does not exceed the scroll view
            var contentEndPosition = Mathf.Clamp(
                (float)InterpolationUtility.LinearInterpolate(
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
                    Mathf.Abs((contentEndPosition - contentBeginPosition) / snapSpeed),
                _ => throw new ArgumentOutOfRangeException()
            };
            if (duration is not float.NaN and > 0 and not float.PositiveInfinity)
            {
                var       exitToken        = UnityEnvironment.ExitToken;
                var       disableToken     = gameObject.GetDisableToken();
                using var tweenTokenSource = CancellationTokenSource.CreateLinkedTokenSource(exitToken, disableToken);
                var       tweenToken       = tweenTokenSource.Token;
                _tweenTokenSource = tweenTokenSource;
                try
                {
                    if (scrollRect.movementType == ScrollRect.MovementType.Elastic &&
                        GetNormalizedScrollPosition() is < 0 or > 1)
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

                    await SnapAsync(contentBeginPosition, contentEndPosition, duration, snapInterpolation, tweenToken);

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
                SetContentPosition(contentEndPosition);
                scrollRect.velocity = Set(scrollRect.velocity, 0);
            }
        }

        private async Task SnapAsync(
            float             contentBeginPosition,
            float             contentEndPosition,
            float             duration,
            Interpolation     interpolation,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var beginTime = Time.timeAsDouble;
            var endTime   = beginTime + duration;
            do
            {
                await new DelayFrameAwaitable(1, PlayerLoopPhase.LateUpdating, cancellationToken);
                if (Time.deltaTime > 0)
                {
                    SetContentPosition(
                        (float)InterpolationUtility.Interpolate(
                            contentBeginPosition,
                            contentEndPosition,
                            Clamp01(
                                InterpolationUtility.InverseLinearInterpolate(beginTime, endTime, Time.timeAsDouble)
                            ),
                            interpolation
                        )
                    );
                }
            } while (Time.timeAsDouble < endTime);

            SetContentPosition(contentEndPosition);
            scrollRect.velocity = Set(scrollRect.velocity, 0);
            // Occupy the task until after ScrollRect.LateUpdate to prevent auto-snap from triggering again in OnScrollRectValueChange
            await new PlayerLoopPhaseAwaitable(PlayerLoopPhase.LateUpdated, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Clamp01(double value)
        {
            return value switch
            {
                < 0 => 0,
                > 1 => 1,
                _   => value
            };
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
            _scrollTimer.Change(TimeSpan.FromSeconds(scrollSnapDelay), Timeout.InfiniteTimeSpan);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DisableScrollTimer()
        {
            _scrollTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        /// <summary>
        /// Finds the index of the first item whose begin (content) position is greater than or equal to the specified content position.
        /// </summary>
        /// <param name="contentPosition">The content position.</param>
        /// <returns>The index of the first item whose begin (content) position is greater than or equal to the specified content position, if found; otherwise -1.</returns>
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
            // Exact match, but since there may be identical positions, continue to search for the first one
            if (matchPositionIndex >= 0)
            {
                // Search backward for an equal position
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
        /// Finds the index of the first item whose end (content) position is greater than or equal to the specified content position.
        /// </summary>
        /// <param name="contentPosition">The content position.</param>
        /// <returns>The index of the first item whose end (content) position is greater than or equal to the specified content position, if found; otherwise -1.</returns>
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
            // Exact match, but since there may be identical positions, continue to search for the last one
            if (matchPositionIndex >= 0)
            {
                // Search forward for an equal value
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
        /// Finds the index of the item closest to the specified content position.
        /// </summary>
        /// <param name="contentPosition">The content position.</param>
        /// <returns>The index of the item closest to <paramref name="contentPosition"/> if the item count is greater than 0; otherwise -1.</returns>
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
            // Exact match. Although there may be identical positions, here we neither bias toward the first nor the last, so use the current binary search result
            if (matchPositionIndex >= 0)
            {
                return matchPositionIndex / 2;
            }
            var insertPositionIndex = ~matchPositionIndex;
            // The insertion position is at the beginning, so the first item is the closest
            if (insertPositionIndex == 0)
            {
                return 0;
            }
            // The insertion position is at the end, so the last item is the closest
            if (insertPositionIndex == positionCount)
            {
                return positionCount / 2 - 1;
            }
            /*
             * In _itemPositions, "begin position" and "end position" are arranged in order.
             * All "begin position" indices are even, and all "end position" indices are odd.
             * If the insertion position is even, it lies between the end position of the previous item and the begin position of the next item; the result can be obtained by comparing the two distances;
             * If the insertion position is odd, it lies between the begin and end positions of some item, so that item is the result.
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
        /// Gets a recycled item, or instantiates <paramref name="itemPrefab"/> to create a new item.
        /// </summary>
        /// <param name="itemPrefab">The item prefab. If no recycled item with an <see cref="ScrollViewItem.identifier"/> equal to that of <paramref name="itemPrefab"/> is found, this prefab is instantiated to create a new item.</param>
        /// <param name="isNewCreated">Whether the item is newly created.</param>
        /// <returns>The item obtained from recycled items or newly created.</returns>
        /// <remarks>This method is called only by the implementation of <see cref="IScrollViewController.GetItem"/>.</remarks>
        public ScrollViewItem GetRecycledOrCreateNewItem(ScrollViewItem itemPrefab, out bool isNewCreated)
        {
            if (!itemPrefab)
            {
                throw new ArgumentNullException(nameof(itemPrefab));
            }

            if (_recycledItems.FindLastIndex(AreIdentifierEqual, itemPrefab) is var index and >= 0)
            {
                var item = _recycledItems[index];
                _recycledItems.RemoveAt(index);

                isNewCreated = false;
                return item;
            }
            {
#if UNITY_EDITOR
                var itemPrefabName = itemPrefab.name;
#endif
                var item = Instantiate(itemPrefab, inactiveContainer, false); // ensure it is inactive
#if UNITY_EDITOR
                item.name = itemPrefabName;
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
                InternalGetItemEndPosition(index) - InternalGetItemBeginPosition(index)
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
            item.OnGet(isNewCreated);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddLastItem(int index)
        {
            var item = GetItem(index, out var isNewCreated);
            item.transform.SetSiblingIndex(item.transform.parent.childCount - 2); // before _trailingPlaceholder
            _activeItems.EnqueueLast(item);
            item.gameObject.SetActive(true);
            item.OnGet(isNewCreated);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReturnItem(ScrollViewItem item, bool isBeingDestroyed)
        {
            if (item.visible)
            {
                item.visible = false;
                item.OnInvisible();
            }
            item.OnReturn(isBeingDestroyed);
            item.index = -1;
            if (isBeingDestroyed)
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

#if UNITY_EDITOR
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
#endif

        /// <summary>
        /// Converts a content position to a normalized viewport position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ConvertContentPositionToNormalizedViewportPosition(float contentPosition)
        {
            return InternalConvertContentPositionToNormalizedViewportPosition(contentPosition);
        }

        /// <summary>
        /// Converts a normalized viewport position to a content position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ConvertNormalizedViewportPositionToContentPosition(float normalizedViewportPosition)
        {
            return InternalConvertNormalizedViewportPositionToContentPosition(normalizedViewportPosition);
        }

        /// <summary>
        /// Converts a content position to a normalized scroll position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ConvertContentPositionToNormalizedScrollPosition(float contentPosition)
        {
            return InternalConvertContentPositionToNormalizedScrollPosition(contentPosition);
        }

        /// <summary>
        /// Converts a normalized scroll position to a content position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ConvertNormalizedScrollPositionToContentPosition(double normalizedScrollPosition)
        {
            return InternalConvertNormalizedScrollPositionToContentPosition(normalizedScrollPosition);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float InternalConvertContentPositionToNormalizedViewportPosition(float contentPosition)
        {
            var contentBeginPosition = GetContentPosition();
            var contentEndPosition   = contentBeginPosition + GetViewportSize();
            return (float)InterpolationUtility.InverseLinearInterpolate(
                contentBeginPosition,
                contentEndPosition,
                contentPosition
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float InternalConvertNormalizedViewportPositionToContentPosition(float normalizedViewportPosition)
        {
            var contentBeginPosition = GetContentPosition();
            var contentEndPosition   = contentBeginPosition + GetViewportSize();
            return (float)InterpolationUtility.LinearInterpolate(
                contentBeginPosition,
                contentEndPosition,
                normalizedViewportPosition
            );
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
            return (float)(overflowedContentSize == 0 ? 0 : overflowedContentSize * normalizedScrollPosition);
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

            if ((snapTrigger & ScrollViewSnapTrigger.OnPointerUpWithLowSpeed) != 0 && !IsTweening() &&
                Mathf.Abs(Get(scrollRect.velocity)) < 1)
            {
                DisableValueChangeCounter();
                DisableScrollTimer();
                BeginSnap();
            }
        }

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _dragging = true;
#if UNITY_EDITOR
            Dirty = true;
#endif

            InternalStopTween();
            DisableValueChangeCounter();
            DisableScrollTimer();
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            // if (!_dragging || eventData.button != PointerEventData.InputButton.Left)
            // {
            //     return;
            // }

            /*
             * There is no logic here yet
             * However, to make OnBeginDrag and OnEndDrag execute, OnDrag must be implemented
             */
        }

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            _dragging = false;
#if UNITY_EDITOR
            Dirty = true;
#endif

            if ((snapTrigger & ScrollViewSnapTrigger.OnEndDrag) != 0 && !IsTweening())
            {
                DisableValueChangeCounter();
                DisableScrollTimer();
                BeginSnap();
            }
        }

        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            if (!eventData.IsScrolling())
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
                LimitSpeed();
            }
            else if (playerLoopPhase == PlayerLoopPhase.LateUpdated)
            {
                RefreshItemsVisibleState();
                _scrolling = false;
#if UNITY_EDITOR
                var contentPosition          = GetContentPosition();
                var normalizedScrollPosition = GetNormalizedScrollPosition();
                if (!_contentPosition.Equals(contentPosition) ||
                    !_normalizedScrollPosition.Equals(normalizedScrollPosition))
                {
                    _contentPosition          = contentPosition;
                    _normalizedScrollPosition = normalizedScrollPosition;
                    Dirty                     = true;
                }
#endif
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void LimitSpeed()
        {
            if (speedLimit > 0 && Get(scrollRect.velocity) is var velocity && Mathf.Abs(velocity) is var speed &&
                speed > speedLimit)
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

            var contentBeginPosition = InternalConvertNormalizedViewportPositionToContentPosition(0);
            var contentEndPosition   = InternalConvertNormalizedViewportPositionToContentPosition(1);
            var firstVisibleIndex    = InternalFindFirstIndex(contentBeginPosition);
            var lastVisibleIndex     = InternalFindLastIndex(contentEndPosition);
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
                            item.OnVisible();
                        }
                    }
                    else
                    {
                        if (item.visible)
                        {
                            item.visible = false;
                            item.OnInvisible();
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
                        item.OnInvisible();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnValueChangeCounterTriggered()
        {
            Assert.IsFalse(IsTweening());

            if (!_dragging && (snapTrigger & ScrollViewSnapTrigger.OnNormalizedScrollPositionChanged) != 0 &&
                snapSpeedThreshold > 0 && Mathf.Abs(Get(scrollRect.velocity)) is var speed)
            {
                if (speed < snapSpeedThreshold)
                {
                    BeginSnap();
                }
                else
                {
                    // The condition is not satisfied; re-enable and continue checking next frame
                    EnableValueChangeCounter();
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnScrollTimerTriggered()
        {
            Assert.IsFalse(IsTweening());

            if (!_dragging && (snapTrigger & ScrollViewSnapTrigger.OnNormalizedScrollPositionChanged) != 0 &&
                snapSpeedThreshold > 0 && Mathf.Abs(Get(scrollRect.velocity)) is var speed &&
                speed < snapSpeedThreshold)
            {
                BeginSnap();
            }
        }

        private void OnScrollRectValueChanged(Vector2 value)
        {
            if (!_dragging && !_scrolling && !IsTweening())
            {
                // Because the scrollbar was operated, this callback is executed
                if (scrollbar && scrollbar.IsActive() && SelectableUtility.IsPressed(scrollbar))
                {
                    DisableValueChangeCounter();
                    EnableScrollTimer();
                }
                else if ((snapTrigger & ScrollViewSnapTrigger.OnNormalizedScrollPositionChanged) != 0 &&
                         snapSpeedThreshold > 0 && Mathf.Abs(Get(scrollRect.velocity)) is var speed)
                {
                    /*
                     * Because ScrollRect.onValueChanged triggering is unreliable, another way is needed to detect that the velocity is below the threshold
                     * Currently the velocity is not below the threshold, so enable the counter (even if already enabled, it is fine; repeated enabling only takes effect the last time); the counter callback will check whether the velocity is below the threshold
                     */
                    if (speed >= snapSpeedThreshold)
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
            // Ensure that ScrollViewItem.OnInvisible and ScrollViewItem.OnReturn execute in pairs with ScrollViewItem.OnVisible and ScrollViewItem.OnGet respectively
            while (_activeItems.TryDequeueLast(out var item))
            {
                ReturnItem(item, true);
            }

            base.OnDestroy();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Avoid being reformatted because a single line is too long; keep it aligned with OnDisable
            // @formatter:max_line_length 10000
            PlayerLoopUtility.AddPlayerLoopItem(this, PlayerLoopPhase.LateUpdating);
            PlayerLoopUtility.AddPlayerLoopItem(this, PlayerLoopPhase.LateUpdated);
            scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
            _valueChangeCounter = new UnityFrameCountPlayerLoopCounter(OnValueChangeCounterTriggerCallback, this, PlayerLoopPhase.LateUpdating);
            _scrollTimer = new UnityUnscaledTimePlayerLoopTimer(OnScrollTimerTriggerCallback, this, PlayerLoopPhase.LateUpdating);
            // @formatter:max_line_length restore
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
