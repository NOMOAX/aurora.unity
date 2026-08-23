using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Responds to <see cref="IPointerUpHandler.OnPointerUp"/> but performs no action.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullPointerUpHandler : MonoBehaviour, IPointerUpHandler
    {
        void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
        {
        }
    }
}
