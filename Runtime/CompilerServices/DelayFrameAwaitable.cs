using System;
using System.Threading;
using System.Threading.Tasks;
using Aurora.CompilerServices;
using Aurora.Threading;
using Aurora.Unity.PlayerLoop;
using Aurora.Unity.Threading.Tasks;

namespace Aurora.Unity.CompilerServices
{
    /// <summary>
    /// 提供用于切换到几帧后的可等待上下文。
    /// </summary>
    public readonly struct DelayFrameAwaitable : IAwaitable
    {
        private readonly int _delayFrameCount;

        private readonly PlayerLoopPhase _playerLoopPhase;

        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// 初始化 <see cref="DelayFrameAwaitable"/> 结构的新实例。
        /// </summary>
        /// <param name="delayFrameCount">要等待的帧的数量。</param>
        /// <param name="playerLoopPhase">播放器循环阶段。</param>
        public DelayFrameAwaitable(int delayFrameCount, PlayerLoopPhase playerLoopPhase)
        {
            _delayFrameCount   = delayFrameCount;
            _playerLoopPhase   = playerLoopPhase;
            _cancellationToken = CancellationToken.None;
        }

        /// <summary>
        /// 初始化 <see cref="DelayFrameAwaitable"/> 结构的新实例。
        /// </summary>
        /// <param name="delayFrameCount">要等待的帧的数量。</param>
        /// <param name="playerLoopPhase">播放器循环阶段。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        public DelayFrameAwaitable(
            int               delayFrameCount,
            PlayerLoopPhase   playerLoopPhase,
            CancellationToken cancellationToken)
        {
            _delayFrameCount   = delayFrameCount;
            _playerLoopPhase   = playerLoopPhase;
            _cancellationToken = cancellationToken;
        }

        /// <inheritdoc />
        public IAwaiter GetAwaiter()
        {
            return new Awaiter(_delayFrameCount, _playerLoopPhase, _cancellationToken);
        }

        private readonly struct Awaiter : IAwaiter
        {
            private static readonly Action<Task, object> RunAction = (_, state) => ((Action) state)();

            private readonly Task _task;

            internal Awaiter(int delayFrameCount, PlayerLoopPhase playerLoopPhase, CancellationToken cancellationToken)
            {
                _task = UnityTasks.DelayFrame(delayFrameCount, playerLoopPhase, cancellationToken);
            }

            /// <inheritdoc />
            public bool IsCompleted => _task is null || _task.IsCompleted;

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
                if (_task is null || _task.IsCompleted)
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
