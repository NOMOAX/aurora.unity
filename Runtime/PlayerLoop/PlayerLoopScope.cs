using System;
using UnityEngine;

namespace Aurora.Unity.PlayerLoop
{
    /// <summary>
    /// An update scope.
    /// </summary>
    public sealed class PlayerLoopScope : IPlayerLoopItem, IDisposable
    {
        private Invocation _invocation;

        private readonly PlayerLoopPhase _playerLoopPhase;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerLoopScope"/> class.
        /// </summary>
        /// <param name="action">The action to execute on update.</param>
        /// <param name="playerLoopPhase">The player loop phase.</param>
        public PlayerLoopScope(Action action, PlayerLoopPhase playerLoopPhase)
        {
            if (action != null)
            {
                _invocation = new InvocationAction(action);
                PlayerLoopUtility.AddPlayerLoopItem(this, playerLoopPhase);
            }
            _playerLoopPhase = playerLoopPhase;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerLoopScope"/> class.
        /// </summary>
        /// <param name="action">The action to execute on update; it has an <see cref="object"/> parameter.</param>
        /// <param name="state">The argument to pass to <paramref name="action"/>.</param>
        /// <param name="playerLoopPhase">The player loop phase.</param>
        public PlayerLoopScope(Action<object> action, object state, PlayerLoopPhase playerLoopPhase)
        {
            if (action != null)
            {
                _invocation = new InvocationActionWithState(action, state);
                PlayerLoopUtility.AddPlayerLoopItem(this, playerLoopPhase);
            }
            _playerLoopPhase = playerLoopPhase;
        }

        ~PlayerLoopScope()
        {
            Debug.LogError("This instance should be explicitly disposed after use instead of being left to the GC");
            InternalDispose();
        }

        void IPlayerLoopItem.Run(PlayerLoopPhase playerLoopPhase)
        {
            _invocation.Invoke();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            InternalDispose();
            GC.SuppressFinalize(this);
        }

        private void InternalDispose()
        {
            if (_invocation is null)
            {
                return;
            }
            _invocation = null;
            PlayerLoopUtility.RemovePlayerLoopItem(this, _playerLoopPhase);
        }
    }
}
