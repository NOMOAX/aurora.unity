using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides utility methods for the <see cref="AsyncOperation"/> class.
    /// </summary>
    public static class AsyncOperationUtility
    {
        private static readonly FieldInfo PtrFieldInfo = typeof(AsyncOperation).GetField(
            "m_Ptr",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        /// <summary>
        /// Determines whether the specified <see cref="AsyncOperation"/> has been disposed.
        /// </summary>
        /// <param name="asyncOperation">The asynchronous operation.</param>
        /// <returns><see langword="true"/> if <paramref name="asyncOperation"/> has been disposed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="asyncOperation"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDisposed(AsyncOperation asyncOperation)
        {
            if (asyncOperation == null)
            {
                throw new ArgumentNullException(nameof(asyncOperation));
            }
            return InternalIsDisposed(asyncOperation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool InternalIsDisposed(AsyncOperation asyncOperation)
        {
            var ptr = (IntPtr)PtrFieldInfo.GetValue(asyncOperation);
            return ptr == IntPtr.Zero;
        }
    }
}
