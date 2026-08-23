using System;

namespace Aurora.Unity.Threading
{
    /// <summary>
    /// A timer that counts from a specified start point to an end point.
    /// </summary>
    public interface IFromToTimer : IDisposable
    {
        /// <summary>
        /// Gets or sets whether the timer is running.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance has been disposed.</exception>
        bool Running { get; set; }

        /// <summary>
        /// Gets or sets the start point.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a number or is infinity.</exception>
        /// <exception cref="ObjectDisposedException">This instance has been disposed.</exception>
        double From { get; set; }

        /// <summary>
        /// Gets or sets the end point.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a number.</exception>
        /// <exception cref="ObjectDisposedException">This instance has been disposed.</exception>
        double To { get; set; }

        /// <summary>
        /// Gets or sets the current time.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a number.</exception>
        /// <exception cref="ObjectDisposedException">This instance has been disposed.</exception>
        double Time { get; set; }

        /// <summary>
        /// Gets the current time with its fractional part truncated.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance has been disposed.</exception>
        double TimeTruncated { get; }

        /// <summary>
        /// Gets or sets the progress.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not in the [0, 1] range.</exception>
        /// <exception cref="ObjectDisposedException">This instance has been disposed.</exception>
        double Progress { get; set; }

        /// <summary>
        /// <see cref="Time"/> changes.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance has been disposed.</exception>
        event FromToTimerValueChangedEventHandler TimeChanged;

        /// <summary>
        /// <see cref="TimeTruncated"/> changes.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance has been disposed.</exception>
        event FromToTimerValueChangedEventHandler TimeTruncatedChanged;

        /// <summary>
        /// <see cref="Progress"/> changes.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance has been disposed.</exception>
        event FromToTimerValueChangedEventHandler ProgressChanged;

        /// <summary>
        /// Timing ends.
        /// </summary>
        /// <exception cref="ObjectDisposedException">This instance has been disposed.</exception>
        event FromToTimerCompletedEventHandler Completed;
    }
}
