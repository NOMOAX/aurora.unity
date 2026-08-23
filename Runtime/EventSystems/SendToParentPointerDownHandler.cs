using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 将 <see cref="IPointerDownHandler.OnPointerDown"/> 传递给父级。
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
