using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// This interface provides members similar to the <see cref="IScrollHandler"/> interface.
    /// </summary>
    public interface IScrollExecutor : IEventSystemExecutor
    {
        /// <summary>
        /// Corresponds to <see cref="IScrollHandler.OnScroll"/>.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="eventData">The pointer event data.</param>
        void OnScroll(object sender, PointerEventData eventData);
    }
}
