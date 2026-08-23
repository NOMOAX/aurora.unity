using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Forwards <see cref="IEndDragHandler.OnEndDrag"/> to the parent.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentEndDragHandler : SendToParentEventSystemHandler<IEndDragHandler>, IEndDragHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<IEndDragHandler> CallbackEventFunction =>
            ExecuteEvents.endDragHandler;

        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
