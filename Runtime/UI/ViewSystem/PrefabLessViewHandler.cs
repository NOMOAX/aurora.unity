using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Aurora.Unity.UI.ViewSystem
{
    /// <summary>
    /// The view handler for <see cref="PrefabLessView"/>.
    /// </summary>
    public sealed class PrefabLessViewHandler : ViewHandler
    {
        /// <summary>
        /// Gets the single instance.
        /// </summary>
        public static PrefabLessViewHandler Instance { get; } = new();

        private PrefabLessViewHandler()
        {
        }

        /// <inheritdoc />
        public override Type HandledViewType => typeof(PrefabLessView);

        /// <inheritdoc />
        public override Task<T> CreateInactiveOrDisabledViewAsync<T>(CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<T>(cancellationToken);
            }
            var gameObject = new GameObject(typeof(T).Name);
            gameObject.SetActive(false);
            var t = gameObject.AddComponent<T>();
            return Task.FromResult(t);
        }
    }
}
