using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 将 <see cref="IPointerUpHandler.OnPointerUp"/> 传递给父级。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentPointerUpHandler : SendToParentEventSystemHandler<IPointerUpHandler>,
                                                       IPointerUpHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<IPointerUpHandler> CallbackEventFunction =>
            ExecuteEvents.pointerUpHandler;

        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
