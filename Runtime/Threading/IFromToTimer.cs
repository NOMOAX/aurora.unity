using System;

namespace Aurora.Unity.Threading
{
    /// <summary>
    /// 从指定的起点计时到终点的计时器。
    /// </summary>
    public interface IFromToTimer : IDisposable
    {
        /// <summary>
        /// 获取或设置计时器是否在运行。
        /// </summary>
        /// <exception cref="ObjectDisposedException">此实例已释放。</exception>
        bool Running { get; set; }

        /// <summary>
        /// 获取或设置起点。
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> 为非数字或无穷大。</exception>
        /// <exception cref="ObjectDisposedException">此实例已释放。</exception>
        double From { get; set; }

        /// <summary>
        /// 获取或设置终点。
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> 为非数字。</exception>
        /// <exception cref="ObjectDisposedException">此实例已释放。</exception>
        double To { get; set; }

        /// <summary>
        /// 获取或设置当前时间。
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> 为非数字。</exception>
        /// <exception cref="ObjectDisposedException">此实例已释放。</exception>
        double Time { get; set; }

        /// <summary>
        /// 获取截断小数部分的当前时间。
        /// </summary>
        /// <exception cref="ObjectDisposedException">此实例已释放。</exception>
        double TimeTruncated { get; }

        /// <summary>
        /// 获取或设置进度。
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> 不在 [0, 1] 范围内。</exception>
        /// <exception cref="ObjectDisposedException">此实例已释放。</exception>
        double Progress { get; set; }

        /// <summary>
        /// <see cref="Time"/> 发生改变。
        /// </summary>
        /// <exception cref="ObjectDisposedException">此实例已释放。</exception>
        event FromToTimerValueChangedEventHandler TimeChanged;

        /// <summary>
        /// <see cref="TimeTruncated"/> 发生改变。
        /// </summary>
        /// <exception cref="ObjectDisposedException">此实例已释放。</exception>
        event FromToTimerValueChangedEventHandler TimeTruncatedChanged;

        /// <summary>
        /// <see cref="Progress"/> 发生改变。
        /// </summary>
        /// <exception cref="ObjectDisposedException">此实例已释放。</exception>
        event FromToTimerValueChangedEventHandler ProgressChanged;

        /// <summary>
        /// 计时结束。
        /// </summary>
        /// <exception cref="ObjectDisposedException">此实例已释放。</exception>
        event FromToTimerCompletedEventHandler Completed;
    }
}
