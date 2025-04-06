using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 将 <see cref="IPointerClickHandler.OnPointerClick"/> 传递给父级。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentPointerClickHandler : SendToParentEventSystemHandler<IPointerClickHandler>,
                                                          IPointerClickHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<IPointerClickHandler> CallbackEventFunction =>
            ExecuteEvents.pointerClickHandler;

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
