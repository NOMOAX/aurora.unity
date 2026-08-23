using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Forwards an event to the parent.
    /// </summary>
    /// <typeparam name="T">The event handler.</typeparam>
    public abstract class SendToParentEventSystemHandler<T> : MonoBehaviour where T : IEventSystemHandler
    {
        /// <summary>
        /// The callback that handles the event.
        /// </summary>
        protected abstract ExecuteEvents.EventFunction<T> CallbackEventFunction { get; }

        /// <summary>
        /// Forwards an event to the parent.
        /// </summary>
        /// <param name="eventData">The event data.</param>
        protected void SendToParent(BaseEventData eventData)
        {
            if (transform.parent)
            {
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, CallbackEventFunction);
            }
        }
    }
}
