using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 将 <see cref="IDropHandler.OnDrop"/> 传递给父级。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentDropHandler : SendToParentEventSystemHandler<IDropHandler>, IDropHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<IDropHandler> CallbackEventFunction => ExecuteEvents.dropHandler;

        void IDropHandler.OnDrop(PointerEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
