using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Forwards <see cref="IInitializePotentialDragHandler.OnInitializePotentialDrag"/> to the parent.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentInitializePotentialDragHandler :
        SendToParentEventSystemHandler<IInitializePotentialDragHandler>,
        IInitializePotentialDragHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<IInitializePotentialDragHandler> CallbackEventFunction =>
            ExecuteEvents.initializePotentialDrag;

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
