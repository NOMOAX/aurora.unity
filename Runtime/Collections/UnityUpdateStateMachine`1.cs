using System;
using Aurora.Collections;

namespace Aurora.Unity.Collections
{
    /// <inheritdoc />
    /// <summary>
    /// 在 Unity 帧更新时执行自定义操作的有限状态机。
    /// </summary>
    /// <remarks>只接受 <see cref="IUnityUpdateState{T}"/> 类型的状态。</remarks>
    public class UnityUpdateStateMachine<T> : StateMachine<T>
    {
        /// <summary>
        /// 如果 <see cref="state"/> 不是 <see cref="IUnityUpdateState{T}"/>，则抛出 <see cref="ArgumentException"/>。
        /// </summary>
        /// <param name="state">状态。</param>
        /// <exception cref="ArgumentException"><see cref="state"/> 不是 <see cref="IUnityUpdateState{T}"/>。</exception>
        protected override void ThrowIfRejectState(IState<T> state)
        {
            if (state is IUnityUpdateState<T>)
            {
                return;
            }
            throw new ArgumentException(null, nameof(state));
        }

        /// <summary>
        /// 在 Unity 帧更新时执行自定义逻辑。
        /// </summary>
        /// <exception cref="InvalidOperationException">有限状态机正在进入或退出状态。</exception>
        /// <remarks>在帧更新时调用一次。</remarks>
        public void UnityUpdate()
        {
            ThrowIfEnteringOrExiting();
            if (CurrentState is IUnityUpdateState<T> unityUpdateState)
            {
                unityUpdateState.OnUnityUpdate(this);
            }
        }
    }
}
