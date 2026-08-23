using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// This interface provides members similar to the <see cref="IEndDragHandler"/> interface.
    /// </summary>
    public interface IEndDragExecutor : IEventSystemExecutor
    {
        /// <summary>
        /// Corresponds to <see cref="IEndDragHandler.OnEndDrag"/>.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="eventData">The pointer event data.</param>
        void OnEndDrag(object sender, PointerEventData eventData);
    }
}
