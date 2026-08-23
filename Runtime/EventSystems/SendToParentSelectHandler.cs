using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Forwards <see cref="ISelectHandler.OnSelect"/> to the parent.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentSelectHandler : SendToParentEventSystemHandler<ISelectHandler>, ISelectHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<ISelectHandler> CallbackEventFunction =>
            ExecuteEvents.selectHandler;

        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
