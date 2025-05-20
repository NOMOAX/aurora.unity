using System;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 为 <see cref="EnhancedButtonStatus"/> 枚举提供扩展方法。
    /// </summary>
    public static class EnhancedButtonStatusExtensions
    {
        /// <summary>
        /// 假设当某个 <see cref="EnhancedButton"/> 拥有指定的状态时，获取一个值，这个值指示该按钮是否可交互。
        /// </summary>
        /// <param name="status">按钮状态。</param>
        /// <returns>如果 <paramref name="status"/> 为 <see cref="EnhancedButtonStatus.Default"/>、<see cref="EnhancedButtonStatus.Hovered"/>、<see cref="EnhancedButtonStatus.Pressed"/>，则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="status"/> 不是在 <see cref="EnhancedButtonStatus"/> 枚举中定义的成员。</exception>
        public static bool IsInteractable(this EnhancedButtonStatus status)
        {
            return status switch
            {
                EnhancedButtonStatus.Default => true,
                EnhancedButtonStatus.Hovered => true,
                EnhancedButtonStatus.Pressed => true,
                EnhancedButtonStatus.NotInteractable => false,
                EnhancedButtonStatus.Inactive => false,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }

        /// <summary>
        /// 假设当某个 <see cref="EnhancedButton"/> 拥有指定的状态时，获取一个值，这个值指示指针是否位于按钮内部。
        /// </summary>
        /// <param name="status">按钮状态。</param>
        /// <returns>如果 <paramref name="status"/> 为 <see cref="EnhancedButtonStatus.Hovered"/>、<see cref="EnhancedButtonStatus.Pressed"/>，则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="status"/> 不是在 <see cref="EnhancedButtonStatus"/> 枚举中定义的成员。</exception>
        public static bool IsPointerInside(this EnhancedButtonStatus status)
        {
            return status switch
            {
                EnhancedButtonStatus.Default => false,
                EnhancedButtonStatus.Hovered => true,
                EnhancedButtonStatus.Pressed => true,
                EnhancedButtonStatus.NotInteractable => false,
                EnhancedButtonStatus.Inactive => false,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }

        /// <summary>
        /// 假设当某个 <see cref="EnhancedButton"/> 拥有指定的状态时，获取一个值，这个值指示指针是否按下了按钮。
        /// </summary>
        /// <param name="status">按钮状态。</param>
        /// <returns>如果 <paramref name="status"/> 为 <see cref="EnhancedButtonStatus.Pressed"/>，则为 <see langword="true"/>，否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="status"/> 不是在 <see cref="EnhancedButtonStatus"/> 枚举中定义的成员。</exception>
        public static bool IsPointerDown(this EnhancedButtonStatus status)
        {
            return status switch
            {
                EnhancedButtonStatus.Default => false,
                EnhancedButtonStatus.Hovered => false,
                EnhancedButtonStatus.Pressed => true,
                EnhancedButtonStatus.NotInteractable => false,
                EnhancedButtonStatus.Inactive => false,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }
    }
}
