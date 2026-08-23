using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Responds to <see cref="IMoveHandler.OnMove"/> but performs no action.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullMoveHandler : MonoBehaviour, IMoveHandler
    {
        void IMoveHandler.OnMove(AxisEventData eventData)
        {
        }
    }
}
