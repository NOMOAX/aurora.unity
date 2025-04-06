using System;
using System.Threading;
using Aurora.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity.CompilerServices
{
    /// <summary>
    /// 提供用于切换到从资源包加载资源完毕时的可等待上下文。
    /// </summary>
    /// <typeparam name="T">资源类型。</typeparam>
    public readonly struct AssetBundleRequestAwaitable<T> : IAwaitable<T> where T : Object
    {
        private readonly AssetBundleRequest _assetBundleRequest;

        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// 初始化 <see cref="AssetBundleRequestAwaitable{T}"/> 结构的新实例。
        /// </summary>
        /// <param name="assetBundleRequest">异步从资源包加载资源的请求。</param>
        public AssetBundleRequestAwaitable(AssetBundleRequest assetBundleRequest)
        {
            _assetBundleRequest = assetBundleRequest;
            _cancellationToken  = CancellationToken.None;
        }

        /// <summary>
        /// 初始化 <see cref="AssetBundleRequestAwaitable{T}"/> 结构的新实例。
        /// </summary>
        /// <param name="assetBundleRequest">异步从资源包加载资源的请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
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
                return (T) _assetBundleRequest?.asset;
            }
        }

        /// <summary>
        /// 提供用于切换到从资源包加载所有资源完毕时的可等待上下文。
        /// </summary>
        public readonly struct All : IAwaitable<T[]>
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
                return (T[]) _assetBundleRequest?.allAssets;
            }
        }
    }
}
