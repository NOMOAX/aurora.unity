using System;
using UnityEngine;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides extension methods for the <see cref="CanvasGroup"/> class.
    /// </summary>
    public static class CanvasGroupExtensions
    {
        /// <summary>
        /// Sets the <see cref="CanvasGroup.alpha"/> and <see cref="CanvasGroup.blocksRaycasts"/> of a <see cref="CanvasGroup"/>.
        /// </summary>
        /// <param name="canvasGroup">The canvas group.</param>
        /// <param name="value">
        /// A boolean value, its meaning is as follows:
        /// <list type="table">
        /// <listheader><term>Value</term><description>Meaning</description></listheader>
        /// <item><term><see langword="true"/></term><description>Sets <paramref name="canvasGroup"/>'s <see cref="CanvasGroup.alpha"/> to 1 and <see cref="CanvasGroup.blocksRaycasts"/> to <see langword="true"/></description></item>
        /// <item><term><see langword="false"/></term><description>Sets <paramref name="canvasGroup"/>'s <see cref="CanvasGroup.alpha"/> to 0 and <see cref="CanvasGroup.blocksRaycasts"/> to <see langword="false"/></description></item>
        /// </list>
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="canvasGroup"/> is <see langword="null"/>.</exception>
        public static void Set(this CanvasGroup canvasGroup, bool value)
        {
            if (!canvasGroup)
            {
                throw new ArgumentNullException(nameof(canvasGroup));
            }
            canvasGroup.alpha          = value ? 1 : 0;
            canvasGroup.blocksRaycasts = value;
        }
    }
}
