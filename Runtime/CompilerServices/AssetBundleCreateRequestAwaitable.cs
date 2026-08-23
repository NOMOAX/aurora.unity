using System;
using System.Threading;
using Aurora.CompilerServices;
using UnityEngine;

namespace Aurora.Unity.CompilerServices
{
    /// <summary>
    /// Provides an awaitable context for switching to when asset bundle creation finishes.
    /// </summary>
    public readonly struct AssetBundleCreateRequestAwaitable : IAwaitable<AssetBundle>
    {
        private readonly AssetBundleCreateRequest _assetBundleCreateRequest;

        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetBundleCreateRequestAwaitable"/> struct.
        /// </summary>
        /// <param name="assetBundleCreateRequest">The request that asynchronously creates an asset bundle.</param>
        public AssetBundleCreateRequestAwaitable(AssetBundleCreateRequest assetBundleCreateRequest)
        {
            _assetBundleCreateRequest = assetBundleCreateRequest;
            _cancellationToken        = CancellationToken.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetBundleCreateRequestAwaitable"/> struct.
        /// </summary>
        /// <param name="assetBundleCreateRequest">The request that asynchronously creates an asset bundle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public AssetBundleCreateRequestAwaitable(
            AssetBundleCreateRequest assetBundleCreateRequest,
            CancellationToken        cancellationToken)
        {
            _assetBundleCreateRequest = assetBundleCreateRequest;
            _cancellationToken        = cancellationToken;
        }

        /// <inheritdoc />
        public IAwaiter<AssetBundle> GetAwaiter()
        {
            return new Awaiter(_assetBundleCreateRequest, _cancellationToken);
        }

        private readonly struct Awaiter : IAwaiter<AssetBundle>
        {
            private readonly AssetBundleCreateRequest _assetBundleCreateRequest;

            private readonly AsyncOperationAwaitable.Awaiter _awaiter;

            internal Awaiter(AssetBundleCreateRequest assetBundleCreateRequest, CancellationToken cancellationToken)
            {
                _assetBundleCreateRequest = assetBundleCreateRequest;
                _awaiter = new AsyncOperationAwaitable.Awaiter(assetBundleCreateRequest, cancellationToken);
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
            public AssetBundle GetResult()
            {
                _awaiter.GetResult();
                return _assetBundleCreateRequest?.assetBundle;
            }
        }
    }
}
