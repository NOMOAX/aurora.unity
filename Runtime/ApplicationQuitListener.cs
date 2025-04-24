#if !UNITY_EDITOR
using UnityEngine;

namespace Aurora.Unity
{
    [DoNotDestroyOnLoad]
    [WithHideFlags(HideFlags.HideAndDontSave)]
    internal sealed class ApplicationQuitListener : SingletonBehaviour<ApplicationQuitListener>
    {
        private void OnApplicationQuit()
        {
            RuntimeInitialization.ExitPlayMode(null);
        }
    }
}
#endif
