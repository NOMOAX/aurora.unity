using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// This interface provides members similar to the <see cref="IDropHandler"/> interface.
    /// </summary>
    public interface IDropExecutor : IEventSystemExecutor
    {
        /// <summary>
        /// Corresponds to <see cref="IDropHandler.OnDrop"/>.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="eventData">The pointer event data.</param>
        void OnDrop(object sender, PointerEventData eventData);
    }
}
