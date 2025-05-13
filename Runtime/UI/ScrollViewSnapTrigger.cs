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
        /// 当结束拖拽时自动吸附。
        /// </summary>
        OnEndDrag = 1 << 0,

        /// <summary>
        /// 当不在拖拽中，且标准化滚动位置改变且速度小于阈值时自动吸附。
        /// </summary>
        /// <remarks></remarks>
        OnNormalizedScrollPositionChanged = 1 << 1,

        /// <summary>
        /// 当不在拖拽中，当指针抬起时并且 <see cref="ScrollView"/> 的速度很低时自动吸附。
        /// </summary>
        OnPointerUpWithLowSpeed = 1 << 2
    }
}
