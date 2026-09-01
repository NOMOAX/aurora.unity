using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Collections;
using Aurora.Pooling;
using Aurora.Sorting;
using Object = UnityEngine.Object;

namespace Aurora.Unity.UI.ViewSystem
{
    /// <summary>
    /// A view handler.
    /// </summary>
    public abstract class ViewHandler
    {
        private static readonly HashSet<ViewHandler> ViewHandlers = new();

        private static readonly IComparer<ViewHandler> ReversedViewHandlerComparer =
            new ReversedComparer<ViewHandler>(new ViewHandlerComparer());

        /// <summary>
        /// Registers a view handler.
        /// </summary>
        /// <param name="viewHandler">The view handler.</param>
        /// <exception cref="ArgumentNullException"><paramref name="viewHandler"/> is <see langword="null"/>.</exception>
        public static void Register(ViewHandler viewHandler)
        {
            if (viewHandler is null)
            {
                throw new ArgumentNullException(nameof(viewHandler));
            }
            var handledViewType = viewHandler.HandledViewType;
            if (handledViewType == null)
            {
                throw new ArgumentException($"{nameof(viewHandler)}.{nameof(HandledViewType)} is null");
            }
            if (handledViewType != typeof(View) && !handledViewType.IsSubclassOf(typeof(View)))
            {
                throw new ArgumentException($"{nameof(viewHandler)}.{nameof(HandledViewType)} is not a view type");
            }
            ViewHandlers.Add(viewHandler);
        }

        /// <summary>
        /// Gets the view handler most suitable for handling a view of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the view.</typeparam>
        /// <returns>The view handler most suitable for handling a view of the specified type.</returns>
        public static ViewHandler Get<T>() where T : View
        {
            return InternalGet(typeof(T));
        }

        /// <summary>
        /// Gets the view handler most suitable for handling a view of the specified type.
        /// </summary>
        /// <param name="viewType">The type of the view.</param>
        /// <returns>The view handler most suitable for handling a view of the specified type.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="viewType"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="viewType"/> is not a view type.</exception>
        public static ViewHandler Get(Type viewType)
        {
            if (viewType == null)
            {
                throw new ArgumentNullException(nameof(viewType));
            }
            if (viewType != typeof(View) && !viewType.IsSubclassOf(typeof(View)))
            {
                throw new ArgumentException($"{nameof(viewType)} is not a view type", nameof(viewType));
            }
            return InternalGet(viewType);
        }

        private static ViewHandler InternalGet(Type viewType)
        {
            var list = PredefinedPools<ViewHandler>.List.Get();
            try
            {
                foreach (var viewHandler in ViewHandlers)
                {
                    var handledViewType = viewHandler.HandledViewType;
                    if (viewType == handledViewType || viewType.IsSubclassOf(handledViewType))
                    {
                        list.Add(viewHandler);
                    }
                }
                TimSort.Sort(list, ReversedViewHandlerComparer);
                return list.Count > 0 ? list[0] : null;
            }
            finally
            {
                PredefinedPools<ViewHandler>.List.Return(list);
            }
        }

        /// <summary>
        /// Gets the view type that this <see cref="ViewHandler"/> handles.
        /// </summary>
        public abstract Type HandledViewType { get; }

        /// <summary>
        /// Creates an inactive or disabled view.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <typeparam name="T">The type of the view to create.</typeparam>
        /// <returns>The task object of the asynchronous operation.</returns>
        /// <exception cref="ArgumentException"><typeparamref name="T"/> is an abstract type, or <typeparamref name="T"/> is not derived from <see cref="HandledViewType"/>.</exception>
        /// <remarks>The view is inactive or disabled so that its <c>OnEnable</c> is not invoked prematurely during creation, allowing developers to safely write initialization code in <c>OnEnable</c>.</remarks>
        public abstract Task<T> CreateInactiveOrDisabledViewAsync<T>(CancellationToken cancellationToken = default)
            where T : View;

        /// <summary>
        /// Releases a view.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <remarks>This method is intended only for the view system to call; do not call it yourself.</remarks>
        public virtual void ReleaseView(View view)
        {
            Object.Destroy(view.gameObject);
        }
    }
}
