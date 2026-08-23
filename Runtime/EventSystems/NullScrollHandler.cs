using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Responds to <see cref="IScrollHandler.OnScroll"/> but performs no action.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullScrollHandler : MonoBehaviour, IScrollHandler
    {
        void IScrollHandler.OnScroll(PointerEventData eventData)
        {
        }
    }
}
