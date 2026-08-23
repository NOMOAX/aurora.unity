using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// This interface provides members similar to the <see cref="IBeginDragHandler"/> interface.
    /// </summary>
    public interface IBeginDragExecutor : IEventSystemExecutor
    {
        /// <summary>
        /// Corresponds to <see cref="IBeginDragHandler.OnBeginDrag"/>.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="eventData">The pointer event data.</param>
        void OnBeginDrag(object sender, PointerEventData eventData);
    }
}
