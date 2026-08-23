using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 此接口提供与 <see cref="IDropHandler"/> 接口相似的成员。
    /// </summary>
    public interface IDropExecutor : IEventSystemExecutor
    {
        /// <summary>
        /// 对应于 <see cref="IDropHandler.OnDrop"/>。
        /// </summary>
        /// <param name="sender">调用方。</param>
        /// <param name="eventData">指针事件数据。</param>
        void OnDrop(object sender, PointerEventData eventData);
    }
}
