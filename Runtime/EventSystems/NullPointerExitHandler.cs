using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 响应 <see cref="IPointerExitHandler.OnPointerExit"/>，但不执行任何操作。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullPointerExitHandler : MonoBehaviour, IPointerExitHandler
    {
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
        }
    }
}
