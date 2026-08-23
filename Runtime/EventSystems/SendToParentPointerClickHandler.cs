using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Forwards <see cref="IPointerClickHandler.OnPointerClick"/> to the parent.
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
