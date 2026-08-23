using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 将 <see cref="ICancelHandler.OnCancel"/> 传递给父级。
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
