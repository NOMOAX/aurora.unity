using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 响应 <see cref="IScrollHandler.OnScroll"/>，但不执行任何操作。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullScrollHandler : MonoBehaviour, IScrollHandler
    {
        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
        }
    }
}
