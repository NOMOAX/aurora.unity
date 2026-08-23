using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Forwards <see cref="IBeginDragHandler.OnBeginDrag"/> to the parent.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentBeginDragHandler : SendToParentEventSystemHandler<IBeginDragHandler>,
                                                       IBeginDragHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<IBeginDragHandler> CallbackEventFunction =>
            ExecuteEvents.beginDragHandler;

        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
