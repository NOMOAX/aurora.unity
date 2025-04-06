using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 此接口提供与 <see cref="IBeginDragHandler"/> 接口相似的成员。
    /// </summary>
    public interface IBeginDragExecutor : IEventSystemExecutor
    {
        /// <summary>
        /// 对应于 <see cref="IBeginDragHandler.OnBeginDrag"/>。
        /// </summary>
        /// <param name="sender">调用方。</param>
        /// <param name="eventData">指针事件数据。</param>
        void OnBeginDrag(object sender, PointerEventData eventData);
    }
}
