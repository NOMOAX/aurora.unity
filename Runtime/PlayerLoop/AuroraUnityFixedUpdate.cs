using System.Runtime.InteropServices;

namespace Aurora.Unity.PlayerLoop
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal struct AuroraUnityFixedUpdate
    {
        #region FixedUpdating

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct FixedUpdatingForPlayerLoopRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct FixedUpdatingForContinuationRunner
        {
        }

        #endregion

        #region FixedUpdated

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct FixedUpdatedForPlayerLoopRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct FixedUpdatedForContinuationRunner
        {
        }

        #endregion
    }
}
