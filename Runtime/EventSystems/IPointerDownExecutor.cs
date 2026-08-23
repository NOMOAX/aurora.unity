using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// This interface provides members similar to the <see cref="IPointerDownHandler"/> interface.
    /// </summary>
    public interface IPointerDownExecutor : IEventSystemExecutor
    {
        /// <summary>
        /// Corresponds to <see cref="IPointerDownHandler.OnPointerDown"/>.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="eventData">The pointer event data.</param>
        void OnPointerDown(object sender, PointerEventData eventData);
    }
}
