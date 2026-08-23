using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 将 <see cref="IUpdateSelectedHandler.OnUpdateSelected"/> 传递给父级。
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
