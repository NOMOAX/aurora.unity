using System;
using System.Reflection;
using UnityEngine.UI;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 为 <see cref="Selectable"/> 类提供工具方法。
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
        /// 判断 <see cref="Selectable"/> 是否已被按下。
        /// </summary>
        /// <param name="selectable">可选择对象。</param>
        /// <returns>如果 <paramref name="selectable"/> 已被按下，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="selectable"/> 为 <see langword="null"/>。</exception>
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
