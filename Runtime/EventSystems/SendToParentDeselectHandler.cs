using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 将 <see cref="IDeselectHandler.OnDeselect"/> 传递给父级。
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
