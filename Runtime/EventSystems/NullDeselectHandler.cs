using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 响应 <see cref="IDeselectHandler.OnDeselect"/>，但不执行任何操作。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullDeselectHandler : MonoBehaviour, IDeselectHandler
    {
        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
        }
    }
}
