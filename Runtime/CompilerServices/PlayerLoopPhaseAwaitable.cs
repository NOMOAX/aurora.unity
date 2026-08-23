using System;
using System.Threading;
using Aurora.CompilerServices;
using Aurora.Unity.PlayerLoop;

namespace Aurora.Unity.CompilerServices
{
    /// <summary>
    /// Provides an awaitable context for switching to a target <see cref="PlayerLoopPhase"/>.
    /// </summary>
    public readonly struct PlayerLoopPhaseAwaitable : IAwaitable
    {
        private readonly PlayerLoopPhase _playerLoopPhase;

        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerLoopPhaseAwaitable"/> struct.
        /// </summary>
        /// <param name="playerLoopPhase">The player loop phase.</param>
        public PlayerLoopPhaseAwaitable(PlayerLoopPhase playerLoopPhase)
        {
            _playerLoopPhase   = playerLoopPhase;
            _cancellationToken = CancellationToken.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerLoopPhaseAwaitable"/> struct.
        /// </summary>
        /// <param name="playerLoopPhase">The player loop phase.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
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
        /// Provides an awaitable context for switching to the earliest-executing one among multiple <see cref="PlayerLoopPhase"/>.
        /// </summary>
        public readonly struct Any : IAwaitable
        {
            private readonly PlayerLoopPhase[] _playerLoopPhases;

            private readonly CancellationToken _cancellationToken;

            /// <summary>
            /// Initializes a new instance of the <see cref="Any"/> struct.
            /// </summary>
            /// <param name="playerLoopPhases">Multiple player loop phases.</param>
            /// <exception cref="playerLoopPhases"><paramref name="playerLoopPhases"/> is <see langword="null"/>.</exception>
            /// <exception cref="playerLoopPhases">The length of <paramref name="playerLoopPhases"/> is 0.</exception>
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
            /// Initializes a new instance of the <see cref="Any"/> struct.
            /// </summary>
            /// <param name="playerLoopPhases">Multiple player loop phases.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
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
