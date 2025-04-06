using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 将 <see cref="IDragHandler.OnDrag"/> 传递给父级。
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
