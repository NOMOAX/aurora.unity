using System;
using System.Threading;

namespace Aurora.Unity.Threading
{
    /// <summary>
    /// A timer.
    /// </summary>
    public interface ITimer : IDisposable
    {
        /// <summary>
        /// Updates the wait times before the timer triggers for the first time and again.
        /// </summary>
        /// <param name="dueTime">
        /// The wait time before the timer triggers for the first time.
        /// <list type="table">
        /// <listheader><term>Value</term><description>Meaning</description></listheader>
        /// <item><term><see cref="Timeout.InfiniteTimeSpan"/></term><description>Disables the timer</description></item>
        /// <item><term><see cref="TimeSpan.Zero"/></term><description>Disables the timer, then enables it and triggers it immediately</description></item>
        /// <item><term>Greater than <see cref="TimeSpan.Zero"/></term><description>Disables the timer, then enables it; the timer triggers after the specified time (the actual wait time is affected by timer precision)</description></item>
        /// </list>
        /// </param>
        /// <param name="period">
        /// The wait time before the timer triggers again.
        /// <list type="table">
        /// <listheader><term>Value</term><description>Meaning</description></listheader>
        /// <item><term><see cref="Timeout.InfiniteTimeSpan"/></term><description>Disables the timer after it triggers for the first time</description></item>
        /// <item><term><see cref="TimeSpan.Zero"/> and greater than <see cref="TimeSpan.Zero"/></term><description>After the timer triggers, it triggers again after the specified time, repeating until the timer is disabled (the actual wait time is affected by timer precision)</description></item>
        /// </list>
        /// </param>
        /// <returns><see langword="true"/> if the timer was updated successfully; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dueTime"/> or <paramref name="period"/> is not <see cref="Timeout.InfiniteTimeSpan">Timeout.InfiniteTimeSpan</see>, and their milliseconds are not in the [0, 4294967294] range.</exception>
        bool Change(TimeSpan dueTime, TimeSpan period);
    }
}
