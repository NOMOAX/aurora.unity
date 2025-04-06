using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 响应 <see cref="ICancelHandler.OnCancel"/>，但不执行任何操作。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullCancelHandler : MonoBehaviour, ICancelHandler
    {
        void ICancelHandler.OnCancel(BaseEventData eventData)
        {
        }
    }
}
