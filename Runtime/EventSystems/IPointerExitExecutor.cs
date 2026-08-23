using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// This interface provides members similar to the <see cref="IPointerExitHandler"/> interface.
    /// </summary>
    public interface IPointerExitExecutor : IEventSystemExecutor
    {
        /// <summary>
        /// Corresponds to <see cref="IPointerExitHandler.OnPointerExit"/>.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="eventData">The pointer event data.</param>
        void OnPointerExit(object sender, PointerEventData eventData);
    }
}
