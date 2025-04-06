using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// 响应 <see cref="IMoveHandler.OnMove"/>，但不执行任何操作。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullMoveHandler : MonoBehaviour, IMoveHandler
    {
        void IMoveHandler.OnMove(AxisEventData eventData)
        {
        }
    }
}
