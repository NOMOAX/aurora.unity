using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// This interface provides members similar to the <see cref="ICancelHandler"/> interface.
    /// </summary>
    public interface ICancelExecutor : IEventSystemExecutor
    {
        /// <summary>
        /// Corresponds to <see cref="ICancelHandler.OnCancel"/>.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="eventData">The pointer event data.</param>
        void OnCancel(object sender, BaseEventData eventData);
    }
}
