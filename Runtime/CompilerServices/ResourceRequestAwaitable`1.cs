using System;
using System.Threading;
using Aurora.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity.CompilerServices
{
    /// <summary>
    /// Provides an awaitable context for switching to when resource loading finishes.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    public readonly struct ResourceRequestAwaitable<T> : IAwaitable<T> where T : Object
    {
        private readonly ResourceRequest _resourceRequest;

        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceRequestAwaitable"/> struct.
        /// </summary>
        /// <param name="resourceRequest">The request that asynchronously loads a resource.</param>
        public ResourceRequestAwaitable(ResourceRequest resourceRequest)
        {
            _resourceRequest   = resourceRequest;
            _cancellationToken = CancellationToken.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceRequestAwaitable"/> struct.
        /// </summary>
        /// <param name="resourceRequest">The request that asynchronously loads a resource.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public ResourceRequestAwaitable(ResourceRequest resourceRequest, CancellationToken cancellationToken)
        {
            _resourceRequest   = resourceRequest;
            _cancellationToken = cancellationToken;
        }

        /// <inheritdoc />
        public IAwaiter<T> GetAwaiter()
        {
            return new Awaiter(_resourceRequest, _cancellationToken);
        }

        internal readonly struct Awaiter : IAwaiter<T>
        {
            private readonly ResourceRequest _resourceRequest;

            private readonly AsyncOperationAwaitable.Awaiter _awaiter;

            internal Awaiter(ResourceRequest resourceRequest, CancellationToken cancellationToken)
            {
                _resourceRequest = resourceRequest;
                _awaiter         = new AsyncOperationAwaitable.Awaiter(resourceRequest, cancellationToken);
            }

            /// <inheritdoc />
            public bool IsCompleted => _awaiter.IsCompleted;

            /// <inheritdoc />
            public void OnCompleted(Action continuation)
            {
                _awaiter.OnCompleted(continuation);
            }

            /// <inheritdoc />
            public void UnsafeOnCompleted(Action continuation)
            {
                _awaiter.UnsafeOnCompleted(continuation);
            }

            /// <inheritdoc />
            public T GetResult()
            {
                _awaiter.GetResult();
                return (T)_resourceRequest?.asset;
            }
        }
    }
}
