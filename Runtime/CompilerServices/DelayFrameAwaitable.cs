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
    /// Provides an awaitable context for switching to several frames later.
    /// </summary>
    public readonly struct DelayFrameAwaitable : IAwaitable
    {
        private readonly int _delayFrameCount;

        private readonly PlayerLoopPhase _playerLoopPhase;

        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayFrameAwaitable"/> struct.
        /// </summary>
        /// <param name="delayFrameCount">The number of frames to wait.</param>
        /// <param name="playerLoopPhase">The player loop phase.</param>
        public DelayFrameAwaitable(int delayFrameCount, PlayerLoopPhase playerLoopPhase)
        {
            _delayFrameCount   = delayFrameCount;
            _playerLoopPhase   = playerLoopPhase;
            _cancellationToken = CancellationToken.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayFrameAwaitable"/> struct.
        /// </summary>
        /// <param name="delayFrameCount">The number of frames to wait.</param>
        /// <param name="playerLoopPhase">The player loop phase.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
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
            private static readonly Action<Task, object> RunAction = (_, state) => ((Action)state)();

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
