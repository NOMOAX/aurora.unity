using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 将 <see cref="IMoveHandler.OnMove"/> 传递给父级。
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
