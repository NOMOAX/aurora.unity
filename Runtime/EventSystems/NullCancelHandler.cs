using UnityEngine;
using UnityEngine.EventSystems;

namespace Aurora.Unity.EventSystems
{
    /// <summary>
    /// Responds to <see cref="ICancelHandler.OnCancel"/> but performs no action.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NullCancelHandler : MonoBehaviour, ICancelHandler
    {
        void ICancelHandler.OnCancel(BaseEventData eventData)
        {
        }
    }
}
