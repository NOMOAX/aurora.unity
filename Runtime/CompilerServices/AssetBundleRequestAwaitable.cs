using System.Threading;
using Aurora.CompilerServices;
using UnityEngine;

namespace Aurora.Unity.CompilerServices
{
    /// <summary>
    /// 提供用于切换到从资源包加载资源完毕时的可等待上下文。
    /// </summary>
    public readonly struct AssetBundleRequestAwaitable : IAwaitable<Object>
    {
        private readonly AssetBundleRequest _assetBundleRequest;

        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// 初始化 <see cref="AssetBundleRequestAwaitable"/> 结构的新实例。
        /// </summary>
        /// <param name="assetBundleRequest">异步从资源包加载资源的请求。</param>
        public AssetBundleRequestAwaitable(AssetBundleRequest assetBundleRequest)
        {
            _assetBundleRequest = assetBundleRequest;
            _cancellationToken  = CancellationToken.None;
        }

        /// <summary>
        /// 初始化 <see cref="AssetBundleRequestAwaitable"/> 结构的新实例。
        /// </summary>
        /// <param name="assetBundleRequest">异步从资源包加载资源的请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
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
        /// 提供用于切换到从资源包加载所有资源完毕时的可等待上下文。
        /// </summary>
        public readonly struct All : IAwaitable<Object[]>
        {
            private readonly AssetBundleRequest _assetBundleRequest;

            private readonly CancellationToken _cancellationToken;

            /// <summary>
            /// 初始化 <see cref="AssetBundleRequestAwaitable.All"/> 结构的新实例。
            /// </summary>
            /// <param name="assetBundleRequest">异步从资源包加载资源的请求。</param>
            public All(AssetBundleRequest assetBundleRequest)
            {
                _assetBundleRequest = assetBundleRequest;
                _cancellationToken  = CancellationToken.None;
            }

            /// <summary>
            /// 初始化 <see cref="AssetBundleRequestAwaitable.All"/> 结构的新实例。
            /// </summary>
            /// <param name="assetBundleRequest">异步从资源包加载资源的请求。</param>
            /// <param name="cancellationToken">取消令牌。</param>
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
