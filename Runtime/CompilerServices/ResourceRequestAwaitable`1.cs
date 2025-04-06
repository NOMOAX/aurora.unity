using System;
using System.Threading;
using Aurora.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity.CompilerServices
{
    /// <summary>
    /// 提供用于切换到加载资源完毕时的可等待上下文。
    /// </summary>
    /// <typeparam name="T">资源类型。</typeparam>
    public readonly struct ResourceRequestAwaitable<T> : IAwaitable<T> where T : Object
    {
        private readonly ResourceRequest _resourceRequest;

        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// 初始化 <see cref="ResourceRequestAwaitable"/> 结构的新实例。
        /// </summary>
        /// <param name="resourceRequest">异步加载资源的请求。</param>
        public ResourceRequestAwaitable(ResourceRequest resourceRequest)
        {
            _resourceRequest   = resourceRequest;
            _cancellationToken = CancellationToken.None;
        }

        /// <summary>
        /// 初始化 <see cref="ResourceRequestAwaitable"/> 结构的新实例。
        /// </summary>
        /// <param name="resourceRequest">异步加载资源的请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
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
                return (T) _resourceRequest?.asset;
            }
        }
    }
}
