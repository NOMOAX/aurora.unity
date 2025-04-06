using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 将 <see cref="ISelectHandler.OnSelect"/> 传递给父级。
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
