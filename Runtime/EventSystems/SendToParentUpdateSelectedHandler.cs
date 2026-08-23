using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Forwards <see cref="IUpdateSelectedHandler.OnUpdateSelected"/> to the parent.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentUpdateSelectedHandler : SendToParentEventSystemHandler<IUpdateSelectedHandler>,
                                                            IUpdateSelectedHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<IUpdateSelectedHandler> CallbackEventFunction =>
            ExecuteEvents.updateSelectedHandler;

        void IUpdateSelectedHandler.OnUpdateSelected(BaseEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
