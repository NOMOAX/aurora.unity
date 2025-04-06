using System;
using System.Threading;

namespace Aurora.Unity.Threading
{
    /// <summary>
    /// 计时器。
    /// </summary>
    public interface ITimer : IDisposable
    {
        /// <summary>
        /// 更新计时器的启动时间和调用计时器回调方法之间的时间间隔。
        /// </summary>
        /// <param name="dueTime">
        /// 启动时间。
        /// <list type="table">
        /// <listheader><term>值</term><description>含义</description></listheader>
        /// <item><term><see cref="Timeout.InfiniteTimeSpan"/></term><description>禁用计时器</description></item>
        /// <item><term><see cref="TimeSpan.Zero"/></term><description>禁用计时器，然后立即启用计时器</description></item>
        /// <item><term>大于 <see cref="TimeSpan.Zero"/></term><description>禁用计时器，然后在指定的时间后启用计时器</description></item>
        /// </list>
        /// </param>
        /// <param name="period">
        /// 调用计时器回调方法之间的时间间隔。
        /// <list type="table">
        /// <listheader><term>值</term><description>含义</description></listheader>
        /// <item><term><see cref="Timeout.InfiniteTimeSpan"/></term><description>在首次执行计时器回调方法后禁用计时器</description></item>
        /// <item><term><see cref="TimeSpan.Zero"/> 以及大于 <see cref="TimeSpan.Zero"/></term><description>在首次执行计时器回调方法后每间隔该时间再执行一次回调方法（实际间隔时间受到计时器精度的影响）</description></item>
        /// </list>
        /// </param>
        /// <returns>如果更新计时器成功，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dueTime"/> 或 <paramref name="period"/> 不为 <see cref="Timeout.InfiniteTimeSpan">Timeout.InfiniteTimeSpan</see>，并且它们的毫秒数不在 [0, 4294967294] 范围内。</exception>
        bool Change(TimeSpan dueTime, TimeSpan period);
    }
}
