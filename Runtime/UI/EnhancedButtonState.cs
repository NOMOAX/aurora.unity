namespace Aurora.Unity.UI
{
    /// <summary>
    /// <see cref="EnhancedButton"/> 的状态。
    /// </summary>
    public enum EnhancedButtonState
    {
        /// <summary>
        /// 默认状态。
        /// </summary>
        Default,

        /// <summary>
        /// 指针处于内部。
        /// </summary>
        Hovered,

        /// <summary>
        /// 指针按下，且处于内部。
        /// </summary>
        /// <list type="table">
        /// <listheader>
        /// <term>指针操作</term>
        /// <description>切换到新状态</description>
        /// </listheader>
        /// <item>
        /// <term>指针抬起</term>
        /// <description><see cref="Hovered"/></description>
        /// </item>
        /// <item>
        /// <term>指针离开内部</term>
        /// <description><see cref="Default"/></description>
        /// </item>
        /// <item>
        /// <term>指针离开内部，然后又进入内部</term>
        /// <description><see cref="Hovered"/></description>
        /// </item>
        /// </list>
        Pressed
    }
}
