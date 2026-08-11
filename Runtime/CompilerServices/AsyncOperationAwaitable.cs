using System;
using System.Threading;
using System.Threading.Tasks;
using Aurora.CompilerServices;
using Aurora.Threading;
using Aurora.Unity.Threading.Tasks;
using UnityEngine;

namespace Aurora.Unity.CompilerServices
{
    /// <summary>
    /// 提供用于切换到目标 <see cref="AsyncOperation"/> 执行完毕时的可等待上下文。
    /// </summary>
    public readonly struct AsyncOperationAwaitable : IAwaitable
    {
        private readonly AsyncOperation _asyncOperation;

        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// 初始化 <see cref="AsyncOperationAwaitable"/> 结构的新实例。
        /// </summary>
        /// <param name="asyncOperation">Unity 异步操作。</param>
        public AsyncOperationAwaitable(AsyncOperation asyncOperation)
        {
            _asyncOperation    = asyncOperation;
            _cancellationToken = CancellationToken.None;
        }

        /// <summary>
        /// 初始化 <see cref="AsyncOperationAwaitable"/> 结构的新实例。
        /// </summary>
        /// <param name="asyncOperation">Unity 异步操作。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        public AsyncOperationAwaitable(AsyncOperation asyncOperation, CancellationToken cancellationToken)
        {
            _asyncOperation    = asyncOperation;
            _cancellationToken = cancellationToken;
        }

        /// <inheritdoc />
        public IAwaiter GetAwaiter()
        {
            return new Awaiter(_asyncOperation, _cancellationToken);
        }

        internal readonly struct Awaiter : IAwaiter
        {
            private static readonly Action<Task, object> RunAction = (_, state) => ((Action)state)();

            private readonly Task _task;

            internal Awaiter(AsyncOperation asyncOperation, CancellationToken cancellationToken)
            {
                _task = UnityTasks.WhenAsyncOperation(asyncOperation, cancellationToken);
            }

            /// <inheritdoc />
            public bool IsCompleted => InternalIsCompleted;

            private bool InternalIsCompleted => _task is null || _task.IsCompleted;

            /// <inheritdoc />
            public void OnCompleted(Action continuation)
            {
                InternalOnCompleted(continuation);
            }

            /// <inheritdoc />
            public void UnsafeOnCompleted(Action continuation)
            {
                InternalOnCompleted(continuation);
            }

            /// <inheritdoc />
            public void GetResult()
            {
                if (_task != null)
                {
                    TaskUtility.ThrowIfFaultedOrCanceled(_task);
                }
            }

            private void InternalOnCompleted(Action continuation)
            {
                if (InternalIsCompleted)
                {
                    continuation();
                }
                else
                {
                    TaskUtility.ContinueWithSynchronously(_task, RunAction, continuation);
                }
            }
        }
    }
}
