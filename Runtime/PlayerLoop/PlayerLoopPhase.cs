namespace Aurora.Unity.PlayerLoop
{
    /// <summary>
    /// Unity player loop phases.
    /// </summary>
    public enum PlayerLoopPhase : byte
    {
        /// <summary>
        /// Before <see cref="UnityEngine.PlayerLoop.FixedUpdate.ScriptRunBehaviourFixedUpdate"/>.
        /// </summary>
        FixedUpdating,

        /// <summary>
        /// After <see cref="UnityEngine.PlayerLoop.FixedUpdate.ScriptRunBehaviourFixedUpdate"/>.
        /// </summary>
        FixedUpdated,

        /// <summary>
        /// Before <see cref="UnityEngine.PlayerLoop.Update.ScriptRunBehaviourUpdate"/>.
        /// </summary>
        Updating,

        /// <summary>
        /// After <see cref="UnityEngine.PlayerLoop.Update.ScriptRunBehaviourUpdate"/> and before <see cref="UnityEngine.PlayerLoop.Update.ScriptRunDelayedDynamicFrameRate"/>.
        /// </summary>
        Updated,

        /// <summary>
        /// After <see cref="UnityEngine.PlayerLoop.Update.ScriptRunDelayedDynamicFrameRate"/> and before <see cref="UnityEngine.PlayerLoop.Update.ScriptRunDelayedTasks"/>.
        /// </summary>
        UpdateYielded,

        /// <summary>
        /// After <see cref="UnityEngine.PlayerLoop.Update.ScriptRunDelayedTasks"/>.
        /// </summary>
        UpdatePosted,

        /// <summary>
        /// Before <see cref="UnityEngine.PlayerLoop.PreLateUpdate.ScriptRunBehaviourLateUpdate"/>.
        /// </summary>
        LateUpdating,

        /// <summary>
        /// After <see cref="UnityEngine.PlayerLoop.PreLateUpdate.ScriptRunBehaviourLateUpdate"/>.
        /// </summary>
        LateUpdated
    }
}
