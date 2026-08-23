using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// This interface provides members similar to the <see cref="IPointerUpHandler"/> interface.
    /// </summary>
    public interface IPointerUpExecutor : IEventSystemExecutor
    {
        /// <summary>
        /// Corresponds to <see cref="IPointerUpHandler.OnPointerUp"/>.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="eventData">The pointer event data.</param>
        void OnPointerUp(object sender, PointerEventData eventData);
    }
}
