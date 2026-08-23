using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Responds to <see cref="IPointerEnterHandler.OnPointerEnter"/> but performs no action.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullPointerEnterHandler : MonoBehaviour, IPointerEnterHandler
    {
        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
        }
    }
}
