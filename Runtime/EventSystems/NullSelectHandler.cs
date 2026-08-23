using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Responds to <see cref="ISelectHandler.OnSelect"/> but performs no action.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullSelectHandler : MonoBehaviour, ISelectHandler
    {
        void ISelectHandler.OnSelect(BaseEventData eventData)
        {
        }
    }
}
