using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Forwards <see cref="IPointerUpHandler.OnPointerUp"/> to the parent.
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
