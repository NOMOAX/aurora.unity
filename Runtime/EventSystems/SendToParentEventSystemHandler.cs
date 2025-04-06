using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 将事件传递给父级。
    /// </summary>
    /// <typeparam name="T">事件处理程序。</typeparam>
    public abstract class SendToParentEventSystemHandler<T> : MonoBehaviour where T : IEventSystemHandler
    {
        /// <summary>
        /// 处理事件的回调。
        /// </summary>
        protected abstract ExecuteEvents.EventFunction<T> CallbackEventFunction { get; }

        /// <summary>
        /// 将事件传递给父级。
        /// </summary>
        /// <param name="eventData">事件数据。</param>
        protected void SendToParent(BaseEventData eventData)
        {
            if (transform.parent == null)
            {
                return;
            }
            ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, CallbackEventFunction);
        }
    }
}
