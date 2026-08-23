using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Responds to <see cref="IPointerDownHandler.OnPointerDown"/> but performs no action.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullPointerDownHandler : MonoBehaviour, IPointerDownHandler
    {
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
        }
    }
}
