using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// This interface provides members similar to the <see cref="IMoveHandler"/> interface.
    /// </summary>
    public interface IMoveExecutor : IEventSystemExecutor
    {
        /// <summary>
        /// Corresponds to <see cref="IMoveHandler.OnMove"/>.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="eventData">The pointer event data.</param>
        void OnMove(object sender, AxisEventData eventData);
    }
}
