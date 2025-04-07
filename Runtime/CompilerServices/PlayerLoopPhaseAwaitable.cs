using System;
using System.Threading;
using Aurora.CompilerServices;
using Aurora.Unity.PlayerLoop;

namespace Aurora.Unity.CompilerServices
{
    /// <summary>
    /// 提供用于切换到目标 <see cref="PlayerLoopPhase"/> 的可等待上下文。
    /// </summary>
    public readonly struct PlayerLoopPhaseAwaitable : IAwaitable
    {
        private readonly PlayerLoopPhase _playerLoopPhase;

        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// 初始化 <see cref="PlayerLoopPhaseAwaitable"/> 结构的新实例。
        /// </summary>
        /// <param name="playerLoopPhase">播放器循环阶段。</param>
        public PlayerLoopPhaseAwaitable(PlayerLoopPhase playerLoopPhase)
        {
            _playerLoopPhase   = playerLoopPhase;
            _cancellationToken = CancellationToken.None;
        }

        /// <summary>
        /// 初始化 <see cref="PlayerLoopPhaseAwaitable"/> 结构的新实例。
        /// </summary>
        /// <param name="playerLoopPhase">播放器循环阶段。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        public PlayerLoopPhaseAwaitable(PlayerLoopPhase playerLoopPhase, CancellationToken cancellationToken)
        {
            _playerLoopPhase   = playerLoopPhase;
            _cancellationToken = cancellationToken;
        }

        /// <inheritdoc />
        public IAwaiter GetAwaiter()
        {
            return new Awaiter(_playerLoopPhase, _cancellationToken);
        }

        private readonly struct Awaiter : IAwaiter
        {
            private readonly PlayerLoopPhase _playerLoopPhase;

            private readonly CancellationToken _cancellationToken;

            internal Awaiter(PlayerLoopPhase playerLoopPhase, CancellationToken cancellationToken)
            {
                _playerLoopPhase   = playerLoopPhase;
                _cancellationToken = cancellationToken;
            }

            /// <inheritdoc />
            public bool IsCompleted => InternalIsCompleted;

            private bool InternalIsCompleted => IsCanceled || OnTargetPlayerLoopPhase;

            private bool IsCanceled => _cancellationToken.IsCancellationRequested;

            private bool OnTargetPlayerLoopPhase => _playerLoopPhase == PlayerLoopUtility.CurrentPhase;

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
                _cancellationToken.ThrowIfCancellationRequested();
            }

            private void InternalOnCompleted(Action continuation)
            {
                if (InternalIsCompleted)
                {
                    continuation();
                }
                else
                {
                    PlayerLoopUtility.AddContinuation(continuation, _playerLoopPhase);
                }
            }
        }

        /// <summary>
        /// 提供用于切换到多个 <see cref="PlayerLoopPhase"/> 中的最早执行的那一个的可等待上下文。
        /// </summary>
        public readonly struct Any : IAwaitable
        {
            private readonly PlayerLoopPhase[] _playerLoopPhases;

            private readonly CancellationToken _cancellationToken;

            /// <summary>
            /// 初始化 <see cref="Any"/> 结构的新实例。
            /// </summary>
            /// <param name="playerLoopPhases">多个播放器循环阶段。</param>
            /// <exception cref="playerLoopPhases"><paramref name="playerLoopPhases"/> 为 <see langword="null"/>。</exception>
            /// <exception cref="playerLoopPhases"><paramref name="playerLoopPhases"/> 的长度为 0。</exception>
            public Any(PlayerLoopPhase[] playerLoopPhases)
            {
                if (playerLoopPhases == null)
                {
                    throw new ArgumentNullException(nameof(playerLoopPhases));
                }
                if (playerLoopPhases.Length == 0)
                {
                    throw new ArgumentException();
                }
                _playerLoopPhases  = playerLoopPhases;
                _cancellationToken = CancellationToken.None;
            }

            /// <summary>
            /// 初始化 <see cref="Any"/> 结构的新实例。
            /// </summary>
            /// <param name="playerLoopPhases">多个播放器循环阶段。</param>
            /// <param name="cancellationToken">取消令牌。</param>
            public Any(PlayerLoopPhase[] playerLoopPhases, CancellationToken cancellationToken)
            {
                if (playerLoopPhases == null)
                {
                    throw new ArgumentNullException(nameof(playerLoopPhases));
                }
                if (playerLoopPhases.Length == 0)
                {
                    throw new ArgumentException();
                }
                _playerLoopPhases  = playerLoopPhases;
                _cancellationToken = cancellationToken;
            }

            /// <inheritdoc />
            public IAwaiter GetAwaiter()
            {
                return new AnyAwaiter(_playerLoopPhases, _cancellationToken);
            }
        }

        private readonly struct AnyAwaiter : IAwaiter
        {
            private readonly PlayerLoopPhase[] _playerLoopPhases;

            private readonly CancellationToken _cancellationToken;

            internal AnyAwaiter(PlayerLoopPhase[] playerLoopPhases, CancellationToken cancellationToken)
            {
                _playerLoopPhases  = playerLoopPhases;
                _cancellationToken = cancellationToken;
            }

            /// <inheritdoc />
            public bool IsCompleted => InternalIsCompleted;

            private bool InternalIsCompleted => IsCanceled || OnTargetPlayerLoopPhase;

            private bool IsCanceled => _cancellationToken.IsCancellationRequested;

            private bool OnTargetPlayerLoopPhase => PlayerLoopUtility.CurrentPhase is { } currentPlayerLoopPhase &&
                                                    Array.IndexOf(_playerLoopPhases, currentPlayerLoopPhase) >= 0;

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
                _cancellationToken.ThrowIfCancellationRequested();
            }

            private void InternalOnCompleted(Action continuation)
            {
                if (InternalIsCompleted)
                {
                    continuation();
                }
                else
                {
                    var continuationInvocation = new OneTimeInvocation(new InvocationAction(continuation));
                    foreach (var playerLoopPhase in _playerLoopPhases)
                    {
                        PlayerLoopUtility.AddContinuation(continuationInvocation, playerLoopPhase);
                    }
                }
            }
        }
    }
}
