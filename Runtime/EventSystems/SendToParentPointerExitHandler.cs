using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Forwards <see cref="IPointerExitHandler.OnPointerExit"/> to the parent.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentPointerExitHandler : SendToParentEventSystemHandler<IPointerExitHandler>,
                                                         IPointerExitHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<IPointerExitHandler> CallbackEventFunction =>
            ExecuteEvents.pointerExitHandler;

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
