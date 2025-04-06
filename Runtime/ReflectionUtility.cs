using System;
using System.Reflection;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 提供一组与反射相关的工具方法。
    /// </summary>
    public static class ReflectionUtility
    {
        private static readonly FieldInfo WaitForSecondsSecondsFieldInfo = typeof(WaitForSeconds).GetField(
            "m_Seconds",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        /// <summary>
        /// 获取 <see cref="WaitForSeconds"/> 中的秒数。
        /// </summary>
        /// <param name="waitForSeconds"><see cref="WaitForSeconds"/> 实例。</param>
        /// <returns><paramref name="waitForSeconds"/> 的秒数。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="waitForSeconds"/> 为 <see langword="null"/>。</exception>
        public static float GetWaitForSecondsSeconds(WaitForSeconds waitForSeconds)
        {
            if (waitForSeconds is null)
            {
                throw new ArgumentNullException(nameof(waitForSeconds));
            }
            var seconds = (float) WaitForSecondsSecondsFieldInfo.GetValue(waitForSeconds);
            return seconds;
        }
    }
}
