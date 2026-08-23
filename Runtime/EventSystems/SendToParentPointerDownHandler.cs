using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Forwards <see cref="IPointerDownHandler.OnPointerDown"/> to the parent.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentPointerDownHandler : SendToParentEventSystemHandler<IPointerDownHandler>,
                                                         IPointerDownHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<IPointerDownHandler> CallbackEventFunction =>
            ExecuteEvents.pointerDownHandler;

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
