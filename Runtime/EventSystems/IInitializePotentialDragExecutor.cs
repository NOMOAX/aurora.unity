using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// This interface provides members similar to the <see cref="IInitializePotentialDragHandler"/> interface.
    /// </summary>
    public interface IInitializePotentialDragExecutor : IEventSystemExecutor
    {
        /// <summary>
        /// Corresponds to <see cref="IInitializePotentialDragHandler.OnInitializePotentialDrag"/>.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="eventData">The pointer event data.</param>
        void OnInitializePotentialDrag(object sender, PointerEventData eventData);
    }
}
