using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Forwards <see cref="IMoveHandler.OnMove"/> to the parent.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentMoveHandler : SendToParentEventSystemHandler<IMoveHandler>, IMoveHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<IMoveHandler> CallbackEventFunction => ExecuteEvents.moveHandler;

        void IMoveHandler.OnMove(AxisEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
