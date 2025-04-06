using System;
using System.Threading;
using Aurora.CompilerServices;
using UnityEngine;

namespace Aurora.Unity.CompilerServices
{
    /// <summary>
    /// 提供用于切换到创建资源包完毕时的可等待上下文。
    /// </summary>
    public readonly struct AssetBundleCreateRequestAwaitable : IAwaitable<AssetBundle>
    {
        private readonly AssetBundleCreateRequest _assetBundleCreateRequest;

        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// 初始化 <see cref="AssetBundleCreateRequestAwaitable"/> 结构的新实例。
        /// </summary>
        /// <param name="assetBundleCreateRequest">异步创建资源包的请求。</param>
        public AssetBundleCreateRequestAwaitable(AssetBundleCreateRequest assetBundleCreateRequest)
        {
            _assetBundleCreateRequest = assetBundleCreateRequest;
            _cancellationToken        = CancellationToken.None;
        }

        /// <summary>
        /// 初始化 <see cref="AssetBundleCreateRequestAwaitable"/> 结构的新实例。
        /// </summary>
        /// <param name="assetBundleCreateRequest">异步创建资源包的请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
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
