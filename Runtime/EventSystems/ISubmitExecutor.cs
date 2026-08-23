using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// This interface provides members similar to the <see cref="ISubmitHandler"/> interface.
    /// </summary>
    public interface ISubmitExecutor : IEventSystemExecutor
    {
        /// <summary>
        /// Corresponds to <see cref="ISubmitHandler.OnSubmit"/>.
        /// </summary>
        /// <param name="sender">The caller.</param>
        /// <param name="eventData">The pointer event data.</param>
        void OnSubmit(object sender, BaseEventData eventData);
    }
}
