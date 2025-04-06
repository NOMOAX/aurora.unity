using System.Runtime.InteropServices;

namespace Aurora.Unity.PlayerLoop
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal struct AuroraUnityUpdate
    {
        #region Updating

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct UpdatingForPlayerLoopRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct UpdatingForContinuationRunner
        {
        }

        #endregion

        #region Updated

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct UpdatedForPlayerLoopRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct UpdatedForContinuationRunner
        {
        }

        #endregion

        #region UpdateYielded

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct UpdateYieldedForPlayerLoopRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct UpdateYieldedForContinuationRunner
        {
        }

        #endregion

        #region UpdatePosted

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct UpdatePostedForPlayerLoopRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        internal struct UpdatePostedForContinuationRunner
        {
        }

        #endregion
    }
}
