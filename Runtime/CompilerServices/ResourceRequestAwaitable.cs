using System.Threading;
using Aurora.CompilerServices;
using UnityEngine;

namespace Aurora.Unity.CompilerServices
{
    /// <summary>
    /// Provides an awaitable context for switching to when resource loading finishes.
    /// </summary>
    public readonly struct ResourceRequestAwaitable : IAwaitable<Object>
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
        public IAwaiter<Object> GetAwaiter()
        {
            return new ResourceRequestAwaitable<Object>.Awaiter(_resourceRequest, _cancellationToken);
        }
    }
}
