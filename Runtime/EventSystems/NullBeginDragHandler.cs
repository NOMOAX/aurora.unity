using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 响应 <see cref="IBeginDragHandler.OnBeginDrag"/>，但不执行任何操作。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullBeginDragHandler : MonoBehaviour, IBeginDragHandler
    {
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
        }
    }
}
