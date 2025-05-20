using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Collections;
using Aurora.Diagnostics;
using Aurora.Pooling;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace Aurora.Unity.UI.ViewSystem
{
    /// <summary>
    /// 界面。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public abstract class View : UIBehaviour, IEnumerable<View>
    {
        private static readonly List<ViewContainer> Containers = new List<ViewContainer>();

        private static readonly Func<View, IEnumerable<View>> FuncGetChildren = GetChildrenAsEnumerable;

        /// <summary>
        /// 获取界面容器的数量。
        /// </summary>
        public static int ContainerCount => Containers.Count;

        /// <summary>
        /// 添加界面容器。
        /// </summary>
        /// <param name="rectTransform">一个矩形变换，它将作为界面的容器。</param>
        public static void AddContainer(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            var index = Containers.FindIndex(Match, rectTransform);
            if (index >= 0)
            {
                throw new ArgumentException();
            }
            Containers.Add(new ViewContainer(rectTransform));

            static bool Match(ViewContainer container, RectTransform rectTransform)
            {
                return ReferenceEquals(container.RectTransform, rectTransform);
            }
        }

        /// <summary>
        /// 移除位于指定索引处的界面容器。
        /// </summary>
        /// <param name="index">界面容器的索引。</param>
        /// <exception cref="InvalidOperationException">要移除的界面容器下界面的数量不为 0。</exception>
        public static void RemoveContainerAt(int index)
        {
            var container = Containers[index];
            if (container.Views.Count != 0)
            {
                throw new InvalidOperationException("无法移除此界面容器，因为它仍然容纳着界面");
            }
        }

        /// <summary>
        /// 获取位于指定索引处的界面容器。
        /// </summary>
        /// <param name="index">界面容器的索引。</param>
        public static ViewContainer GetContainer(int index)
        {
            return Containers[index];
        }

#if UNITY_EDITOR
        internal static void ClearContainers()
        {
            Containers.Clear();
        }
#endif

        private static IEnumerable<View> GetChildrenAsEnumerable(View view)
        {
            return view._children;
        }

        /// <summary>
        /// 判断指定的界面是否是最上层界面。
        /// </summary>
        /// <param name="view">界面。</param>
        /// <returns>如果 <paramref name="view"/> 是最上层界面，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="view"/> 为 <see langword="null"/>。</exception>
        public static bool IsTopmost(View view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }
            return InternalIsTopmost(view);
        }

        /// <summary>
        /// 判断指定的界面是否是最上层界面。
        /// </summary>
        /// <param name="view">界面。</param>
        /// <param name="exclusions">包含要排除的界面或者界面的类型的数组。</param>
        /// <returns>如果 <paramref name="view"/> 是最上层界面，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="view"/> 为 <see langword="null"/>，或者 <paramref name="exclusions"/> 含有 <see langword="null"/> 元素。</exception>
        /// <exception cref="ArgumentException"><paramref name="exclusions"/> 含有 <paramref name="view"/>，或含有既不是 <see cref="View"/> 类型又不是 <see cref="Type"/> 类型的元素。</exception>
        public static bool IsTopmost(View view, object[] exclusions)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }
            return exclusions == null || exclusions.Length == 0
                       ? InternalIsTopmost(view)
                       : InternalIsTopmost(view, exclusions);
        }

        private static bool InternalIsTopmost(Object view)
        {
            var topmostView = InternalGetTopmostView();
            return ReferenceEquals(view, topmostView);
        }

        private static bool InternalIsTopmost(Object view, object[] exclusions)
        {
            var views = PredefinedPools<View>.List.Get();
            try
            {
                InternalGetViews(TreeEnumOrder.DepthFirstRld, views);
                foreach (var exclusion in exclusions)
                {
                    switch (exclusion)
                    {
                        case null:
                            throw new ArgumentNullException(nameof(exclusions));
                        case View excludedView:
                            if (ReferenceEquals(excludedView, view))
                            {
                                throw new ArgumentException(null, nameof(exclusions));
                            }
                            views.Remove(excludedView);
                            break;
                        case Type excludedViewType:
                            views.RemoveAll(Match);
                            break;

                            bool Match(View e)
                            {
                                return !ReferenceEquals(e, view) && excludedViewType.IsInstanceOfType(e);
                            }
                        default:
                            throw new ArgumentException(null, nameof(exclusions));
                    }
                }
                return views.Count > 0 && ReferenceEquals(views[0], view);
            }
            finally
            {
                PredefinedPools<View>.List.Return(views);
            }
        }

        /// <summary>
        /// 获取最上层界面。
        /// </summary>
        /// <returns>最上层界面。</returns>
        public static View GetTopmostView()
        {
            return InternalGetTopmostView();
        }

        /// <summary>
        /// 获取最上层界面。
        /// </summary>
        /// <param name="exclusions">包含要排除的界面或者界面的类型的数组。</param>
        /// <returns>最上层界面。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exclusions"/> 含有 <see langword="null"/> 元素。</exception>
        /// <exception cref="ArgumentException"><paramref name="exclusions"/> 含有既不是 <see cref="View"/> 类型又不是 <see cref="Type"/> 类型的元素。</exception>
        public static View GetTopmostView(object[] exclusions)
        {
            return exclusions == null || exclusions.Length == 0
                       ? InternalGetTopmostView()
                       : InternalGetTopmostView(exclusions);
        }

        private static View InternalGetTopmostView()
        {
            // return GetViewWithoutRoot<View>(TreeEnumOrder.DepthFirstRld);
            // 下面的实现消耗更少的内存和性能

            var containersCount = Containers.Count;
            if (containersCount == 0)
            {
                return null;
            }
            var container = Containers[containersCount - 1];
            var views     = container.Views;
            var viewCount = views.Count;
            if (viewCount == 0)
            {
                return null;
            }
            var topmostView = views[viewCount - 1];
            for (var childCount = topmostView._children.Count; childCount > 0; childCount = topmostView._children.Count)
            {
                topmostView = topmostView._children[childCount - 1];
            }
            return topmostView;
        }

        private static View InternalGetTopmostView(object[] exclusions)
        {
            var views = PredefinedPools<View>.List.Get();
            try
            {
                InternalGetViews(TreeEnumOrder.DepthFirstRld, views);
                foreach (var exclusion in exclusions)
                {
                    switch (exclusion)
                    {
                        case null:
                            throw new ArgumentNullException(nameof(exclusions));
                        case View excludedView:
                            views.Remove(excludedView);
                            break;
                        case Type excludedViewType:
                            views.RemoveAll(Match);
                            break;

                            bool Match(View e)
                            {
                                return excludedViewType.IsInstanceOfType(e);
                            }
                        default:
                            throw new ArgumentException(null, nameof(exclusions));
                    }
                }
                return views.Count > 0 ? views[0] : null;
            }
            finally
            {
                PredefinedPools<View>.List.Return(views);
            }
        }

        /// <summary>
        /// 按照 <see cref="TreeEnumOrder.DepthFirstRld"/> 枚举顺序枚举所有界面，并返回第一个满足指定类型的界面。
        /// </summary>
        public static T GetView<T>() where T : class
        {
            return InternalGetView<T>(TreeEnumOrder.DepthFirstRld);
        }

        /// <summary>
        /// 按照指定的枚举顺序枚举所有界面，并返回第一个满足指定类型的界面。
        /// </summary>
        public static T GetView<T>(TreeEnumOrder order) where T : class
        {
            return InternalGetView<T>(order);
        }

        private static T InternalGetView<T>(TreeEnumOrder order) where T : class
        {
            switch (order)
            {
                case TreeEnumOrder.Default:
                {
                    foreach (var container in Containers)
                    {
                        if (container.GetViewFromContainer<T>(order) is { } t)
                        {
                            return t;
                        }
                    }
                    return null;
                }
                case TreeEnumOrder.BreadthFirstLr:
                {
                    var queue = PredefinedPools<View>.Queue.Get();
                    try
                    {
                        foreach (var container in Containers)
                        {
                            var views = container.Views;
                            foreach (var view in views)
                            {
                                if (view is T t)
                                {
                                    return t;
                                }
                                queue.Enqueue(view);
                            }
                        }
                        while (queue.Count > 0)
                        {
                            var view = queue.Dequeue();
                            foreach (var child in view._children)
                            {
                                if (child is T t)
                                {
                                    return t;
                                }
                                queue.Enqueue(child);
                            }
                        }
                        return null;
                    }
                    finally
                    {
                        PredefinedPools<View>.Queue.Return(queue);
                    }
                }
                case TreeEnumOrder.BreadthFirstRl:
                {
                    var queue = PredefinedPools<View>.Queue.Get();
                    try
                    {
                        for (var i = Containers.Count - 1; i >= 0; i--)
                        {
                            var container = Containers[i];
                            var views     = container.Views;
                            for (var j = views.Count - 1; j >= 0; j--)
                            {
                                var view = container.Views[j];
                                if (view is T t)
                                {
                                    return t;
                                }
                                queue.Enqueue(view);
                            }
                        }
                        while (queue.Count > 0)
                        {
                            var view = queue.Dequeue();
                            for (var i = view._children.Count - 1; i >= 0; i--)
                            {
                                var child = view._children[i];
                                if (child is T t)
                                {
                                    return t;
                                }
                                queue.Enqueue(child);
                            }
                        }
                        return null;
                    }
                    finally
                    {
                        PredefinedPools<View>.Queue.Return(queue);
                    }
                }
                case TreeEnumOrder.DepthFirstDlr:
                case TreeEnumOrder.DepthFirstLrd:
                {
                    foreach (var container in Containers)
                    {
                        if (container.GetViewFromContainer<T>(order) is { } t)
                        {
                            return t;
                        }
                    }
                    return null;
                }
                case TreeEnumOrder.DepthFirstDrl:
                case TreeEnumOrder.DepthFirstRld:
                {
                    for (var i = Containers.Count - 1; i >= 0; i--)
                    {
                        var container = Containers[i];
                        if (container.GetViewFromContainer<T>(order) is { } t)
                        {
                            return t;
                        }
                    }
                    return null;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(order), order, null);
            }
        }

        /// <summary>
        /// 按照指定的枚举顺序枚举所有界面，并将指定类型的界面存入指定的集合。
        /// </summary>
        public static void GetViews<T>(TreeEnumOrder order, ICollection<T> results) where T : class
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }
            if (results.IsReadOnly)
            {
                throw new ArgumentException();
            }
            InternalGetViews(order, results);
        }

        private static void InternalGetViews<T>(TreeEnumOrder order, ICollection<T> results) where T : class
        {
            switch (order)
            {
                case TreeEnumOrder.Default:
                {
                    foreach (var container in Containers)
                    {
                        if (container.GetViewFromContainer<T>(order) is { } t)
                        {
                            results.Add(t);
                        }
                    }
                    break;
                }
                case TreeEnumOrder.BreadthFirstLr:
                {
                    var queue = PredefinedPools<View>.Queue.Get();
                    try
                    {
                        foreach (var container in Containers)
                        {
                            var views = container.Views;
                            foreach (var view in views)
                            {
                                if (view is T t)
                                {
                                    results.Add(t);
                                }
                                queue.Enqueue(view);
                            }
                        }
                        while (queue.Count > 0)
                        {
                            var view = queue.Dequeue();
                            foreach (var child in view._children)
                            {
                                if (child is T t)
                                {
                                    results.Add(t);
                                }
                                queue.Enqueue(child);
                            }
                        }
                    }
                    finally
                    {
                        PredefinedPools<View>.Queue.Return(queue);
                    }
                    break;
                }
                case TreeEnumOrder.BreadthFirstRl:
                {
                    var queue = PredefinedPools<View>.Queue.Get();
                    try
                    {
                        for (var i = Containers.Count - 1; i >= 0; i--)
                        {
                            var container = Containers[i];
                            var views     = container.Views;
                            for (var j = views.Count - 1; j >= 0; j--)
                            {
                                var view = container.Views[j];
                                if (view is T t)
                                {
                                    results.Add(t);
                                }
                                queue.Enqueue(view);
                            }
                        }
                        while (queue.Count > 0)
                        {
                            var view = queue.Dequeue();
                            for (var i = view._children.Count - 1; i >= 0; i--)
                            {
                                var child = view._children[i];
                                if (child is T t)
                                {
                                    results.Add(t);
                                }
                                queue.Enqueue(child);
                            }
                        }
                    }
                    finally
                    {
                        PredefinedPools<View>.Queue.Return(queue);
                    }
                    break;
                }
                case TreeEnumOrder.DepthFirstDlr:
                case TreeEnumOrder.DepthFirstLrd:
                {
                    foreach (var container in Containers)
                    {
                        container.InternalGetViewsFromContainer(order, results);
                    }
                    break;
                }
                case TreeEnumOrder.DepthFirstDrl:
                case TreeEnumOrder.DepthFirstRld:
                {
                    for (var i = Containers.Count - 1; i >= 0; i--)
                    {
                        var container = Containers[i];
                        container.InternalGetViewsFromContainer(order, results);
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(order), order, null);
            }
        }

        private static async Task<T> CreateInactiveOrDisabledViewAsync<T>(CancellationToken cancellationToken)
            where T : View
        {
            cancellationToken.ThrowIfCancellationRequested();
            var handler = ViewHandler.Get<T>();
            if (handler == null)
            {
                throw new InvalidOperationException(
                    $"无法获取适合于处理 {TypeUtility.GetNicelyFormattedName(typeof(T))} 类型的界面处理程序"
                );
            }
            var view = await handler.CreateInactiveOrDisabledViewAsync<T>(cancellationToken);
            view._handler = handler;
            return view;
        }

        /// <summary>
        /// 开始异步打开根界面。
        /// </summary>
        public static async void BeginOpen<T>(ViewContainer container) where T : View
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            _ = await InternalOpenAsync<T>(container, null, CancellationToken.None);
        }

        /// <summary>
        /// 开始异步打开根界面。
        /// </summary>
        public static async void BeginOpen<T>(ViewContainer container, CancellationToken cancellationToken)
            where T : View
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            cancellationToken.ThrowIfCancellationRequested();
            _ = await InternalOpenAsync<T>(container, null, cancellationToken);
        }

        /// <summary>
        /// 开始异步打开根界面。
        /// </summary>
        public static async void BeginOpen<T>(ViewContainer container, object state) where T : View
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            _ = await InternalOpenAsync<T>(container, state, CancellationToken.None);
        }

        /// <summary>
        /// 开始异步打开根界面。
        /// </summary>
        public static async void BeginOpen<T>(
            ViewContainer     container,
            object            state,
            CancellationToken cancellationToken) where T : View
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            cancellationToken.ThrowIfCancellationRequested();
            _ = await InternalOpenAsync<T>(container, state, cancellationToken);
        }

        /// <summary>
        /// 开始异步打开界面。
        /// </summary>
        public static async void BeginOpen<T>(View parent) where T : View
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }
            _ = await InternalOpenAsync<T>(parent, null, CancellationToken.None);
        }

        /// <summary>
        /// 开始异步打开界面。
        /// </summary>
        public static async void BeginOpen<T>(View parent, CancellationToken cancellationToken) where T : View
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }
            cancellationToken.ThrowIfCancellationRequested();
            _ = await InternalOpenAsync<T>(parent, null, cancellationToken);
        }

        /// <summary>
        /// 开始异步打开界面。
        /// </summary>
        public static async void BeginOpen<T>(View parent, object state) where T : View
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }
            _ = await InternalOpenAsync<T>(parent, state, CancellationToken.None);
        }

        /// <summary>
        /// 开始异步打开界面。
        /// </summary>
        public static async void BeginOpen<T>(View parent, object state, CancellationToken cancellationToken)
            where T : View
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }
            cancellationToken.ThrowIfCancellationRequested();
            _ = await InternalOpenAsync<T>(parent, state, cancellationToken);
        }

        /// <summary>
        /// 异步打开根界面。
        /// </summary>
        public static Task<T> OpenAsync<T>(ViewContainer container) where T : View
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            return InternalOpenAsync<T>(container, null, CancellationToken.None);
        }

        /// <summary>
        /// 异步打开根界面。
        /// </summary>
        public static Task<T> OpenAsync<T>(ViewContainer container, CancellationToken cancellationToken) where T : View
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            return !cancellationToken.IsCancellationRequested
                       ? InternalOpenAsync<T>(container, null, cancellationToken)
                       : Task.FromCanceled<T>(cancellationToken);
        }

        /// <summary>
        /// 异步打开根界面。
        /// </summary>
        public static Task<T> OpenAsync<T>(ViewContainer container, object state) where T : View
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            return InternalOpenAsync<T>(container, state, CancellationToken.None);
        }

        /// <summary>
        /// 异步打开根界面。
        /// </summary>
        public static Task<T> OpenAsync<T>(ViewContainer container, object state, CancellationToken cancellationToken)
            where T : View
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            return !cancellationToken.IsCancellationRequested
                       ? InternalOpenAsync<T>(container, state, cancellationToken)
                       : Task.FromCanceled<T>(cancellationToken);
        }

        private static async Task<T> InternalOpenAsync<T>(
            ViewContainer     container,
            object            state,
            CancellationToken cancellationToken) where T : View
        {
            var view = await CreateInactiveOrDisabledViewAsync<T>(cancellationToken);
            if (view == null)
            {
                throw new InvalidOperationException(
                    $"最适合于处理 {TypeUtility.GetNicelyFormattedName(typeof(T))} 类型界面的界面处理程序创建出的界面为 null"
                );
            }
            if (view.gameObject.activeSelf && view.enabled)
            {
                throw new BehaviourActiveAndEnabledException(
                    view,
                    $"最适合于处理 {TypeUtility.GetNicelyFormattedName(typeof(T))} 类型界面的界面处理程序创建出的界面所关联的游戏物体处于激活状态并且界面启用"
                );
            }

            view._container = container;
            container.Views.Add(view);

            var viewTransform = view.RectTransform;
            viewTransform.SetParent(container.RectTransform, false);
            RectTransformUtility.AlignToParentEdges(viewTransform);

            view.State = state;

            view.OnSettingActiveAndEnabling();
            if (!view.gameObject.activeSelf)
            {
                view.gameObject.SetActive(true);
            }
            if (!view.enabled)
            {
                view.enabled = true;
            }

            return view;
        }

        /// <summary>
        /// 异步打开界面。
        /// </summary>
        public static Task<T> OpenAsync<T>(View parent) where T : View
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }
            return InternalOpenAsync<T>(parent, null, CancellationToken.None);
        }

        /// <summary>
        /// 异步打开界面。
        /// </summary>
        public static Task<T> OpenAsync<T>(View parent, CancellationToken cancellationToken) where T : View
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }
            return !cancellationToken.IsCancellationRequested
                       ? InternalOpenAsync<T>(parent, null, cancellationToken)
                       : Task.FromCanceled<T>(cancellationToken);
        }

        /// <summary>
        /// 异步打开界面。
        /// </summary>
        public static Task<T> OpenAsync<T>(View parent, object state) where T : View
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }
            return InternalOpenAsync<T>(parent, state, CancellationToken.None);
        }

        /// <summary>
        /// 异步打开界面。
        /// </summary>
        public static Task<T> OpenAsync<T>(View parent, object state, CancellationToken cancellationToken)
            where T : View
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }
            return !cancellationToken.IsCancellationRequested
                       ? InternalOpenAsync<T>(parent, state, cancellationToken)
                       : Task.FromCanceled<T>(cancellationToken);
        }

        private static async Task<T> InternalOpenAsync<T>(
            View              parent,
            object            state,
            CancellationToken cancellationToken) where T : View
        {
            var view = await CreateInactiveOrDisabledViewAsync<T>(cancellationToken);
            if (view == null)
            {
                throw new InvalidOperationException(
                    $"最适合于处理 {TypeUtility.GetNicelyFormattedName(typeof(T))} 类型界面的界面处理程序创建出的界面为 null"
                );
            }
            if (view.gameObject.activeSelf && view.enabled)
            {
                throw new BehaviourActiveAndEnabledException(
                    view,
                    $"最适合于处理 {TypeUtility.GetNicelyFormattedName(typeof(T))} 类型界面的界面处理程序创建出的界面所关联的游戏物体处于激活状态并且界面启用"
                );
            }

            view._container = parent._container;
            view._parent    = parent;
            parent._children.Add(view);

            var viewTransform = view.RectTransform;
            viewTransform.SetParent(parent.ChildContainerOrRectTransformIfNull, false);
            RectTransformUtility.AlignToParentEdges(viewTransform);

            view.State = state;

            view.OnSettingActiveAndEnabling();
            if (!view.gameObject.activeSelf)
            {
                view.gameObject.SetActive(true);
            }
            if (!view.enabled)
            {
                view.enabled = true;
            }

            return view;
        }

        private RectTransform _rectTransform;

        private ViewHandler _handler;

        private ViewContainer _container;

        private View _parent;

        /// <summary>
        /// 子界面的容器。
        /// </summary>
        public RectTransform childContainer;

        private readonly List<View> _children = new List<View>();

        /// <summary>
        /// 获取与此实例关联的矩形变换。
        /// </summary>
        public RectTransform RectTransform =>
            _rectTransform ? _rectTransform : _rectTransform = (RectTransform) transform;

        /// <summary>
        /// 由用户定义的数据。将在打开界面时赋值。
        /// </summary>
        public object State { get; private set; }

        /// <summary>
        /// 由用户定义的数据。将在关闭界面时赋值。
        /// </summary>
        public object CloseState { get; private set; }

        private RectTransform ChildContainerOrRectTransformIfNull => childContainer ?? RectTransform;

        /// <summary>
        /// 获取或设置容器。
        /// </summary>
        /// <remarks>不能设置为 <see langword="null"/>。</remarks>
        public ViewContainer Container
        {
            get => _container;
            set
            {
                if (ReferenceEquals(_container, value))
                {
                    return;
                }
                if (value is null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                if (_parent is null)
                {
                    _container.Views.Remove(this);
                }
                else
                {
                    _parent._children.Remove(this);
                    _parent = null;
                }
                _container = value;
                using (var enumerator = InternalGetEnumerator(TreeEnumOrder.DepthFirstDlr))
                {
                    while (enumerator.MoveNext())
                    {
                        enumerator.Current!._container = _container;
                    }
                }
                _container.Views.Add(this);
                RectTransform.SetParent(_container.RectTransform, false);
                RectTransform.SetAsLastSibling();
                RectTransformUtility.AlignToParentEdges(RectTransform);
            }
        }

        /// <summary>
        /// 获取或设置父界面。
        /// </summary>
        public View Parent
        {
            get => _parent;
            set
            {
                if (ReferenceEquals(_parent, value))
                {
                    return;
                }
                if (value is null)
                {
                    _parent._children.Remove(this);
                    _parent = null;
                    _container.Views.Add(this);
                    RectTransform.SetParent(_container.RectTransform, false);
                }
                else
                {
                    if (ReferenceEquals(value, this))
                    {
                        throw new ArgumentException("不能设置自己为自己的父界面", nameof(value));
                    }
                    if (value.InternalIsChildOf(this))
                    {
                        throw new ArgumentException("不能设置子界面为父界面", nameof(value));
                    }
                    if (_parent is null)
                    {
                        _container.Views.Remove(this);
                    }
                    else
                    {
                        _parent._children.Remove(this);
                    }
                    var containerChanged = ReferenceEquals(_container, value._container);
                    _container = value._container;
                    _parent    = value;
                    if (containerChanged)
                    {
                        using var enumerator = InternalGetEnumerator(TreeEnumOrder.DepthFirstDlr);
                        while (enumerator.MoveNext())
                        {
                            enumerator.Current!._container = _container;
                        }
                    }
                    RectTransform.SetParent(_parent.ChildContainerOrRectTransformIfNull, false);
                }
                RectTransform.SetAsLastSibling();
                RectTransformUtility.AlignToParentEdges(RectTransform);
            }
        }

        /// <summary>
        /// 获取根界面。
        /// </summary>
        public View Root
        {
            get
            {
                var root = this;
                for (var parent = _parent; parent != null; parent = parent._parent)
                {
                    root = parent;
                }
                return root;
            }
        }

        /// <summary>
        /// 获取一个值，这个值指示这个界面是否是根界面。
        /// </summary>
        public bool IsRoot => _parent == null;

        /// <summary>
        /// 获取一个值，这个值指示这个界面是否是叶子界面。
        /// </summary>
        public bool IsLeaf => _children.Count == 0;

        /// <summary>
        /// 获取一个值，这个值指示这个界面是否是最上层界面。
        /// </summary>
        /// <returns>如果这个界面是否是最上层界面，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <remarks>此方法是虚方法，若子类未重写，实际执行 <see cref="IsTopmost(View)"/>。</remarks>
        public virtual bool IsTopmost()
        {
            return InternalIsTopmost(this);
        }

        /// <summary>
        /// 在打开界面的最后一步“激活此界面所关联的游戏物体，并启用此界面”前调用此方法。
        /// </summary>
        protected virtual void OnSettingActiveAndEnabling()
        {
        }

        /// <summary>
        /// 获取一个值，这个值指示这个界面是否是指定界面的子界面。
        /// </summary>
        /// <param name="view">指定的界面。</param>
        /// <returns>这个界面是否是指定界面的子界面。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="view"/> 为 <see langword="null"/>。</exception>
        public bool IsChildOf(View view)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }
            return InternalIsChildOf(view);
        }

        private bool InternalIsChildOf(Object view)
        {
            if (ReferenceEquals(view, this))
            {
                return false;
            }
            for (var parent = _parent; parent != null; parent = parent._parent)
            {
                if (view == parent)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取指定类型的子界面。
        /// </summary>
        /// <typeparam name="T">界面的类型。</typeparam>
        /// <returns>如果存在指定类型的子界面，则返回第一个匹配项；否则为 <see langword="null"/>。</returns>
        public T GetChild<T>() where T : View
        {
            foreach (var child in _children)
            {
                if (child is T t)
                {
                    return t;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取指定类型的子界面，并将结果存入指定的列表。
        /// </summary>
        /// <param name="results">存放结果的列表。</param>
        /// <typeparam name="T">界面的类型。</typeparam>
        public void GetChildren<T>(List<T> results)
        {
            if (results is null)
            {
                throw new ArgumentNullException(nameof(results));
            }
            foreach (var child in _children)
            {
                if (child is T t)
                {
                    results.Add(t);
                }
            }
        }

        /// <summary>
        /// 按照指定的枚举顺序枚举此界面，并返回第一个满足指定类型的界面。
        /// </summary>
        public T GetViewFromThis<T>(TreeEnumOrder order) where T : class
        {
            using var enumerator = InternalGetEnumerator(order);
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is T t)
                {
                    return t;
                }
            }
            return null;
        }

        /// <summary>
        /// 按照指定的枚举顺序枚举此界面，并将指定类型的界面存入指定的集合。
        /// </summary>
        public void GetViewsFromThis<T>(TreeEnumOrder order, ICollection<T> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }
            if (results.IsReadOnly)
            {
                throw new ArgumentException();
            }
            InternalGetViewsFromThis(order, results);
        }

        private void InternalGetViewsFromThis<T>(TreeEnumOrder order, ICollection<T> results)
        {
            using var enumerator = InternalGetEnumerator(order);
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is T t)
                {
                    results.Add(t);
                }
            }
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="closeState">由用户定义的数据，将赋值给 <see cref="View.CloseState"/>。</param>
        public void Close(object closeState = null)
        {
            InternalClose(closeState);
        }

        private void InternalClose(object closeState)
        {
            for (var i = _children.Count - 1; i >= 0; i--)
            {
                var child = _children[i];
                _children.RemoveAt(i);
                child.InternalClose(null);
            }

            if (_parent is null)
            {
                _container.Views.Remove(this);
                _container = null;
            }
            else
            {
                _container = null;
                _parent._children.Remove(this);
                _parent = null;
            }

            CloseState = closeState;
            if (_handler != null)
            {
                try
                {
                    _handler.ReleaseView(this);
                }
                catch (Exception e)
                {
                    Log.E(e);
                    if (this != null)
                    {
                        Destroy(gameObject);
                    }
                }
                finally
                {
                    _handler = null;
                }
            }
            else
            {
                Log.E("界面的处理程序为 null");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 获取枚举此 <see cref="View"/> 的直接子界面的枚举器。
        /// </summary>
        public IEnumerator<View> GetEnumerator()
        {
            return InternalGetEnumerator(TreeEnumOrder.Default);
        }

        /// <summary>
        /// 根据指定的枚举顺序，获取一个枚举此 <see cref="View"/> 的枚举器。
        /// </summary>
        /// <param name="order">枚举顺序。</param>
        /// <returns>用于枚举此 <see cref="Node"/> 的枚举器。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="order"/> 不是在 <see cref="TreeEnumOrder"/> 枚举中定义的成员。</exception>
        public IEnumerator<View> GetEnumerator(TreeEnumOrder order)
        {
            return InternalGetEnumerator(order);
        }

        private IEnumerator<View> InternalGetEnumerator(TreeEnumOrder order)
        {
            return order switch
            {
                TreeEnumOrder.Default        => new DirectChildEnumerator(this),
                TreeEnumOrder.BreadthFirstLr => new LrEnumerator<View>(this, FuncGetChildren),
                TreeEnumOrder.BreadthFirstRl => new RlEnumerator<View>(this, FuncGetChildren),
                TreeEnumOrder.DepthFirstDlr  => new DlrEnumerator<View>(this, FuncGetChildren),
                TreeEnumOrder.DepthFirstDrl  => new DrlEnumerator<View>(this, FuncGetChildren),
                TreeEnumOrder.DepthFirstLrd  => new LrdEnumerator<View>(this, FuncGetChildren),
                TreeEnumOrder.DepthFirstRld  => new RldEnumerator<View>(this, FuncGetChildren),
                _                            => throw new ArgumentOutOfRangeException(nameof(order), order, null)
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator<View> IEnumerable<View>.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 界面容器。
        /// </summary>
        public sealed class ViewContainer
        {
            public readonly RectTransform RectTransform;

            internal readonly List<View> Views = new List<View>();

            internal ViewContainer(RectTransform rectTransform)
            {
                if (rectTransform == null)
                {
                    throw new ArgumentNullException(nameof(rectTransform));
                }
                var gameObject = rectTransform.gameObject;
                if (!gameObject.activeSelf)
                {
                    throw new GameObjectInactiveException(gameObject);
                }
                if (gameObject.GetComponentInParent<Canvas>() == null)
                {
                    throw new ComponentNotGotException(gameObject, GetComponentMethod.Parent, typeof(Canvas));
                }
                RectTransform = rectTransform;
            }

            /// <summary>
            /// 按照指定的枚举顺序枚举此容器，并返回第一个满足指定类型的界面。
            /// </summary>
            public T GetViewFromContainer<T>(TreeEnumOrder order) where T : class
            {
                switch (order)
                {
                    case TreeEnumOrder.Default:
                    {
                        foreach (var view in Views)
                        {
                            if (view is T t)
                            {
                                return t;
                            }
                        }
                        return null;
                    }
                    case TreeEnumOrder.BreadthFirstLr:
                    {
                        var queue = PredefinedPools<View>.Queue.Get();
                        try
                        {
                            foreach (var view in Views)
                            {
                                if (view is T t)
                                {
                                    return t;
                                }
                                queue.Enqueue(view);
                            }
                            while (queue.Count > 0)
                            {
                                var view = queue.Dequeue();
                                foreach (var child in view._children)
                                {
                                    if (child is T t)
                                    {
                                        return t;
                                    }
                                    queue.Enqueue(child);
                                }
                            }
                            return null;
                        }
                        finally
                        {
                            PredefinedPools<View>.Queue.Return(queue);
                        }
                    }
                    case TreeEnumOrder.BreadthFirstRl:
                    {
                        var queue = PredefinedPools<View>.Queue.Get();
                        try
                        {
                            for (var i = Views.Count - 1; i >= 0; i--)
                            {
                                var view = Views[i];
                                if (view is T t)
                                {
                                    return t;
                                }
                                queue.Enqueue(view);
                            }
                            while (queue.Count > 0)
                            {
                                var view = queue.Dequeue();
                                for (var i = view._children.Count - 1; i >= 0; i--)
                                {
                                    var child = view._children[i];
                                    if (child is T t)
                                    {
                                        return t;
                                    }
                                    queue.Enqueue(child);
                                }
                            }
                            return null;
                        }
                        finally
                        {
                            PredefinedPools<View>.Queue.Return(queue);
                        }
                    }
                    case TreeEnumOrder.DepthFirstDlr:
                    case TreeEnumOrder.DepthFirstLrd:
                    {
                        foreach (var view in Views)
                        {
                            if (view.GetViewFromThis<T>(order) is { } t)
                            {
                                return t;
                            }
                        }
                        return null;
                    }
                    case TreeEnumOrder.DepthFirstDrl:
                    case TreeEnumOrder.DepthFirstRld:
                    {
                        for (var i = Views.Count - 1; i >= 0; i--)
                        {
                            var view = Views[i];
                            if (view.GetViewFromThis<T>(order) is { } t)
                            {
                                return t;
                            }
                        }
                        return null;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(order), order, null);
                }
            }

            /// <summary>
            /// 按照指定的枚举顺序枚举此容器，并将指定类型的界面存入指定的集合。
            /// </summary>
            public void GetViewsFromContainer<T>(TreeEnumOrder order, ICollection<T> results) where T : class
            {
                if (results == null)
                {
                    throw new ArgumentNullException(nameof(results));
                }
                if (results.IsReadOnly)
                {
                    throw new ArgumentException();
                }
                InternalGetViewsFromContainer(order, results);
            }

            internal void InternalGetViewsFromContainer<T>(TreeEnumOrder order, ICollection<T> results) where T : class
            {
                switch (order)
                {
                    case TreeEnumOrder.Default:
                    {
                        foreach (var view in Views)
                        {
                            if (view is T t)
                            {
                                results.Add(t);
                            }
                        }
                        break;
                    }
                    case TreeEnumOrder.BreadthFirstLr:
                    {
                        var queue = PredefinedPools<View>.Queue.Get();
                        try
                        {
                            foreach (var view in Views)
                            {
                                if (view is T t)
                                {
                                    results.Add(t);
                                }
                                queue.Enqueue(view);
                            }
                            while (queue.Count > 0)
                            {
                                var view = queue.Dequeue();
                                foreach (var child in view._children)
                                {
                                    if (child is T t)
                                    {
                                        results.Add(t);
                                    }
                                    queue.Enqueue(child);
                                }
                            }
                        }
                        finally
                        {
                            PredefinedPools<View>.Queue.Return(queue);
                        }
                        break;
                    }
                    case TreeEnumOrder.BreadthFirstRl:
                    {
                        var queue = PredefinedPools<View>.Queue.Get();
                        try
                        {
                            for (var i = Views.Count - 1; i >= 0; i--)
                            {
                                var view = Views[i];
                                if (view is T t)
                                {
                                    results.Add(t);
                                }
                                queue.Enqueue(view);
                            }
                            while (queue.Count > 0)
                            {
                                var view = queue.Dequeue();
                                for (var i = view._children.Count - 1; i >= 0; i--)
                                {
                                    var child = view._children[i];
                                    if (child is T t)
                                    {
                                        results.Add(t);
                                    }
                                    queue.Enqueue(child);
                                }
                            }
                        }
                        finally
                        {
                            PredefinedPools<View>.Queue.Return(queue);
                        }
                        break;
                    }
                    case TreeEnumOrder.DepthFirstDlr:
                    case TreeEnumOrder.DepthFirstLrd:
                    {
                        foreach (var view in Views)
                        {
                            view.InternalGetViewsFromThis(order, results);
                        }
                        break;
                    }
                    case TreeEnumOrder.DepthFirstDrl:
                    case TreeEnumOrder.DepthFirstRld:
                    {
                        for (var i = Views.Count - 1; i >= 0; i--)
                        {
                            var view = Views[i];
                            view.InternalGetViewsFromThis(order, results);
                        }
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(order), order, null);
                }
            }
        }

        /// <summary>
        /// 界面范围。
        /// </summary>
        /// <typeparam name="T">界面的类型。</typeparam>
        /// <remarks><see cref="Scope{T}"/> 实现 <see cref="IDisposable"/>，调用 <see cref="Dispose"/> 会关闭界面。</remarks>
        public sealed class Scope<T> : IDisposable where T : View
        {
            private T _view;

            private Invocation<object> _closeStateGetter;

            /// <summary>
            /// 初始化 <see cref="Scope{T}"/> 类的新实例。
            /// </summary>
            /// <param name="view">界面。</param>
            public Scope(T view)
            {
                _view             = view;
                _closeStateGetter = null;
            }

            /// <summary>
            /// 初始化 <see cref="Scope{T}"/> 类的新实例。
            /// </summary>
            /// <param name="view">界面。</param>
            /// <param name="closeState">由用户定义的数据。将赋值给 <see cref="CloseState"/>。</param>
            public Scope(T view, object closeState)
            {
                _view             = view;
                _closeStateGetter = Invocation<object>.FromResult(closeState);
            }

            /// <summary>
            /// 初始化 <see cref="Scope{T}"/> 类的新实例。
            /// </summary>
            /// <param name="view">界面。</param>
            /// <param name="closeStateGetter">一个调用，可以通过它获取到由用户定义的数据。该数据将赋值给 <see cref="CloseState"/>。</param>
            public Scope(T view, Invocation<object> closeStateGetter)
            {
                _view             = view;
                _closeStateGetter = closeStateGetter;
            }

            /// <summary>
            /// 关闭界面。
            /// </summary>
            public void Dispose()
            {
                var view             = _view;
                var closeStateGetter = _closeStateGetter;
                if (!view)
                {
                    return;
                }
                _view             = null;
                _closeStateGetter = null;
                view.InternalClose(closeStateGetter?.Invoke());
            }

            /// <summary>
            /// 获取界面。
            /// </summary>
            public T View => _view;
        }

        private struct DirectChildEnumerator : IEnumerator<View>
        {
            private View _view;

            private int _index;

            private View _current;

            internal DirectChildEnumerator(View view)
            {
                _view    = view;
                _index   = 0;
                _current = null;
            }

            void IDisposable.Dispose()
            {
                if (_view == null)
                {
                    return;
                }
                _view    = null;
                _current = null;
            }

            bool IEnumerator.MoveNext()
            {
                if (_view == null)
                {
                    throw new ObjectDisposedException(typeof(DirectChildEnumerator).FullName);
                }
                if (_index == _view._children.Count)
                {
                    _current = null;
                    return false;
                }
                _current = _view._children[_index];
                ++_index;
                return true;
            }

            readonly object IEnumerator.Current => Current;

            readonly View IEnumerator<View>.Current => Current;

            private readonly View Current
            {
                get
                {
                    if (_view == null)
                    {
                        throw new ObjectDisposedException(typeof(DirectChildEnumerator).FullName);
                    }
                    return _current != null ? _current : throw new InvalidOperationException();
                }
            }

            void IEnumerator.Reset()
            {
                if (_view == null)
                {
                    throw new ObjectDisposedException(typeof(DirectChildEnumerator).FullName);
                }
                _index   = 0;
                _current = null;
            }
        }
    }
}
