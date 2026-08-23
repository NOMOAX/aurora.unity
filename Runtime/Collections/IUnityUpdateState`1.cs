using Aurora.Collections;

namespace Aurora.Unity.Collections
{
    /// <inheritdoc />
    /// <summary>
    /// A state that executes a custom action during Unity frame update.
    /// </summary>
    /// <remarks>Can be used for finite state machines of type <see cref="UnityUpdateStateMachine{T}"/>.</remarks>
    public interface IUnityUpdateState<T> : IState<T>
    {
        /// <summary>
        /// The finite state machine executes custom logic during Unity frame update.
        /// </summary>
        /// <param name="stateMachine">The finite state machine.</param>
        void OnUnityUpdate(UnityUpdateStateMachine<T> stateMachine);
    }
}
