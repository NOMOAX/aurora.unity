using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Forwards <see cref="ISubmitHandler.OnSubmit"/> to the parent.
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
