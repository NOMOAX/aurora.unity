using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// This interface provides members similar to the <see cref="IUpdateSelectedHandler"/> interface.
    /// </summary>
    public interface IUpdateSelectedExecutor : IEventSystemExecutor
    {
        /// <summary>
        /// Corresponds to <see cref="IUpdateSelectedHandler.OnUpdateSelected"/>.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="eventData">The pointer event data.</param>
        void OnUpdateSelected(object sender, BaseEventData eventData);
    }
}
