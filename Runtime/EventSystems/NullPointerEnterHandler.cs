using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 响应 <see cref="IPointerEnterHandler.OnPointerEnter"/>，但不执行任何操作。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullPointerEnterHandler : MonoBehaviour, IPointerEnterHandler
    {
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
        }
    }
}
