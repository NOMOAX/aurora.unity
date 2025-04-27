using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// 为 <see cref="CanvasGroup"/> 类提供扩展方法。
    /// </summary>
    public static class CanvasGroupExtensions
    {
        /// <summary>
        /// 设置 <see cref="CanvasGroup"/> 的 <see cref="CanvasGroup.alpha"/> 与 <see cref="CanvasGroup.blocksRaycasts"/>。
        /// </summary>
        /// <param name="canvasGroup">画布组。</param>
        /// <param name="value">
        /// 一个布尔值，它的含义如下：
        /// <list type="table">
        /// <listheader><term>值</term><description>含义</description></listheader>
        /// <item><term><see langword="true"/></term><description>设置 <paramref name="canvasGroup"/> 的 <see cref="CanvasGroup.alpha"/> 为 1，<see cref="CanvasGroup.blocksRaycasts"/> 为 <see langword="true"/></description></item>
        /// <item><term><see langword="false"/></term><description>设置 <paramref name="canvasGroup"/> 的 <see cref="CanvasGroup.alpha"/> 为 0，<see cref="CanvasGroup.blocksRaycasts"/> 为 <see langword="false"/></description></item>
        /// </list>
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="canvasGroup"/> 为 <see langword="null"/>。</exception>
        public static void Set(this CanvasGroup canvasGroup, bool value)
        {
            if (canvasGroup == null)
            {
                throw new ArgumentNullException(nameof(canvasGroup));
            }
            canvasGroup.alpha          = value ? 1f : 0f;
            canvasGroup.blocksRaycasts = value;
        }
    }
}
