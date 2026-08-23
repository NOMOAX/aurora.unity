using System;
using System.Threading;
using Aurora.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity.CompilerServices
{
    /// <summary>
    /// Provides an awaitable context for switching to when resources are loaded from an asset bundle.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    public readonly struct AssetBundleRequestAwaitable<T> : IAwaitable<T> where T : Object
    {
        private readonly AssetBundleRequest _assetBundleRequest;

        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetBundleRequestAwaitable{T}"/> struct.
        /// </summary>
        /// <param name="assetBundleRequest">The request that asynchronously loads resources from an asset bundle.</param>
        public AssetBundleRequestAwaitable(AssetBundleRequest assetBundleRequest)
        {
            _assetBundleRequest = assetBundleRequest;
            _cancellationToken  = CancellationToken.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetBundleRequestAwaitable{T}"/> struct.
        /// </summary>
        /// <param name="assetBundleRequest">The request that asynchronously loads resources from an asset bundle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public AssetBundleRequestAwaitable(AssetBundleRequest assetBundleRequest, CancellationToken cancellationToken)
        {
            _assetBundleRequest = assetBundleRequest;
            _cancellationToken  = cancellationToken;
        }

        /// <inheritdoc />
        public IAwaiter<T> GetAwaiter()
        {
            return new Awaiter(_assetBundleRequest, _cancellationToken);
        }

        internal readonly struct Awaiter : IAwaiter<T>
        {
            private readonly AssetBundleRequest _assetBundleRequest;

            private readonly AsyncOperationAwaitable.Awaiter _awaiter;

            internal Awaiter(AssetBundleRequest assetBundleRequest, CancellationToken cancellationToken)
            {
                _assetBundleRequest = assetBundleRequest;
                _awaiter            = new AsyncOperationAwaitable.Awaiter(assetBundleRequest, cancellationToken);
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
                return (T)_assetBundleRequest?.asset;
            }
        }

        /// <summary>
        /// Provides an awaitable context for switching to when all resources are loaded from an asset bundle.
        /// </summary>
        public readonly struct All : IAwaitable<T[]>
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
            public IAwaiter<T[]> GetAwaiter()
            {
                return new AllAwaiter(_assetBundleRequest, _cancellationToken);
            }
        }

        internal readonly struct AllAwaiter : IAwaiter<T[]>
        {
            private readonly AssetBundleRequest _assetBundleRequest;

            private readonly AsyncOperationAwaitable.Awaiter _awaiter;

            internal AllAwaiter(AssetBundleRequest assetBundleRequest, CancellationToken cancellationToken)
            {
                _assetBundleRequest = assetBundleRequest;
                _awaiter            = new AsyncOperationAwaitable.Awaiter(assetBundleRequest, cancellationToken);
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
            public T[] GetResult()
            {
                _awaiter.GetResult();
                return (T[])_assetBundleRequest?.allAssets;
            }
        }
    }
}
