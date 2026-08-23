using System.Threading;
using Aurora.CompilerServices;
using UnityEngine;

namespace Aurora.Unity.CompilerServices
{
    /// <summary>
    /// 提供用于切换到加载资源完毕时的可等待上下文。
    /// </summary>
    public readonly struct ResourceRequestAwaitable : IAwaitable<Object>
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
        public IAwaiter<Object> GetAwaiter()
        {
            return new ResourceRequestAwaitable<Object>.Awaiter(_resourceRequest, _cancellationToken);
        }
    }
}
