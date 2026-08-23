using System;
using UnityEngine.UI;

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
        /// <remarks>简单、常见、好用，结束拖拽就自动吸附</remarks>
        OnEndDrag = 1 << 0,

        /// <summary>
        /// 当不在拖拽中，标准化滚动位置改变，并且速率小于阈值时自动吸附。
        /// </summary>
        /// <remarks>一般用于用户快速拖拽，将内容“甩”出去，内容由于惯性（<see cref="ScrollRect.inertia">ScrollRect.inertia</see> 为 <see langword="true"/>）慢慢减速，在速率低于阈值时自动吸附</remarks>
        /// <seealso cref="ScrollView.snapSpeedThreshold"/>
        /// <seealso cref="ScrollView.scrollSnapDelay"/>
        OnNormalizedScrollPositionChanged = 1 << 1,

        /// <summary>
        /// 当不在拖拽中，指针抬起，并且 <see cref="ScrollRect"/> 的速率很低时自动吸附。
        /// </summary>
        /// <remarks>一般与 <see cref="OnNormalizedScrollPositionChanged"/> 一起使用，在 <see cref="OnNormalizedScrollPositionChanged"/> 的基础上，用户没有将内容“甩”出去，而是在某次拖拽后按住不动，然后松手，此时 <see cref="ScrollRect"/> 的速率很低，自动吸附</remarks>
        OnPointerUpWithLowSpeed = 1 << 2
    }
}
