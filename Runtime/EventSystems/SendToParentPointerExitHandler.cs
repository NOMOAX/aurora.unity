using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 将 <see cref="IPointerExitHandler.OnPointerExit"/> 传递给父级。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentPointerExitHandler : SendToParentEventSystemHandler<IPointerExitHandler>,
                                                         IPointerExitHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<IPointerExitHandler> CallbackEventFunction =>
            ExecuteEvents.pointerExitHandler;

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
