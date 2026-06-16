namespace Aurora.Unity.PlayerLoop
{
    /// <summary>
    /// Unity 主循环阶段。
    /// </summary>
    public enum PlayerLoopPhase : byte
    {
        /// <summary>
        /// <see cref="UnityEngine.PlayerLoop.FixedUpdate.ScriptRunBehaviourFixedUpdate"/> 之前。
        /// </summary>
        FixedUpdating,

        /// <summary>
        /// <see cref="UnityEngine.PlayerLoop.FixedUpdate.ScriptRunBehaviourFixedUpdate"/> 之后。
        /// </summary>
        FixedUpdated,

        /// <summary>
        /// <see cref="UnityEngine.PlayerLoop.Update.ScriptRunBehaviourUpdate"/> 之前。
        /// </summary>
        Updating,

        /// <summary>
        /// <see cref="UnityEngine.PlayerLoop.Update.ScriptRunBehaviourUpdate"/> 之后，<see cref="UnityEngine.PlayerLoop.Update.ScriptRunDelayedDynamicFrameRate"/> 之前。
        /// </summary>
        Updated,

        /// <summary>
        /// <see cref="UnityEngine.PlayerLoop.Update.ScriptRunDelayedDynamicFrameRate"/> 之后，<see cref="UnityEngine.PlayerLoop.Update.ScriptRunDelayedTasks"/> 之前。
        /// </summary>
        UpdateYielded,

        /// <summary>
        /// <see cref="UnityEngine.PlayerLoop.Update.ScriptRunDelayedTasks"/> 之后。
        /// </summary>
        UpdatePosted,

        /// <summary>
        /// <see cref="UnityEngine.PlayerLoop.PreLateUpdate.ScriptRunBehaviourLateUpdate"/> 之前。
        /// </summary>
        LateUpdating,

        /// <summary>
        /// <see cref="UnityEngine.PlayerLoop.PreLateUpdate.ScriptRunBehaviourLateUpdate"/> 之后。
        /// </summary>
        LateUpdated
    }
}
