using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Forwards <see cref="IDropHandler.OnDrop"/> to the parent.
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
