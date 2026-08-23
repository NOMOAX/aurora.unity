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
    /// A view.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public abstract class View : UIBehaviour, IEnumerable<View>
    {
#if UNITY_EDITOR
        internal static bool Dirty;
#endif

        private static readonly List<ViewContainer> Containers = new();

        private static readonly Func<View, IEnumerable<View>> GetChildrenFunc = view => view._children;

        private static readonly ParameterizedPredicate<ViewContainer, RectTransform>
            ViewContainerHasRectTransformPredicate = (container, rectTransform) =>
                ReferenceEquals(container.RectTransform, rectTransform);

        /// <summary>
        /// The first parameter (<see cref="View"/>) is not equal to the <see cref="View"/> of the second tuple parameter, and the first parameter (<see cref="View"/>) is an instance of the <see cref="Type"/> of the second tuple parameter.
        /// </summary>
        private static readonly ParameterizedPredicate<View, ValueTuple<Object, Type>>
            ViewIsNotEqualToStateViewAndIsInstanceOfStateTypePredicate = (view, state) =>
            {
                var (target, type) = state;
                return !ReferenceEquals(view, target) && type.IsInstanceOfType(view);
            };

        /// <summary>
        /// The first parameter (<see cref="View"/>) is an instance of the second parameter (<see cref="Type"/>).
        /// </summary>
        private static readonly ParameterizedPredicate<View, Type> ViewIsInstanceOfTypePredicate =
            (view, type) => type.IsInstanceOfType(view);

        /// <summary>
        /// Gets the number of view containers.
        /// </summary>
        public static int ContainerCount => Containers.Count;

        /// <summary>
        /// Adds a view container.
        /// </summary>
        /// <param name="rectTransform">A RectTransform that will serve as the parent object of the root views under this view container.</param>
        /// <returns>The view container that uses <paramref name="rectTransform"/> as the parent object of its root views.</returns>
        public static ViewContainer AddContainer(RectTransform rectTransform)
        {
            if (!rectTransform)
            {
                throw new ArgumentNullException(nameof(rectTransform));
            }
            var index = Containers.FindIndex(ViewContainerHasRectTransformPredicate, rectTransform);
            if (index >= 0)
            {
                throw new ArgumentException();
            }
            var viewContainer = new ViewContainer(rectTransform);
            Containers.Add(viewContainer);
#if UNITY_EDITOR
            Dirty = true;
#endif
            return viewContainer;
        }

        /// <summary>
        /// Removes the view container at the specified index.
        /// </summary>
        /// <param name="index">The index of the view container.</param>
        /// <exception cref="InvalidOperationException">The number of views under the view container to remove is not 0.</exception>
        public static void RemoveContainerAt(int index)
        {
            var container = Containers[index];
            if (container.Views.Count != 0)
            {
                throw new InvalidOperationException("Cannot remove this view container because it still holds views");
            }
            Containers.RemoveAt(index);
#if UNITY_EDITOR
            Dirty = true;
#endif
        }

        /// <summary>
        /// Gets the view container at the specified index.
        /// </summary>
        /// <param name="index">The index of the view container.</param>
        public static ViewContainer GetContainer(int index)
        {
            return Containers[index];
        }

#if UNITY_EDITOR
        internal static void ClearContainers()
        {
            if (Containers.Count == 0)
            {
                return;
            }
            Containers.Clear();
            Dirty = true;
        }
#endif

        /// <summary>
        /// Determines whether the specified view is the topmost view.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <returns><see langword="true"/> if <paramref name="view"/> is the topmost view; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="view"/> is <see langword="null"/>.</exception>
        public static bool IsTopmost(View view)
        {
            if (!view)
            {
                throw new ArgumentNullException(nameof(view));
            }
            return InternalIsTopmost(view);
        }

        /// <summary>
        /// Determines whether the specified view is the topmost view.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="exclusions">An array containing the views or view types to exclude.</param>
        /// <returns><see langword="true"/> if <paramref name="view"/> is the topmost view; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="view"/> is <see langword="null"/>, or <paramref name="exclusions"/> contains a <see langword="null"/> element.</exception>
        /// <exception cref="ArgumentException"><paramref name="exclusions"/> contains <paramref name="view"/>, or contains an element that is neither a <see cref="View"/> type nor a <see cref="Type"/> type.</exception>
        public static bool IsTopmost(View view, object[] exclusions)
        {
            if (!view)
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
                            var state = ValueTuple.Create(view, excludedViewType);
                            views.RemoveAll(ViewIsNotEqualToStateViewAndIsInstanceOfStateTypePredicate, state);
                            break;
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
        /// Gets the topmost view.
        /// </summary>
        /// <returns>The topmost view.</returns>
        public static View GetTopmostView()
        {
            return InternalGetTopmostView();
        }

        /// <summary>
        /// Gets the topmost view.
        /// </summary>
        /// <param name="exclusions">An array containing the views or view types to exclude.</param>
        /// <returns>The topmost view.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="exclusions"/> contains a <see langword="null"/> element.</exception>
        /// <exception cref="ArgumentException"><paramref name="exclusions"/> contains an element that is neither a <see cref="View"/> type nor a <see cref="Type"/> type.</exception>
        public static View GetTopmostView(object[] exclusions)
        {
            return exclusions == null || exclusions.Length == 0
                       ? InternalGetTopmostView()
                       : InternalGetTopmostView(exclusions);
        }

        private static View InternalGetTopmostView()
        {
            // return GetView<View>(TreeEnumOrder.DepthFirstRld);
            // The implementation below consumes less memory and performance

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
                            views.RemoveAll(ViewIsInstanceOfTypePredicate, excludedViewType);
                            break;
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
        /// Enumerates all views in <see cref="TreeEnumOrder.DepthFirstRld"/> order and returns the first view that satisfies the specified type.
        /// </summary>
        public static T GetView<T>() where T : class
        {
            return InternalGetView<T>(TreeEnumOrder.DepthFirstRld);
        }

        /// <summary>
        /// Enumerates all views in the specified enumeration order and returns the first view that satisfies the specified type.
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
        /// Enumerates all views in the specified enumeration order and stores the views of the specified type into the specified collection.
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
                    $"Unable to get a view handler suitable for handling a view of type {TypeUtility.GetNicelyFormattedName(typeof(T))}"
                );
            }
            var view = await handler.CreateInactiveOrDisabledViewAsync<T>(cancellationToken);
            view._handler = handler;
            return view;
        }

        /// <summary>
        /// Begins asynchronously opening a root view.
        /// </summary>
        public static async void BeginOpen<T>(ViewContainer container) where T : View
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            await InternalOpenAsync<T>(container, null, CancellationToken.None);
        }

        /// <summary>
        /// Begins asynchronously opening a root view.
        /// </summary>
        public static async void BeginOpen<T>(ViewContainer container, CancellationToken cancellationToken)
            where T : View
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            cancellationToken.ThrowIfCancellationRequested();
            await InternalOpenAsync<T>(container, null, cancellationToken);
        }

        /// <summary>
        /// Begins asynchronously opening a root view.
        /// </summary>
        public static async void BeginOpen<T>(ViewContainer container, object state) where T : View
        {
            if (container is null)
            {
                throw new ArgumentNullException(nameof(container));
            }
            await InternalOpenAsync<T>(container, state, CancellationToken.None);
        }

        /// <summary>
        /// Begins asynchronously opening a root view.
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
            await InternalOpenAsync<T>(container, state, cancellationToken);
        }

        /// <summary>
        /// Begins asynchronously opening a view.
        /// </summary>
        public static async void BeginOpen<T>(View parent) where T : View
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }
            await InternalOpenAsync<T>(parent, null, CancellationToken.None);
        }

        /// <summary>
        /// Begins asynchronously opening a view.
        /// </summary>
        public static async void BeginOpen<T>(View parent, CancellationToken cancellationToken) where T : View
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }
            cancellationToken.ThrowIfCancellationRequested();
            await InternalOpenAsync<T>(parent, null, cancellationToken);
        }

        /// <summary>
        /// Begins asynchronously opening a view.
        /// </summary>
        public static async void BeginOpen<T>(View parent, object state) where T : View
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }
            await InternalOpenAsync<T>(parent, state, CancellationToken.None);
        }

        /// <summary>
        /// Begins asynchronously opening a view.
        /// </summary>
        public static async void BeginOpen<T>(View parent, object state, CancellationToken cancellationToken)
            where T : View
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }
            cancellationToken.ThrowIfCancellationRequested();
            await InternalOpenAsync<T>(parent, state, cancellationToken);
        }

        /// <summary>
        /// Asynchronously opens a root view.
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
        /// Asynchronously opens a root view.
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
        /// Asynchronously opens a root view.
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
        /// Asynchronously opens a root view.
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
            if (!view)
            {
                throw new InvalidOperationException(
                    $"The view created by the view handler most suitable for handling a view of type {TypeUtility.GetNicelyFormattedName(typeof(T))} is null"
                );
            }
            if (view.gameObject.activeSelf && view.enabled)
            {
                throw new BehaviourActiveAndEnabledException(
                    view,
                    $"The view created by the view handler most suitable for handling a view of type {TypeUtility.GetNicelyFormattedName(typeof(T))} has an associated game object that is active and the view is enabled"
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
#if UNITY_EDITOR
            Dirty = true;
#endif

            return view;
        }

        /// <summary>
        /// Asynchronously opens a view.
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
        /// Asynchronously opens a view.
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
        /// Asynchronously opens a view.
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
        /// Asynchronously opens a view.
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
            if (!view)
            {
                throw new InvalidOperationException(
                    $"The view created by the view handler most suitable for handling a view of type {TypeUtility.GetNicelyFormattedName(typeof(T))} is null"
                );
            }
            if (view.gameObject.activeSelf && view.enabled)
            {
                throw new BehaviourActiveAndEnabledException(
                    view,
                    $"The view created by the view handler most suitable for handling a view of type {TypeUtility.GetNicelyFormattedName(typeof(T))} has an associated game object that is active and the view is enabled"
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
#if UNITY_EDITOR
            Dirty = true;
#endif

            return view;
        }

        private RectTransform _rectTransform;

        private ViewHandler _handler;

        private ViewContainer _container;

        private View _parent;

        /// <summary>
        /// The container of child views.
        /// </summary>
        public RectTransform childContainer;

        private readonly List<View> _children = new();

        /// <summary>
        /// Gets the RectTransform associated with this instance.
        /// </summary>
        public RectTransform RectTransform => _rectTransform ??= (RectTransform)transform;

        /// <summary>
        /// User-defined data. It is assigned when the view is opened.
        /// </summary>
        public object State { get; private set; }

        /// <summary>
        /// User-defined data. It is assigned when the view is closed.
        /// </summary>
        public object CloseState { get; private set; }

        private RectTransform ChildContainerOrRectTransformIfNull => childContainer ?? RectTransform;

        /// <summary>
        /// Gets or sets the container.
        /// </summary>
        /// <remarks>Cannot be set to <see langword="null"/>.</remarks>
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
#if UNITY_EDITOR
                Dirty = true;
#endif
            }
        }

        /// <summary>
        /// Gets or sets the parent view.
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
                        throw new ArgumentException("Cannot set itself as its own parent view", nameof(value));
                    }
                    if (value.InternalIsChildOf(this))
                    {
                        throw new ArgumentException("Cannot set a child view as the parent view", nameof(value));
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
#if UNITY_EDITOR
                Dirty = true;
#endif
            }
        }

        /// <summary>
        /// Gets the root view.
        /// </summary>
        public View Root
        {
            get
            {
                var root = this;
                for (var parent = _parent; parent; parent = parent._parent)
                {
                    root = parent;
                }
                return root;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this view is a root view.
        /// </summary>
        public bool IsRoot => _parent is null;

        /// <summary>
        /// Gets a value indicating whether this view is a leaf view.
        /// </summary>
        public bool IsLeaf => _children.Count == 0;

        /// <summary>
        /// Gets a value indicating whether this view is the topmost view.
        /// </summary>
        /// <returns><see langword="true"/> if this view is the topmost view; otherwise <see langword="false"/>.</returns>
        /// <remarks>This is a virtual method; if a subclass does not override it, <see cref="IsTopmost(View)"/> is actually executed.</remarks>
        public virtual bool IsTopmost()
        {
            return InternalIsTopmost(this);
        }

        /// <summary>
        /// Called before the last step of opening a view: activating the game object associated with this view and enabling this view.
        /// </summary>
        protected virtual void OnSettingActiveAndEnabling()
        {
        }

        /// <summary>
        /// Gets a value indicating whether this view is a child view of the specified view.
        /// </summary>
        /// <param name="view">The specified view.</param>
        /// <returns>Whether this view is a child view of the specified view.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="view"/> is <see langword="null"/>.</exception>
        public bool IsChildOf(View view)
        {
            if (!view)
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
            for (var parent = _parent; parent; parent = parent._parent)
            {
                if (ReferenceEquals(view, parent))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets a child view of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the view.</typeparam>
        /// <returns>The first matching child view if one of the specified type exists; otherwise <see langword="null"/>.</returns>
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
        /// Gets child views of the specified type and stores the results into the specified list.
        /// </summary>
        /// <param name="results">The list used to hold the results.</param>
        /// <typeparam name="T">The type of the view.</typeparam>
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
        /// Enumerates this view in the specified enumeration order and returns the first view that satisfies the specified type.
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
        /// Enumerates this view in the specified enumeration order and stores the views of the specified type into the specified collection.
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
        /// Closes the view.
        /// </summary>
        /// <param name="closeState">User-defined data, assigned to <see cref="View.CloseState"/>.</param>
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

#if UNITY_EDITOR
            Dirty = true;
#endif

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
                    if (this)
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
                Log.E("The view handler is null");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Gets an enumerator that enumerates the direct child views of this <see cref="View"/>.
        /// </summary>
        public IEnumerator<View> GetEnumerator()
        {
            return InternalGetEnumerator(TreeEnumOrder.Default);
        }

        /// <summary>
        /// Gets an enumerator that enumerates this <see cref="View"/> according to the specified enumeration order.
        /// </summary>
        /// <param name="order">The enumeration order.</param>
        /// <returns>The enumerator used to enumerate this <see cref="Node"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="order"/> is not a member defined in the <see cref="TreeEnumOrder"/> enum.</exception>
        public IEnumerator<View> GetEnumerator(TreeEnumOrder order)
        {
            return InternalGetEnumerator(order);
        }

        private IEnumerator<View> InternalGetEnumerator(TreeEnumOrder order)
        {
            return order switch
            {
                TreeEnumOrder.Default        => new DirectChildEnumerator(this),
                TreeEnumOrder.BreadthFirstLr => new LrEnumerator<View>(this, GetChildrenFunc),
                TreeEnumOrder.BreadthFirstRl => new RlEnumerator<View>(this, GetChildrenFunc),
                TreeEnumOrder.DepthFirstDlr  => new DlrEnumerator<View>(this, GetChildrenFunc),
                TreeEnumOrder.DepthFirstDrl  => new DrlEnumerator<View>(this, GetChildrenFunc),
                TreeEnumOrder.DepthFirstLrd  => new LrdEnumerator<View>(this, GetChildrenFunc),
                TreeEnumOrder.DepthFirstRld  => new RldEnumerator<View>(this, GetChildrenFunc),
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
        /// A view container.
        /// </summary>
        public sealed class ViewContainer
        {
            /// <summary>
            /// The parent object of the root views under <see cref="ViewContainer"/>.
            /// </summary>
            public readonly RectTransform RectTransform;

            internal readonly List<View> Views = new();

            internal ViewContainer(RectTransform rectTransform)
            {
                if (!rectTransform)
                {
                    throw new ArgumentNullException(nameof(rectTransform));
                }
                var gameObject = rectTransform.gameObject;
                if (!gameObject.activeSelf)
                {
                    throw new GameObjectInactiveException(gameObject);
                }
                if (!gameObject.GetComponentInParent<Canvas>())
                {
                    throw new ComponentNotGotException(gameObject, GetComponentMethod.Parent, typeof(Canvas));
                }
                RectTransform = rectTransform;
            }

            /// <summary>
            /// Enumerates this container in the specified enumeration order and returns the first view that satisfies the specified type.
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
            /// Enumerates this container in the specified enumeration order and stores the views of the specified type into the specified collection.
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
        /// A view scope.
        /// </summary>
        /// <typeparam name="T">The type of the view.</typeparam>
        /// <remarks><see cref="Scope{T}"/> implements <see cref="IDisposable"/>; calling <see cref="Dispose"/> closes the view.</remarks>
        public sealed class Scope<T> : IDisposable where T : View
        {
            private T _view;

            private Invocation<object> _closeStateGetter;

            /// <summary>
            /// Initializes a new instance of the <see cref="Scope{T}"/> class.
            /// </summary>
            /// <param name="view">The view.</param>
            public Scope(T view)
            {
                _view             = view;
                _closeStateGetter = null;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Scope{T}"/> class.
            /// </summary>
            /// <param name="view">The view.</param>
            /// <param name="closeState">User-defined data. It is assigned to <see cref="CloseState"/>.</param>
            public Scope(T view, object closeState)
            {
                _view             = view;
                _closeStateGetter = Invocation<object>.FromResult(closeState);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="Scope{T}"/> class.
            /// </summary>
            /// <param name="view">The view.</param>
            /// <param name="closeStateGetter">A call through which user-defined data can be obtained. That data is assigned to <see cref="CloseState"/>.</param>
            public Scope(T view, Invocation<object> closeStateGetter)
            {
                _view             = view;
                _closeStateGetter = closeStateGetter;
            }

            /// <summary>
            /// Closes the view.
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
            /// Gets the view.
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
                if (!_view)
                {
                    return;
                }
                _view    = null;
                _current = null;
            }

            bool IEnumerator.MoveNext()
            {
                if (!_view)
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
                    if (!_view)
                    {
                        throw new ObjectDisposedException(typeof(DirectChildEnumerator).FullName);
                    }
                    return _current ? _current : throw new InvalidOperationException();
                }
            }

            void IEnumerator.Reset()
            {
                if (!_view)
                {
                    throw new ObjectDisposedException(typeof(DirectChildEnumerator).FullName);
                }
                _index   = 0;
                _current = null;
            }
        }
    }
}
