namespace Aurora.Unity.UI
{
    /// <summary>
    /// 按钮状态。
    /// </summary>
    public enum EnhancedButtonStatus
    {
        /// <summary>
        /// 已激活，但未有任何交互。
        /// </summary>
        Default,

        /// <summary>
        /// 指针处于内部。
        /// </summary>
        Hovered,

        /// <summary>
        /// 指针按下，并且从指针按下时起，到现在为止，指针一直处于内部。
        /// </summary>
        /// <remarks>
        /// <list type="table">
        /// <listheader>
        /// <term>行为</term>
        /// <description>转变到</description>
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
        /// </remarks>
        Pressed,

        /// <summary>
        /// 已激活，但不可交互。
        /// </summary>
        NotInteractable,

        /// <summary>
        /// 未激活（组件未启用，或者游戏物体未激活）。
        /// </summary>
        Inactive
    }
}
