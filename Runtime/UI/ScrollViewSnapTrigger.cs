using System;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// <see cref="ScrollView"/> 自动吸附的触发条件。
    /// </summary>
    [Flags]
    public enum ScrollViewSnapTrigger
    {
        /// <summary>
        /// 不吸附。
        /// </summary>
        None = 0,

        /// <summary>
        /// 允许在拖拽中触发由其它 <see cref="ScrollViewSnapTrigger"/> 值定义的自动吸附。
        /// </summary>
        AllowWhileDragging = 1 << 0,

        /// <summary>
        /// 当标准化滚动位置改变时自动吸附。
        /// </summary>
        OnNormalizedScrollPositionChanged = 1 << 1,

        /// <summary>
        /// 当结束拖拽时自动吸附。
        /// </summary>
        OnEndDrag = 1 << 2,

        /// <summary>
        /// 当指针抬起时自动吸附。
        /// </summary>
        OnPointerUp = 1 << 3
    }
}
