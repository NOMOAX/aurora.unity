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
        /// 更新计时器首次以及再次触发前的等待时间。
        /// </summary>
        /// <param name="dueTime">
        /// 计时器首次触发前的等待时间。
        /// <list type="table">
        /// <listheader><term>值</term><description>含义</description></listheader>
        /// <item><term><see cref="Timeout.InfiniteTimeSpan"/></term><description>禁用计时器</description></item>
        /// <item><term><see cref="TimeSpan.Zero"/></term><description>禁用计时器，然后启用计时器并立即触发</description></item>
        /// <item><term>大于 <see cref="TimeSpan.Zero"/></term><description>禁用计时器，然后启用计时器，计时器将在指定的时间后触发（实际等待时间受计时器精度影响）</description></item>
        /// </list>
        /// </param>
        /// <param name="period">
        /// 计时器再次触发前的等待时间。
        /// <list type="table">
        /// <listheader><term>值</term><description>含义</description></listheader>
        /// <item><term><see cref="Timeout.InfiniteTimeSpan"/></term><description>在计时器首次触发后禁用计时器</description></item>
        /// <item><term><see cref="TimeSpan.Zero"/> 以及大于 <see cref="TimeSpan.Zero"/></term><description>在计时器触发后，将在指定的时间后再次触发，反复如此，直至计时器被禁用（实际等待时间受计时器精度影响）</description></item>
        /// </list>
        /// </param>
        /// <returns>如果更新计时器成功，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dueTime"/> 或 <paramref name="period"/> 不为 <see cref="Timeout.InfiniteTimeSpan">Timeout.InfiniteTimeSpan</see>，并且它们的毫秒数不在 [0, 4294967294] 范围内。</exception>
        bool Change(TimeSpan dueTime, TimeSpan period);
    }
}
