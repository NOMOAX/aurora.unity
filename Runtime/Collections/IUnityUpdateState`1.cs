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
        void OnUnityUpdate(UnityUpdateStateMachine<T> stateMachine);
    }
}
