using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 响应 <see cref="IPointerUpHandler.OnPointerUp"/>，但不执行任何操作。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullPointerUpHandler : MonoBehaviour, IPointerUpHandler
    {
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
        }
    }
}
