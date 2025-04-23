using System;
using Aurora.Collections;

namespace Aurora.Unity.Collections
{
    /// <inheritdoc />
    /// <summary>
    /// 在 Unity 帧更新时执行自定义操作的有限状态机。
    /// </summary>
    public class UnityUpdateStateMachine<T> : StateMachine<T>
    {
        /// <summary>
        /// 在 Unity 帧更新时执行自定义逻辑。
        /// </summary>
        /// <exception cref="InvalidOperationException">有限状态机正在进入或退出状态。</exception>
        /// <remarks>在每帧的某个时刻，先反复调用 <see cref="StateMachine{T}.Update"/> 直到返回值为 <see langword="false"/>，然后调用此方法一次。</remarks>
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
