using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Collections;
using Aurora.Diagnostics;
using Aurora.Pooling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity.UI.ViewSystem
{
    /// <summary>
    /// 界面。
    /// </summary>
    public abstract class View : MonoBehaviour2D, IEnumerable<View>
    {
        private static RectTransform _rootViewContainer;

        private static readonly List<View> RootViews = new List<View>();

        private static readonly Func<View, IEnumerable<View>> FuncGetChildren = GetChildrenAsEnumerable;

        /// <summary>
        /// 根界面的容器。
        /// </summary>
        public static RectTransform RootViewContainer
        {
            get => _rootViewContainer;
            set
            {
                if (_rootViewContainer == value)
                {
                    return;
                }
                if (value != null)
                {
                    if (!value.gameObject.activeSelf)
                    {
                        throw new ArgumentException(
                            $"{value} 所在的游戏物体处于未激活状态",
                            nameof(value),
                            new GameObjectInactiveException(value.gameObject)
                        );
                    }
                    if (value.gameObject.GetComponentInParent<Canvas>() == null)
                    {
                        throw new ArgumentException($"{value} 不是关联有 {nameof(Canvas)} 组件的物体或子物体", nameof(value));
                    }
                    foreach (var rootView in RootViews)
                    {
                        rootView.transform.SetParent(value, false);
                        RectTransformUtility.AlignToParentEdges(rootView.RectTransform);
                    }
                }
                else
                {
                    if (RootViews.Count > 0 && _rootViewContainer != null)
                    {
                        throw new ArgumentException("在将此属性设置为 null 之前，必须关闭所有根界面", nameof(value));
                    }
                }
                _rootViewContainer = value;
            }
        }

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
            return exclusions != null && exclusions.Length > 0
                       ? InternalIsTopmost(view, exclusions)
                       : InternalIsTopmost(view);
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
                GetViewsWithoutRoot(TreeEnumOrder.DepthFirstRld, views);
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
            return exclusions != null && exclusions.Length > 0
                       ? InternalGetTopmostView(exclusions)
                       : InternalGetTopmostView();
        }

        private static View InternalGetTopmostView()
        {
            // return GetViewWithoutRoot<View>(TreeEnumOrder.DepthFirstRld);
            // 下面的实现消耗更少的内存和性能

            var rootViewCount = RootViews.Count;
            if (rootViewCount == 0)
            {
                return null;
            }
            var topmostView = RootViews[rootViewCount - 1];
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
                GetViewsWithoutRoot(TreeEnumOrder.DepthFirstRld, views);
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
        /// 按照 <see cref="TreeEnumOrder.DepthFirstRld"/> 顺序枚举指定的界面并获取指定类型的界面。
        /// </summary>
        /// <param name="root">指定从该界面枚举并获取；如果为 <see langword="null"/>，则表示枚举各个根界面并获取。</param>
        /// <typeparam name="T">要获取的界面的类型。</typeparam>
        /// <returns>如果获取到了指定类型的界面，则为获取到的界面；否则为 <see langword="null"/>。</returns>
        public static T GetView<T>(View root) where T : View
        {
            return GetView<T>(root, TreeEnumOrder.DepthFirstRld);
        }

        /// <summary>
        /// 按照指定的顺序枚举指定的界面并获取一个指定类型的界面。
        /// </summary>
        /// <param name="root">指定从该界面枚举并获取；如果为 <see langword="null"/>，则表示枚举各个根界面并获取。</param>
        /// <param name="order">枚举顺序。</param>
        /// <typeparam name="T">要获取的界面的类型。</typeparam>
        /// <returns>如果获取到了指定类型的界面，则为获取到的界面；否则为 <see langword="null"/>。</returns>
        public static T GetView<T>(View root, TreeEnumOrder order) where T : View
        {
            return root == null ? GetViewFromRootViews<T>(order) : GetViewFromRoot<T>(root, order);
        }

        /// <summary>
        /// 按照 <see cref="TreeEnumOrder.DepthFirstRld"/> 顺序枚举指定的界面并获取指定类型的界面，并将结果存入指定的列表。
        /// </summary>
        /// <param name="root">指定从该界面枚举并获取；如果为 <see langword="null"/>，则表示枚举各个根界面并获取。</param>
        /// <param name="results">用于存放结果的列表。</param>
        /// <typeparam name="T">要获取的界面的类型。</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="results"/> 为 <see langword="null"/>。</exception>
        public static void GetViews<T>(View root, List<T> results) where T : View
        {
            GetViews(root, TreeEnumOrder.DepthFirstRld, results);
        }

        /// <summary>
        /// 按照指定的顺序枚举指定的界面并获取指定类型的界面，并将结果存入指定的列表。
        /// </summary>
        /// <param name="root">指定从该界面枚举并获取；如果为 <see langword="null"/>，则表示枚举各个根界面并获取。</param>
        /// <param name="order">枚举顺序。</param>
        /// <param name="results">用于存放结果的列表。</param>
        /// <typeparam name="T">要获取的界面的类型。</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="results"/> 为 <see langword="null"/>。</exception>
        public static void GetViews<T>(View root, TreeEnumOrder order, List<T> results) where T : View
        {
            if (results is null)
            {
                throw new ArgumentNullException(nameof(results));
            }
            if (root == null)
            {
                GetViewsWithoutRoot(order, results);
            }
            else
            {
                GetViewsWithRoot(root, order, results);
            }
        }

        private static T GetViewFromRootViews<T>(TreeEnumOrder order) where T : View
        {
            switch (order)
            {
                case TreeEnumOrder.Default:
                {
                    foreach (var rootView in RootViews)
                    {
                        if (rootView is T t)
                        {
                            return t;
                        }
                    }
                    break;
                }
                case TreeEnumOrder.BreadthFirstLr:
                {
                    var queue = PredefinedPools<View>.Queue.Get();
                    try
                    {
                        foreach (var rootView in RootViews)
                        {
                            if (rootView is T t)
                            {
                                return t;
                            }
                            queue.Enqueue(rootView);
                        }
                        while (queue.Count > 0)
                        {
                            var dequeue = queue.Dequeue();
                            foreach (var dequeueChild in dequeue._children)
                            {
                                if (dequeueChild is T t)
                                {
                                    return t;
                                }
                                queue.Enqueue(dequeueChild);
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
                        for (var i = RootViews.Count - 1; i >= 0; i--)
                        {
                            var rootView = RootViews[i];
                            if (rootView is T t)
                            {
                                return t;
                            }
                            queue.Enqueue(rootView);
                        }
                        while (queue.Count > 0)
                        {
                            var dequeue = queue.Dequeue();
                            for (var i = dequeue._children.Count - 1; i >= 0; i--)
                            {
                                var dequeueChild = dequeue._children[i];
                                if (dequeueChild is T t)
                                {
                                    return t;
                                }
                                queue.Enqueue(dequeueChild);
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
                    foreach (var rootView in RootViews)
                    {
                        var t = GetViewFromRoot<T>(rootView, order);
                        if (t != null)
                        {
                            return t;
                        }
                    }
                    break;
                }
                case TreeEnumOrder.DepthFirstDrl:
                case TreeEnumOrder.DepthFirstRld:
                {
                    for (var i = RootViews.Count - 1; i >= 0; i--)
                    {
                        var rootView = RootViews[i];
                        var t        = GetViewFromRoot<T>(rootView, order);
                        if (t != null)
                        {
                            return t;
                        }
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(order), order, null);
            }
            return null;
        }

        private static T GetViewFromRoot<T>(View root, TreeEnumOrder order) where T : View
        {
            using var enumerator = root.InternalGetEnumerator(order);
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is T t)
                {
                    return t;
                }
            }
            return null;
        }

        private static void GetViewsWithoutRoot<T>(TreeEnumOrder order, ICollection<T> results)
        {
            switch (order)
            {
                case TreeEnumOrder.Default:
                {
                    foreach (var rootView in RootViews)
                    {
                        if (rootView is T t)
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
                        foreach (var rootView in RootViews)
                        {
                            queue.Enqueue(rootView);
                        }
                        while (queue.Count > 0)
                        {
                            var dequeue = queue.Dequeue();
                            foreach (var dequeueChild in dequeue._children)
                            {
                                queue.Enqueue(dequeueChild);
                            }
                            if (dequeue is T t)
                            {
                                results.Add(t);
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
                        for (var i = RootViews.Count - 1; i >= 0; i--)
                        {
                            var rootView = RootViews[i];
                            queue.Enqueue(rootView);
                        }
                        while (queue.Count > 0)
                        {
                            var dequeue = queue.Dequeue();
                            for (var i = dequeue._children.Count - 1; i >= 0; i--)
                            {
                                var dequeueChild = dequeue._children[i];
                                queue.Enqueue(dequeueChild);
                            }
                            if (dequeue is T t)
                            {
                                results.Add(t);
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
                    foreach (var rootView in RootViews)
                    {
                        GetViewsWithRoot(rootView, order, results);
                    }
                    break;
                }
                case TreeEnumOrder.DepthFirstDrl:
                case TreeEnumOrder.DepthFirstRld:
                {
                    for (var i = RootViews.Count - 1; i >= 0; i--)
                    {
                        var rootView = RootViews[i];
                        GetViewsWithRoot(rootView, order, results);
                    }
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(order), order, null);
            }
        }

        private static void GetViewsWithRoot<T>(View root, TreeEnumOrder order, ICollection<T> results)
        {
            using var enumerator = root.InternalGetEnumerator(order);
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is T t)
                {
                    results.Add(t);
                }
            }
        }

        /// <summary>
        /// 异步创建“所关联的游戏物体处于未激活状态，或者其本身禁用”的界面。
        /// </summary>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <typeparam name="T">界面的类型。</typeparam>
        /// <returns>异步操作的任务对象，它的 <see cref="Task{TResult}.Result"/> 为创建出来的界面。</returns>
        public static async Task<T> CreateInactiveOrDisabledViewAsync<T>(CancellationToken cancellationToken)
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
        /// 开始异步打开界面。
        /// </summary>
        /// <param name="openParameters">界面打开参数。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <typeparam name="T">界面的类型。</typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="openParameters"/> 为 <see langword="null"/>。</exception>
        public static async void BeginOpen<T>(
            OpenParameters    openParameters,
            CancellationToken cancellationToken = default) where T : View
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = await InternalOpenAsync<T>(openParameters, cancellationToken);
        }

        /// <summary>
        /// 异步打开界面。
        /// </summary>
        /// <param name="openParameters">界面打开参数。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <typeparam name="T">界面的类型。</typeparam>
        /// <returns>异步操作的任务对象，它的 <see cref="Task{TResult}.Result"/> 为打开的界面。</returns>
        public static Task<T> OpenAsync<T>(OpenParameters openParameters, CancellationToken cancellationToken = default)
            where T : View
        {
            return !cancellationToken.IsCancellationRequested
                       ? InternalOpenAsync<T>(openParameters, cancellationToken)
                       : Task.FromCanceled<T>(cancellationToken);
        }

        private static async Task<T> InternalOpenAsync<T>(
            OpenParameters    openParameters,
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
            View   parent;
            object state;
            if (openParameters == null)
            {
                parent = null;
                state  = null;
            }
            else
            {
                parent = openParameters.Parent;
                state  = openParameters.State;
            }
            view.SetParent(parent);
            view.transform.SetAsLastSibling();
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

        private ViewHandler _handler;

        /// <summary>
        /// 子界面的容器。
        /// </summary>
        public RectTransform childContainer;

        private View _parent;

        private readonly List<View> _children = new List<View>();

        /// <summary>
        /// 由用户定义的数据。将在调用 <see cref="OpenAsync{T}"/> 时被赋值。
        /// </summary>
        public abstract object State { get; set; }

        /// <summary>
        /// 由用户定义的数据。将在调用 <see cref="Close"/> 时被赋值。
        /// </summary>
        public abstract object CloseState { get; set; }

        /// <summary>
        /// 获取子界面的容器。
        /// </summary>
        /// <remarks>如果 <see cref="childContainer"/> 不为 <see langword="null"/>，则返回 <see cref="childContainer"/>；否则返回 <see cref="MonoBehaviour2D.RectTransform"/>。</remarks>
        public RectTransform ChildContainerOrRectTransform => childContainer != null ? childContainer : RectTransform;

        /// <summary>
        /// 获取或设置父界面。
        /// </summary>
        public View Parent { get => _parent; set => SetParent(value); }

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
        /// 在执行 <see cref="OpenAsync{T}"/> 的最后一步“激活此界面所关联的游戏物体，并启用此界面”前调用此方法。
        /// </summary>
        protected virtual void OnSettingActiveAndEnabling()
        {
        }

        private void SetParent(View parent)
        {
            if (parent == null)
            {
                SetParentNull();
            }
            else
            {
                SetParentNotNull(parent);
            }
        }

        private void SetParentNull()
        {
            if (_parent == null)
            {
                if (RootViews.Contains(this))
                {
                    return;
                }
            }
            else
            {
                _parent._children.Remove(this);
                _parent = null;
            }
            RootViews.Add(this);
            RectTransform.SetParent(_rootViewContainer, false);
            RectTransformUtility.AlignToParentEdges(RectTransform);
        }

        private void SetParentNotNull(View parent)
        {
            if (ReferenceEquals(_parent, parent))
            {
                return;
            }
            if (parent == this || parent.IsChildOf(this))
            {
                throw new ArgumentException($"{nameof(parent)} 不能是此实例本身，或此实例的子界面", nameof(parent));
            }
            if (_parent == null)
            {
                RootViews.Remove(this);
            }
            else
            {
                _parent._children.Remove(this);
            }
            _parent = parent;
            _parent._children.Add(this);
            RectTransform.SetParent(_parent.ChildContainerOrRectTransform, false);
            RectTransformUtility.AlignToParentEdges(RectTransform);
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
            if (view == this)
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
        /// 关闭界面。
        /// </summary>
        /// <param name="closeState">由用户定义的数据，将赋值给 <see cref="View.CloseState"/>。</param>
        public void Close(object closeState = null)
        {
            InternalClose(closeState);
        }

        private void InternalClose(object closeState)
        {
            // 关闭子界面
            for (var i = _children.Count - 1; i >= 0; i--)
            {
                var child = _children[i];
                _children.RemoveAt(i);
                child.InternalClose(null);
            }

            // 如果是根界面，则从根界面列表中移除；否则，解除与父界面的关系
            if (_parent == null)
            {
                RootViews.Remove(this);
            }
            else
            {
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
        /// 根据指定的枚举顺序，获取一个枚举此 <see cref="View"/> 的枚举器。
        /// </summary>
        /// <param name="order">枚举顺序。</param>
        /// <returns>用于枚举此 <see cref="Node"/> 的枚举器。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="order"/> 的值未定义。</exception>
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
            return InternalGetEnumerator(TreeEnumOrder.Default);
        }

        /// <inheritdoc />
        public IEnumerator<View> GetEnumerator()
        {
            return InternalGetEnumerator(TreeEnumOrder.Default);
        }

        /// <summary>
        /// 界面打开参数。
        /// </summary>
        public sealed class OpenParameters
        {
            /// <summary>
            /// 将要打开界面的父界面；如果为 <see langword="null"/>，则表示将要打开界面为根界面。
            /// </summary>
            public readonly View Parent;

            /// <summary>
            /// 由用户定义的数据，将赋值给将要打开界面的 <see cref="View.State"/>。
            /// </summary>
            public readonly object State;

            /// <summary>
            /// 初始化 <see cref="OpenParameters"/> 类的新实例。
            /// </summary>
            /// <param name="parent">将要打开界面的父界面；如果为 <see langword="null"/>，则表示将要打开界面为根界面。</param>
            /// <param name="state">由用户定义的数据，将赋值给将要打开界面的 <see cref="View.State"/>。</param>
            public OpenParameters(View parent, object state = null)
            {
                Parent = parent;
                State  = state;
            }
        }

        /// <summary>
        /// 界面范围。
        /// </summary>
        /// <remarks><see cref="Scope"/> 实现 <see cref="IDisposable"/>，调用 <see cref="Dispose"/> 会关闭界面。</remarks>
        public sealed class Scope : IDisposable
        {
            private View _view;

            private Invocation<object> _closeStateGetter;

            /// <summary>
            /// 初始化 <see cref="Scope"/> 类的新实例。
            /// </summary>
            /// <param name="view">界面。</param>
            public Scope(View view)
            {
                _view             = view;
                _closeStateGetter = null;
            }

            /// <summary>
            /// 初始化 <see cref="Scope"/> 类的新实例。
            /// </summary>
            /// <param name="view">界面。</param>
            /// <param name="closeState">由用户定义的数据。将赋值给 <see cref="CloseState"/>。</param>
            public Scope(View view, object closeState)
            {
                _view             = view;
                _closeStateGetter = Invocation<object>.FromResult(closeState);
            }

            /// <summary>
            /// 初始化 <see cref="Scope"/> 类的新实例。
            /// </summary>
            /// <param name="view">界面。</param>
            /// <param name="closeStateGetter">一个调用，可以通过它获取到由用户定义的数据。该数据将赋值给 <see cref="CloseState"/>。</param>
            public Scope(View view, Invocation<object> closeStateGetter)
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
                if (view == null)
                {
                    return;
                }
                _view             = null;
                _closeStateGetter = null;
                view.InternalClose(closeStateGetter?.Invoke());
            }
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
