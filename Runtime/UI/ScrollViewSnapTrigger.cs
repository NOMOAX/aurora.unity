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
        OnEndDrag = 1 << 0,

        /// <summary>
        /// 当不在拖拽中，标准化滚动位置改变，并且速度小于阈值时自动吸附。
        /// </summary>
        /// <seealso cref="ScrollView.snapSpeedThreshold"/>
        /// <seealso cref="ScrollView.scrollSnapDelay"/>
        OnNormalizedScrollPositionChanged = 1 << 1,

        /// <summary>
        /// 当不在拖拽中，指针抬起，并且 <see cref="ScrollRect"/> 的速度很低时自动吸附。
        /// </summary>
        OnPointerUpWithLowSpeed = 1 << 2
    }
}
