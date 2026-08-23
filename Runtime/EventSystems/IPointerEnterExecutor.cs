using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// This interface provides members similar to the <see cref="IPointerEnterHandler"/> interface.
    /// </summary>
    public interface IPointerEnterExecutor : IEventSystemExecutor
    {
        /// <summary>
        /// Corresponds to <see cref="IPointerEnterHandler.OnPointerEnter"/>.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="eventData">The pointer event data.</param>
        void OnPointerEnter(object sender, PointerEventData eventData);
    }
}
