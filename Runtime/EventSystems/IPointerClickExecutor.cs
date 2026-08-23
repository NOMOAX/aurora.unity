using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// This interface provides members similar to the <see cref="IPointerClickHandler"/> interface.
    /// </summary>
    public interface IPointerClickExecutor : IEventSystemExecutor
    {
        /// <summary>
        /// Corresponds to <see cref="IPointerClickHandler.OnPointerClick"/>.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="eventData">The pointer event data.</param>
        void OnPointerClick(object sender, PointerEventData eventData);
    }
}
