using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 响应 <see cref="IPointerClickHandler.OnPointerClick"/>，但不执行任何操作。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullPointerClickHandler : MonoBehaviour, IPointerClickHandler
    {
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
        }
    }
}
