using UnityEngine;

namespace Aurora.Unity
{
    [DoNotDestroyOnLoad]
    [WithHideFlags(HideFlags.HideAndDontSave)]
    internal sealed class AlwaysActiveAndEnabled : SingletonBehaviour<AlwaysActiveAndEnabled>
    {
#if !UNITY_EDITOR
        private void OnApplicationQuit()
        {
            UnityEnvironment.IsPlaying = false;
        }
#endif
    }
}
