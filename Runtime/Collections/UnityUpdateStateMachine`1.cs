using System;
using Aurora.Collections;

namespace Aurora.Unity.Collections
{
    /// <inheritdoc />
    /// <summary>
    /// A finite state machine that executes a custom action during Unity frame update.
    /// </summary>
    public class UnityUpdateStateMachine<T> : StateMachine<T>
    {
        /// <summary>
        /// Executes custom logic during Unity frame update.
        /// </summary>
        /// <exception cref="InvalidOperationException">The finite state machine is entering or exiting a state.</exception>
        /// <remarks>At some point each frame, repeatedly call <see cref="StateMachine{T}.Update"/> until it returns <see langword="false"/>, then call this method once.</remarks>
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
