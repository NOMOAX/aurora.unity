using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 响应 <see cref="ISubmitHandler.OnSubmit"/>，但不执行任何操作。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullUpdateSelectedHandler : MonoBehaviour, IUpdateSelectedHandler
    {
        void IUpdateSelectedHandler.OnUpdateSelected(BaseEventData eventData)
        {
        }
    }
}
