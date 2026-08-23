using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Responds to <see cref="IInitializePotentialDragHandler.OnInitializePotentialDrag"/> but performs no action.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullInitializePotentialDragHandler : MonoBehaviour, IInitializePotentialDragHandler
    {
        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
        }
    }
}
