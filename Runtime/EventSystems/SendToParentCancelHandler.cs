using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Forwards <see cref="ICancelHandler.OnCancel"/> to the parent.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentCancelHandler : SendToParentEventSystemHandler<ICancelHandler>, ICancelHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<ICancelHandler> CallbackEventFunction =>
            ExecuteEvents.cancelHandler;

        void ICancelHandler.OnCancel(BaseEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
