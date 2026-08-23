using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Responds to <see cref="IPointerExitHandler.OnPointerExit"/> but performs no action.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullPointerExitHandler : MonoBehaviour, IPointerExitHandler
    {
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
        }
    }
}
