using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 将 <see cref="IPointerEnterHandler.OnPointerEnter"/> 传递给父级。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentPointerEnterHandler : SendToParentEventSystemHandler<IPointerEnterHandler>,
                                                          IPointerEnterHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<IPointerEnterHandler> CallbackEventFunction =>
            ExecuteEvents.pointerEnterHandler;

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
