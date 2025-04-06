using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 将 <see cref="IInitializePotentialDragHandler.OnInitializePotentialDrag"/> 传递给父级。
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
