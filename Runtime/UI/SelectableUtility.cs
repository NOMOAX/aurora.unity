using System;
using System.Reflection;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// Provides utility methods for the <see cref="Selectable"/> class.
    /// </summary>
    public static class SelectableUtility
    {
        private static readonly Func<Selectable, bool> SelectableIsPressedGetter =
            (Func<Selectable, bool>)Delegate.CreateDelegate(
                typeof(Func<Selectable, bool>),
                typeof(Selectable).GetMethod(
                    "IsPressed",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    CallingConventions.HasThis,
                    Type.EmptyTypes,
                    null
                )!
            );

        /// <summary>
        /// Determines whether the <see cref="Selectable"/> has been pressed.
        /// </summary>
        /// <param name="selectable">The selectable object.</param>
        /// <returns><see langword="true"/> if <paramref name="selectable"/> has been pressed; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="selectable"/> is <see langword="null"/>.</exception>
        public static bool IsPressed(Selectable selectable)
        {
            if (!selectable)
            {
                throw new ArgumentNullException(nameof(selectable));
            }
            return SelectableIsPressedGetter(selectable);
        }
    }
}
