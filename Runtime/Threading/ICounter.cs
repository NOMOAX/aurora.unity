using System;

namespace Aurora.Unity.Threading
{
    /// <summary>
    /// 计数器。
    /// </summary>
    public interface ICounter : IDisposable
    {
        /// <summary>
        /// 更新计数器首次以及再次触发时所需的个数。
        /// </summary>
        /// <param name="dueCount">
        /// 计数器首次触发所需的个数。
        /// <list type="table">
        /// <listheader><term>值</term><description>含义</description></listheader>
        /// <item><term>-1</term><description>禁用计数器</description></item>
        /// <item><term>0</term><description>禁用计数器，然后启用计数器并立即触发</description></item>
        /// <item><term>大于 0</term><description>禁用计数器，然后启用计数器，计数器将在指定的个数后触发</description></item>
        /// </list>
        /// </param>
        /// <param name="period">
        /// 计数器再次触发所需的个数。
        /// <list type="table">
        /// <listheader><term>值</term><description>含义</description></listheader>
        /// <item><term>-1</term><description>在计数器首次触发后禁用计数器</description></item>
        /// <item><term>0 以及大于 0</term><description>在计数器触发后，将在指定的个数后再次触发，反复如此，直至计器被禁用（实际个数受计数器精度影响，且至少为 1）</description></item>
        /// </list>
        /// </param>
        /// <returns>如果更新计数器成功，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dueCount"/> 或 <paramref name="period"/> 小于 0，但不为 -1。</exception>
        bool Change(int dueCount, int period);
    }
}
