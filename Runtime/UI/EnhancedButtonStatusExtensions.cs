using System;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 为 <see cref="EnhancedButtonStatus"/> 枚举提供扩展方法。
    /// </summary>
    public static class EnhancedButtonStatusExtensions
    {
        /// <summary>
        /// 获取一个值，这个值指示按钮是否可交互。
        /// </summary>
        /// <param name="status">按钮状态。</param>
        /// <returns><see cref="EnhancedButton.Status"/> 为 <paramref name="status"/> 的 <see cref="EnhancedButton"/> 是否可交互。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="status"/> 不是在 <see cref="EnhancedButtonStatus"/> 枚举中定义的成员。</exception>
        public static bool IsInteractableStatus(this EnhancedButtonStatus status)
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
    }
}
