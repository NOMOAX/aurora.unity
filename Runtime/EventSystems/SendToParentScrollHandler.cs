using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Forwards <see cref="IScrollHandler.OnScroll"/> to the parent.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentScrollHandler : SendToParentEventSystemHandler<IScrollHandler>, IScrollHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<IScrollHandler> CallbackEventFunction =>
            ExecuteEvents.scrollHandler;

        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
