using System;

namespace Aurora.Unity.UI
{
    /// <summary>
    /// 控制 <see cref="ScrollView"/> 终止补间动画的行为。
    /// </summary>
    [Flags]
    public enum ScrollerStopTween
    {
        /// <summary>
        /// 不终止补间动画。
        /// </summary>
        None = 0,

        /// <summary>
        /// 当指针按下时终止补间动画。
        /// </summary>
        OnPointerDown = 1 << 0,

        /// <summary>
        /// 当开始拖拽时终止补间动画。
        /// </summary>
        OnBeginDrag = 1 << 1
    }
}
