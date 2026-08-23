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
            var handleableLeastDerivedViewType = viewHandler.HandleableLeastDerivedViewType;
            if (handleableLeastDerivedViewType == null)
            {
                throw new ArgumentException($"{nameof(viewHandler)}.{nameof(HandleableLeastDerivedViewType)} is null");
            }
            if (handleableLeastDerivedViewType != typeof(View) &&
                !handleableLeastDerivedViewType.IsSubclassOf(typeof(View)))
            {
                throw new ArgumentException(
                    $"{nameof(viewHandler)}.{nameof(HandleableLeastDerivedViewType)} is not a view type"
                );
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
                    var handleableLeastDerivedViewType = viewHandler.HandleableLeastDerivedViewType;
                    if (viewType == handleableLeastDerivedViewType ||
                        viewType.IsSubclassOf(handleableLeastDerivedViewType))
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
        /// Gets the least derived view type that this <see cref="ViewHandler"/> can handle.
        /// </summary>
        public abstract Type HandleableLeastDerivedViewType { get; }

        /// <summary>
        /// Creates a view whose associated game object is inactive, or who is itself disabled.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <typeparam name="T">The type of the view to create.</typeparam>
        /// <returns>The task object of the asynchronous operation.</returns>
        /// <exception cref="ArgumentException"><typeparamref name="T"/> is an abstract type, or <typeparamref name="T"/> conflicts with <see cref="HandleableLeastDerivedViewType"/>.</exception>
        /// <remarks>The reason a view must satisfy "the associated game object is inactive, or the view is itself disabled" is to ensure initialization code can be written in the view script's <c>OnEnable</c>.</remarks>
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
