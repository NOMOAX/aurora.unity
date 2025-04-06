using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 将 <see cref="IEndDragHandler.OnEndDrag"/> 传递给父级。
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
