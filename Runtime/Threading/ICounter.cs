using System;

namespace Aurora.Unity.Threading
{
    /// <summary>
    /// 计数器。
    /// </summary>
    public interface ICounter : IDisposable
    {
        /// <summary>
        /// 更新计数器的启动个数和调用计数器回调方法之间的个数间隔。
        /// </summary>
        /// <param name="dueCount">
        /// 启动个数。
        /// <list type="table">
        /// <listheader><term>值</term><description>含义</description></listheader>
        /// <item><term>-1</term><description>禁用计数器</description></item>
        /// <item><term>0</term><description>禁用计数器，然后立即启用计数器</description></item>
        /// <item><term>大于 0</term><description>禁用计数器，然后在指定的个数后启用计数器</description></item>
        /// </list>
        /// </param>
        /// <param name="period">
        /// 调用计数器回调方法之间的个数间隔。
        /// <list type="table">
        /// <listheader><term>值</term><description>含义</description></listheader>
        /// <item><term>-1</term><description>在首次执行计数器回调方法后禁用计数器</description></item>
        /// <item><term>0 以及大于 0</term><description>在首次执行计数器回调方法后每间隔该个数再执行一次回调方法（实际间隔次数受到计数器精度的影响，且至少为 1）</description></item>
        /// </list>
        /// </param>
        /// <returns>如果更新计数器成功，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dueCount"/> 或 <paramref name="period"/> 小于 0，但不为 -1。</exception>
        bool Change(int dueCount, int period);
    }
}
