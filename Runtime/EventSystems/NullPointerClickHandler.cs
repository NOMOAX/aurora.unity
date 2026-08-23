using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Responds to <see cref="IPointerClickHandler.OnPointerClick"/> but performs no action.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullPointerClickHandler : MonoBehaviour, IPointerClickHandler
    {
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
        }
    }
}
