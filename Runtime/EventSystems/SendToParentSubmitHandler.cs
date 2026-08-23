using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 将 <see cref="ISubmitHandler.OnSubmit"/> 传递给父级。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SendToParentSubmitHandler : SendToParentEventSystemHandler<ISubmitHandler>, ISubmitHandler
    {
        /// <inheritdoc />
        protected override ExecuteEvents.EventFunction<ISubmitHandler> CallbackEventFunction =>
            ExecuteEvents.submitHandler;

        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
            SendToParent(eventData);
        }
    }
}
