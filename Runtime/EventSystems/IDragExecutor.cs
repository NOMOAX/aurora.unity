using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// This interface provides members similar to the <see cref="IDragHandler"/> interface.
    /// </summary>
    public interface IDragExecutor : IEventSystemExecutor
    {
        /// <summary>
        /// Corresponds to <see cref="IDragHandler.OnDrag"/>.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="eventData">The pointer event data.</param>
        void OnDrag(object sender, PointerEventData eventData);
    }
}
