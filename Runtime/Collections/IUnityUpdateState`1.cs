using System;
using Aurora.Collections;

namespace Aurora.Unity.Collections
{
    /// <inheritdoc />
    /// <summary>
    /// 在 Unity 帧更新时执行自定义操作的状态。
    /// </summary>
    /// <remarks>可用于 <see cref="UnityUpdateStateMachine{T}"/> 类型的有限状态机。</remarks>
    public interface IUnityUpdateState<T> : IState<T>
    {
        /// <summary>
        /// 有限状态机在 Unity 帧更新时执行自定义逻辑。
        /// </summary>
        /// <param name="stateMachine">有限状态机。</param>
        /// <exception cref="NotSupportedException">这个状态不是可持续状态。换言之，它总是在 <see cref="IState{T}.OnEnter"/> 中调用 <see cref="StateMachine{T}.TransitionToDuringNextUpdate"/> 或 <see cref="StateMachine{T}.TransitionToNullDuringNextUpdate"/>。</exception>
        /// <remarks>如果这个状态不是可持续状态，请显式地抛出 <see cref="NotSupportedException"/>。</remarks>
        void OnUnityUpdate(UnityUpdateStateMachine<T> stateMachine);
    }
}
