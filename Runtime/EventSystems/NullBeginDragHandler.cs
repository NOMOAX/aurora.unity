using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Responds to <see cref="IBeginDragHandler.OnBeginDrag"/> but performs no action.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullBeginDragHandler : MonoBehaviour, IBeginDragHandler
    {
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
        }
    }
}
