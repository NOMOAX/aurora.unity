using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Responds to <see cref="IEndDragHandler.OnEndDrag"/> but performs no action.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullEndDragHandler : MonoBehaviour, IEndDragHandler
    {
        void IEndDragHandler.OnEndDrag(PointerEventData eventData)
        {
        }
    }
}
