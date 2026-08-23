using System.Threading;
using Aurora.CompilerServices;
using UnityEngine;

namespace Aurora.Unity.CompilerServices
{
    /// <summary>
    /// Provides an awaitable context for switching to when resources are loaded from an asset bundle.
    /// </summary>
    public readonly struct AssetBundleRequestAwaitable : IAwaitable<Object>
    {
        private readonly AssetBundleRequest _assetBundleRequest;

        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetBundleRequestAwaitable"/> struct.
        /// </summary>
        /// <param name="assetBundleRequest">The request that asynchronously loads resources from an asset bundle.</param>
        public AssetBundleRequestAwaitable(AssetBundleRequest assetBundleRequest)
        {
            _assetBundleRequest = assetBundleRequest;
            _cancellationToken  = CancellationToken.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetBundleRequestAwaitable"/> struct.
        /// </summary>
        /// <param name="assetBundleRequest">The request that asynchronously loads resources from an asset bundle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public AssetBundleRequestAwaitable(AssetBundleRequest assetBundleRequest, CancellationToken cancellationToken)
        {
            _assetBundleRequest = assetBundleRequest;
            _cancellationToken  = cancellationToken;
        }

        /// <inheritdoc />
        public IAwaiter<Object> GetAwaiter()
        {
            return new AssetBundleRequestAwaitable<Object>.Awaiter(_assetBundleRequest, _cancellationToken);
        }

        /// <summary>
        /// Provides an awaitable context for switching to when all resources are loaded from an asset bundle.
        /// </summary>
        public readonly struct All : IAwaitable<Object[]>
        {
            private readonly AssetBundleRequest _assetBundleRequest;

            private readonly CancellationToken _cancellationToken;

            /// <summary>
            /// Initializes a new instance of the <see cref="AssetBundleRequestAwaitable.All"/> struct.
            /// </summary>
            /// <param name="assetBundleRequest">The request that asynchronously loads resources from an asset bundle.</param>
            public All(AssetBundleRequest assetBundleRequest)
            {
                _assetBundleRequest = assetBundleRequest;
                _cancellationToken  = CancellationToken.None;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="AssetBundleRequestAwaitable.All"/> struct.
            /// </summary>
            /// <param name="assetBundleRequest">The request that asynchronously loads resources from an asset bundle.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            public All(AssetBundleRequest assetBundleRequest, CancellationToken cancellationToken)
            {
                _assetBundleRequest = assetBundleRequest;
                _cancellationToken  = cancellationToken;
            }

            /// <inheritdoc />
            public IAwaiter<Object[]> GetAwaiter()
            {
                return new AssetBundleRequestAwaitable<Object>.AllAwaiter(_assetBundleRequest, _cancellationToken);
            }
        }
    }
}
