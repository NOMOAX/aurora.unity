using System;

namespace Aurora.Unity.Threading
{
    /// <summary>
    /// A counter.
    /// </summary>
    public interface ICounter : IDisposable
    {
        /// <summary>
        /// Updates the counts required for the counter to trigger for the first time and again.
        /// </summary>
        /// <param name="dueCount">
        /// The count required for the counter to trigger for the first time.
        /// <list type="table">
        /// <listheader><term>Value</term><description>Meaning</description></listheader>
        /// <item><term>-1</term><description>Disables the counter</description></item>
        /// <item><term>0</term><description>Disables the counter, then enables it and triggers it immediately</description></item>
        /// <item><term>Greater than 0</term><description>Disables the counter, then enables it; the counter triggers after the specified count</description></item>
        /// </list>
        /// </param>
        /// <param name="period">
        /// The count required for the counter to trigger again.
        /// <list type="table">
        /// <listheader><term>Value</term><description>Meaning</description></listheader>
        /// <item><term>-1</term><description>Disables the counter after it triggers for the first time</description></item>
        /// <item><term>0 and greater than 0</term><description>After the counter triggers, it triggers again after the specified count, repeating until the counter is disabled (the actual count is affected by counter precision and is at least 1)</description></item>
        /// </list>
        /// </param>
        /// <returns><see langword="true"/> if the counter was updated successfully; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dueCount"/> or <paramref name="period"/> is less than 0 but not -1.</exception>
        bool Change(int dueCount, int period);
    }
}
