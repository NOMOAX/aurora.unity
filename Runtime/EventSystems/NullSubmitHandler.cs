using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Responds to <see cref="ISubmitHandler.OnSubmit"/> but performs no action.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullSubmitHandler : MonoBehaviour, ISubmitHandler
    {
        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
        }
    }
}
