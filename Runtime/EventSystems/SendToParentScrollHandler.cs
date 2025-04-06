using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 将 <see cref="IScrollHandler.OnScroll"/> 传递给父级。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentScrollHandler : SendToParentEventSystemHandler<IScrollHandler>, IScrollHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<IScrollHandler> CallbackEventFunction =>
            ExecuteEvents.scrollHandler;

        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
