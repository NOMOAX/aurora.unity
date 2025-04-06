using System.Runtime.InteropServices;

namespace Aurora.Unity.PlayerLoop
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal struct AuroraUnityLateUpdate
    {
        #region LateUpdating

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct LateUpdatingForPlayerLoopRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct LateUpdatingForContinuationRunner
        {
        }

        #endregion

        #region LateUpdated

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct LateUpdatedForPlayerLoopRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct LateUpdatedForContinuationRunner
        {
        }

        #endregion
    }
}
