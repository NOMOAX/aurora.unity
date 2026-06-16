using System;
using Aurora.Unity.PlayerLoop;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 通知屏幕大小更改范围。
    /// </summary>
    public sealed class NotifyScreenSizeChangedScope : IPlayerLoopItem, IDisposable
    {
        private volatile bool _disposed;

        private Action<Vector2Int> _callback;

        private readonly PlayerLoopPhase _playerLoopPhase;

        private Vector2Int _screenSize;

        private bool _delayGetScreenSize;

        /// <summary>
        /// 初始化 <see cref="NotifyScreenSizeChangedScope"/> 类的新实例。
        /// </summary>
        /// <param name="callback">当屏幕大小更改时执行的方法。</param>
        /// <param name="playerLoopPhase">主循环阶段。</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="playerLoopPhase"/> 不是在 <see cref="PlayerLoopPhase"/> 枚举中定义的成员。</exception>
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
        /// 初始化 <see cref="NotifyScreenSizeChangedScope"/> 类的新实例。
        /// </summary>
        /// <param name="callback">当屏幕方向更改时执行的方法。</param>
        /// <param name="screenSize">屏幕方向。</param>
        /// <param name="playerLoopPhase">主循环阶段。</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="screenSize"/> 的任何分量为负数，或者 <paramref name="playerLoopPhase"/> 不是在 <see cref="PlayerLoopPhase"/> 枚举中定义的成员。</exception>
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
