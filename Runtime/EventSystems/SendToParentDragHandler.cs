using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Forwards <see cref="IDragHandler.OnDrag"/> to the parent.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentDragHandler : SendToParentEventSystemHandler<IDragHandler>, IDragHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<IDragHandler> CallbackEventFunction => ExecuteEvents.dragHandler;

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
