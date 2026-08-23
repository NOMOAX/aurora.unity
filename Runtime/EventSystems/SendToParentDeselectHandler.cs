using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Forwards <see cref="IDeselectHandler.OnDeselect"/> to the parent.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentDeselectHandler : SendToParentEventSystemHandler<IDeselectHandler>, IDeselectHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<IDeselectHandler> CallbackEventFunction =>
            ExecuteEvents.deselectHandler;

        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
