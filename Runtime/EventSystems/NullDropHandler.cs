using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Responds to <see cref="IDropHandler.OnDrop"/> but performs no action.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullDropHandler : MonoBehaviour, IDropHandler
    {
        void IDropHandler.OnDrop(PointerEventData eventData)
        {
        }
    }
}
