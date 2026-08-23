using System;
using Aurora.Unity.PlayerLoop;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// A scope that notifies about screen size changes.
    /// </summary>
    public sealed class NotifyScreenSizeChangedScope : IPlayerLoopItem, IDisposable
    {
        private volatile bool _disposed;

        private Action<Vector2Int> _callback;

        private readonly PlayerLoopPhase _playerLoopPhase;

        private Vector2Int _screenSize;

        private bool _delayGetScreenSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyScreenSizeChangedScope"/> class.
        /// </summary>
        /// <param name="callback">The method to execute when the screen size changes.</param>
        /// <param name="playerLoopPhase">The player loop phase.</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="playerLoopPhase"/> is not a member defined in the <see cref="PlayerLoopPhase"/> enum.</exception>
        public NotifyScreenSizeChangedScope(Action<Vector2Int> callback, PlayerLoopPhase playerLoopPhase)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            if (!EnumUtility<PlayerLoopPhase>.IsDefined(playerLoopPhase))
            {
                throw new ArgumentOutOfRangeException(nameof(playerLoopPhase));
            }
            _callback        = callback;
            _playerLoopPhase = playerLoopPhase;
            if (UnityEnvironment.OnUnityMainThread)
            {
                _screenSize         = new Vector2Int(Screen.width, Screen.height);
                _delayGetScreenSize = false;
            }
            else
            {
                _delayGetScreenSize = true;
            }
            PlayerLoopUtility.AddPlayerLoopItem(this, _playerLoopPhase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyScreenSizeChangedScope"/> class.
        /// </summary>
        /// <param name="callback">The method to execute when the screen size changes.</param>
        /// <param name="screenSize">The screen size.</param>
        /// <param name="playerLoopPhase">The player loop phase.</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Any component of <paramref name="screenSize"/> is negative, or <paramref name="playerLoopPhase"/> is not a member defined in the <see cref="PlayerLoopPhase"/> enum.</exception>
        public NotifyScreenSizeChangedScope(
            Action<Vector2Int> callback,
            Vector2Int         screenSize,
            PlayerLoopPhase    playerLoopPhase)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            if (screenSize.x < 0 || screenSize.y < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(screenSize));
            }
            if (!EnumUtility<PlayerLoopPhase>.IsDefined(playerLoopPhase))
            {
                throw new ArgumentOutOfRangeException(nameof(playerLoopPhase));
            }
            _callback           = callback;
            _playerLoopPhase    = playerLoopPhase;
            _screenSize         = screenSize;
            _delayGetScreenSize = false;
            PlayerLoopUtility.AddPlayerLoopItem(this, _playerLoopPhase);
        }

        void IPlayerLoopItem.Run(PlayerLoopPhase playerLoopPhase)
        {
            if (_disposed)
            {
                return;
            }
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            if (_delayGetScreenSize)
            {
                _delayGetScreenSize = false;
                _screenSize         = screenSize;
            }
            else if (_screenSize != screenSize)
            {
                _screenSize = screenSize;
                _callback(_screenSize);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NotifyScreenSizeChangedScope()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                PlayerLoopUtility.RemovePlayerLoopItem(this, _playerLoopPhase);
            }
            _callback = null;
            _disposed = true;
        }
    }
}
