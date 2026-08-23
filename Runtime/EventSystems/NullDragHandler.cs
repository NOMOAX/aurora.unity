using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Responds to <see cref="IDragHandler.OnDrag"/> but performs no action.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullDragHandler : MonoBehaviour, IDragHandler
    {
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
        }
    }
}
