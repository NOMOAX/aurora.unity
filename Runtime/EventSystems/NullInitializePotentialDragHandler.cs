using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 响应 <see cref="IInitializePotentialDragHandler.OnInitializePotentialDrag"/>，但不执行任何操作。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullInitializePotentialDragHandler : MonoBehaviour, IInitializePotentialDragHandler
    {
        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
        }
    }
}
