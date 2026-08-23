using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// This interface provides members similar to the <see cref="ISelectHandler"/> interface.
    /// </summary>
    public interface ISelectExecutor : IEventSystemExecutor
    {
        /// <summary>
        /// Corresponds to <see cref="ISelectHandler.OnSelect"/>.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="eventData">The pointer event data.</param>
        void OnSelect(object sender, BaseEventData eventData);
    }
}
