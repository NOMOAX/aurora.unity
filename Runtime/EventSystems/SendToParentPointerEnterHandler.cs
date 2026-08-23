using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Forwards <see cref="IPointerEnterHandler.OnPointerEnter"/> to the parent.
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
