using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Responds to <see cref="IDeselectHandler.OnDeselect"/> but performs no action.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullDeselectHandler : MonoBehaviour, IDeselectHandler
    {
        void IDeselectHandler.OnDeselect(BaseEventData eventData)
        {
        }
    }
}
