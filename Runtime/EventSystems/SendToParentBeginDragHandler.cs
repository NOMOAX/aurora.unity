using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 将 <see cref="IBeginDragHandler.OnBeginDrag"/> 传递给父级。
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
