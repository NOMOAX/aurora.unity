using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 响应 <see cref="IEndDragHandler.OnEndDrag"/>，但不执行任何操作。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullEndDragHandler : MonoBehaviour, IEndDragHandler
    {
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
        }
    }
}
