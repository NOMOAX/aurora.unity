using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// The sub-interfaces of <see cref="IEventSystemExecutor"/> correspond one-to-one with the sub-interfaces of <see cref="IEventSystemHandler"/> and provide similar members.
    /// <br/>
    /// Implementing a sub-interface of <see cref="IEventSystemExecutor"/> allows handling events without directly receiving those events sent by the Unity event system merely because a sub-interface of <see cref="IEventSystemHandler"/> is implemented.
    /// </summary>
    public interface IEventSystemExecutor
    {
    }
}
