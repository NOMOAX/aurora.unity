using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 响应 <see cref="IDragHandler.OnDrag"/>，但不执行任何操作。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullDragHandler : MonoBehaviour, IDragHandler
    {
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
        }
    }
}
