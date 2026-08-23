using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 响应 <see cref="ISelectHandler.OnSelect"/>，但不执行任何操作。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullSelectHandler : MonoBehaviour, ISelectHandler
    {
        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
        }
    }
}
