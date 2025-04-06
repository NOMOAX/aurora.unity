using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 为 <see cref="AsyncOperation"/> 类提供工具方法。
    /// </summary>
    public static class AsyncOperationUtility
    {
        private static readonly FieldInfo PtrFieldInfo = typeof(AsyncOperation).GetField(
            "m_Ptr",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        /// <summary>
        /// 判断指定的 <see cref="AsyncOperation"/> 是否已释放。
        /// </summary>
        /// <param name="asyncOperation">异步操作。</param>
        /// <returns>如果 <paramref name="asyncOperation"/> 已释放，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="asyncOperation"/> 为 <see langword="null"/>。</exception>
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
            var ptr = (IntPtr) PtrFieldInfo.GetValue(asyncOperation);
            return ptr == IntPtr.Zero;
        }
    }
}
