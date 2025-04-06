using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// <see cref="IEventSystemExecutor"/> 的各个子接口与 <see cref="IEventSystemHandler"/> 的各个子接口一一对应，提供相似的成员。
    /// <br/>
    /// 实现 <see cref="IEventSystemExecutor"/> 的子接口，既可以处理事件，又不会因为实现了 <see cref="IEventSystemHandler"/> 的子接口而直接收到由 Unity 事件系统发送的事件。
    /// </summary>
    public interface IEventSystemExecutor
    {
    }
}
