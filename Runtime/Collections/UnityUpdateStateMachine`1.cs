using System;
using Aurora.Collections;

namespace Aurora.Unity.Collections
{
    /// <summary>
    /// A finite state machine that executes a custom action during Unity frame update.
    /// </summary>
    /// <inheritdoc path="/typeparam"/>
    /// <inheritdoc path="/remarks"/>
    /// <example>
    /// The following state executes custom logic each frame while it is the current state:
    /// <code>
    /// public sealed class IdleState : IUnityUpdateState&lt;Type&gt;
    /// {
    ///     public Type Id => typeof(IdleState);
    ///     public void OnEnter(StateMachine&lt;Type&gt; stateMachine, IState&lt;Type&gt; from)
    ///     {
    ///         // ...
    ///     }
    ///     public void OnExit(StateMachine&lt;Type&gt; stateMachine, IState&lt;Type&gt; to)
    ///     {
    ///         // ...
    ///     }
    ///     public void OnUnityUpdate(UnityUpdateStateMachine&lt;Type&gt; stateMachine)
    ///     {
    ///         // Runs once per frame while this state is the current state.
    ///         // var deltaTime = Time.deltaTime;
    ///         // ...
    ///     }
    /// }
    /// </code>
    /// The owner drives the state machine from its <c>Update</c> method. First, repeatedly call <see cref="StateMachine{T}.Update"/> until it returns <see langword="false"/> to perform any pending state transitions; then call <see cref="UnityUpdate"/> once:
    /// <code>
    /// private UnityUpdateStateMachine&lt;Type&gt; _stateMachine;
    /// private void OnEnable()
    /// {
    ///     _stateMachine = new UnityUpdateStateMachine&lt;Type&gt;();
    ///     _stateMachine.AddState(new IdleState());
    ///     _stateMachine.ScheduleTransitionTo(typeof(IdleState));
    /// }
    /// private void OnDisable()
    /// {
    ///     _stateMachine = null;
    /// }
    /// private void Update()
    /// {
    ///     while (_stateMachine.Update())
    ///     {
    ///     }
    ///     _stateMachine.UnityUpdate();
    /// }
    /// </code>
    /// </example>
    public class UnityUpdateStateMachine<T> : StateMachine<T>
    {
        /// <summary>
        /// <see cref="StateMachine{T}.CurrentState"/> as <see cref="IUnityUpdateState{T}"/>.
        /// </summary>
        /// <remarks>Returns <see langword="null"/> if the current state does not implement <see cref="IUnityUpdateState{T}"/>.</remarks>
        public IUnityUpdateState<T> CurrentUnityUpdateState => CurrentState as IUnityUpdateState<T>;

        /// <summary>
        /// Executes custom logic during Unity frame update.
        /// </summary>
        /// <exception cref="InvalidOperationException">The finite state machine is entering or exiting a state.</exception>
        /// <remarks>At some point each frame, repeatedly call <see cref="StateMachine{T}.Update"/> until it returns <see langword="false"/>, then call this method once.</remarks>
        public void UnityUpdate()
        {
            ThrowIfEnteringOrExiting();
            if (CurrentUnityUpdateState is { } currentUnityUpdateState)
            {
                DoUnityUpdate(currentUnityUpdateState);
            }
        }

        /// <summary>
        /// Executes the specified state's custom logic during Unity frame update.
        /// </summary>
        /// <param name="currentUnityUpdateState"><see cref="CurrentUnityUpdateState"/>.</param>
        /// <exception cref="ArgumentNullException"><see cref="currentUnityUpdateState"/> is <see langword="null"/>.</exception>
        /// <remarks>Override this method to execute custom logic before or after the current state's <see cref="IUnityUpdateState{T}.OnUnityUpdate"/>.</remarks>
        protected virtual void DoUnityUpdate(IUnityUpdateState<T> currentUnityUpdateState)
        {
            if (currentUnityUpdateState == null)
            {
                throw new ArgumentNullException(nameof(currentUnityUpdateState));
            }
            currentUnityUpdateState.OnUnityUpdate(this);
        }
    }
}
