using System;
using System.Runtime.CompilerServices;
using Aurora.Unity.PlayerLoop;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// A scope that notifies about screen orientation changes.
    /// </summary>
    public sealed class NotifyScreenOrientationChangedScope : IPlayerLoopItem, IDisposable
    {
        private volatile bool _disposed;

        private Action<ScreenOrientation> _callback;

        private readonly PlayerLoopPhase _playerLoopPhase;

        private ScreenOrientation _screenOrientation;

        private bool _delayGetScreenOrientation;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyScreenOrientationChangedScope"/> class.
        /// </summary>
        /// <param name="callback">The method to execute when the screen orientation changes.</param>
        /// <param name="playerLoopPhase">The player loop phase.</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="playerLoopPhase"/> is not a member defined in the <see cref="PlayerLoopPhase"/> enum.</exception>
        public NotifyScreenOrientationChangedScope(Action<ScreenOrientation> callback, PlayerLoopPhase playerLoopPhase)
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
                _screenOrientation         = Screen.orientation;
                _delayGetScreenOrientation = false;
            }
            else
            {
                _delayGetScreenOrientation = true;
            }
            PlayerLoopUtility.AddPlayerLoopItem(this, _playerLoopPhase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotifyScreenOrientationChangedScope"/> class.
        /// </summary>
        /// <param name="callback">The method to execute when the screen orientation changes.</param>
        /// <param name="screenOrientation">The screen orientation.</param>
        /// <param name="playerLoopPhase">The player loop phase.</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="screenOrientation"/> is not a base orientation (<see cref="ScreenOrientation.Portrait"/>, <see cref="ScreenOrientation.PortraitUpsideDown"/>, <see cref="ScreenOrientation.LandscapeLeft"/>, <see cref="ScreenOrientation.LandscapeRight"/>), or <paramref name="playerLoopPhase"/>'s value is undefined.</exception>
        public NotifyScreenOrientationChangedScope(
            Action<ScreenOrientation> callback,
            ScreenOrientation         screenOrientation,
            PlayerLoopPhase           playerLoopPhase)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            if (!IsBaseScreenOrientation(screenOrientation))
            {
                throw new ArgumentOutOfRangeException(nameof(screenOrientation));
            }
            if (!EnumUtility<PlayerLoopPhase>.IsDefined(playerLoopPhase))
            {
                throw new ArgumentOutOfRangeException(nameof(playerLoopPhase));
            }
            _callback                  = callback;
            _playerLoopPhase           = playerLoopPhase;
            _screenOrientation         = screenOrientation;
            _delayGetScreenOrientation = false;
            PlayerLoopUtility.AddPlayerLoopItem(this, _playerLoopPhase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsBaseScreenOrientation(ScreenOrientation screenOrientation)
        {
            return screenOrientation switch
            {
                ScreenOrientation.Portrait           => true,
                ScreenOrientation.PortraitUpsideDown => true,
                ScreenOrientation.LandscapeLeft      => true,
                ScreenOrientation.LandscapeRight     => true,
                _                                    => false
            };
        }

        void IPlayerLoopItem.Run(PlayerLoopPhase playerLoopPhase)
        {
            if (_disposed)
            {
                return;
            }
            var screenOrientation = Screen.orientation;
            if (_delayGetScreenOrientation)
            {
                _delayGetScreenOrientation = false;
                _screenOrientation         = screenOrientation;
            }
            else if (_screenOrientation != screenOrientation)
            {
                _screenOrientation = screenOrientation;
                _callback(_screenOrientation);
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~NotifyScreenOrientationChangedScope()
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
