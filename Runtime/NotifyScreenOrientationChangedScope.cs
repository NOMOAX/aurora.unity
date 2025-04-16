using System;
using System.Runtime.CompilerServices;
using Aurora.Unity.PlayerLoop;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 通知屏幕方向更改范围。
    /// </summary>
    public sealed class NotifyScreenOrientationChangedScope : IPlayerLoopItem, IDisposable
    {
        private volatile bool _disposed;

        private Action<ScreenOrientation> _callback;

        private readonly PlayerLoopPhase _playerLoopPhase;

        private ScreenOrientation _screenOrientation;

        private bool _delayGetScreenOrientation;

        /// <summary>
        /// 初始化 <see cref="NotifyScreenOrientationChangedScope"/> 类的新实例。
        /// </summary>
        /// <param name="callback">当屏幕方向更改时执行的方法。</param>
        /// <param name="playerLoopPhase">播放器循环阶段。</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="playerLoopPhase"/> 的值未定义。</exception>
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
        /// 初始化 <see cref="NotifyScreenOrientationChangedScope"/> 类的新实例。
        /// </summary>
        /// <param name="callback">当屏幕方向更改时执行的方法。</param>
        /// <param name="screenOrientation">屏幕方向。</param>
        /// <param name="playerLoopPhase">播放器循环阶段。</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="screenOrientation"/> 不是基本方向（<see cref="ScreenOrientation.Portrait"/>、<see cref="ScreenOrientation.PortraitUpsideDown"/>、<see cref="ScreenOrientation.LandscapeLeft"/>、<see cref="ScreenOrientation.LandscapeRight"/>），或者 <paramref name="playerLoopPhase"/> 的值未定义。</exception>
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
#if UNITY_EDITOR
                if (!PlayerLoopUtility.IsClearing)
#endif
                {
                    PlayerLoopUtility.RemovePlayerLoopItem(this, _playerLoopPhase);
                }
            }
            _callback = null;
            _disposed = true;
        }
    }
}
