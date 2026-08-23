using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 响应 <see cref="IDropHandler.OnDrop"/>，但不执行任何操作。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullDropHandler : MonoBehaviour, IDropHandler
    {
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
        }
    }
}
