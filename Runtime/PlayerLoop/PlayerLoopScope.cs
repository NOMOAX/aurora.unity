using System;
using UnityEngine;

namespace Aurora.Unity.PlayerLoop
{
    /// <summary>
    /// 更新范围。
    /// </summary>
    public sealed class PlayerLoopScope : IPlayerLoopItem, IDisposable
    {
        private Invocation _invocation;

        private readonly PlayerLoopPhase _playerLoopPhase;

        /// <summary>
        /// 初始化 <see cref="PlayerLoopScope"/> 类的新实例。
        /// </summary>
        /// <param name="action">更新时执行的操作。</param>
        /// <param name="playerLoopPhase">播放器循环阶段。</param>
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
        /// 初始化 <see cref="PlayerLoopScope"/> 类的新实例。
        /// </summary>
        /// <param name="action">更新时执行的操作，它具有一个 <see cref="object"/> 类型的参数。</param>
        /// <param name="state">要传递给 <paramref name="action"/> 的参数。</param>
        /// <param name="playerLoopPhase">播放器循环阶段。</param>
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
            Debug.LogError("应在使用完之后显式地释放此实例，而不是丢给 GC 来处理");
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
