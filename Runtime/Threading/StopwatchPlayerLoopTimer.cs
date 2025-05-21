using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Aurora.Diagnostics;
using Aurora.Unity.PlayerLoop;

namespace Aurora.Unity.Threading
{
    /// <summary>
    /// 使用秒表计时并在特定的播放器循环阶段中处理非立即执行的回调的计时器。
    /// </summary>
    public sealed class StopwatchPlayerLoopTimer : ITimer, IPlayerLoopItem
    {
        private readonly object _lock = new();

        private volatile bool _disposed;

        private TimerTriggerCallback _callback;

        private object _state;

        private TimeSpan _dueTime;

        private TimeSpan _period;

        private readonly PlayerLoopPhase _playerLoopPhase;

        private bool _scheduled;

        private ValueStopwatch _stopwatch;

        private bool _firstCallbackInvoked;

        private volatile int _version;

        /// <summary>
        /// 初始化 <see cref="StopwatchPlayerLoopTimer"/> 类的新实例。
        /// </summary>
        /// <param name="callback">当计时器触发时执行的方法。</param>
        /// <param name="state">将传递给 <see cref="callback"/> 的第二个形参。</param>
        /// <param name="playerLoopPhase">播放器循环阶段。</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="playerLoopPhase"/> 不是在 <see cref="PlayerLoopPhase"/> 枚举中定义的成员。</exception>
        public StopwatchPlayerLoopTimer(TimerTriggerCallback callback, object state, PlayerLoopPhase playerLoopPhase)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            if (!EnumUtility<PlayerLoopPhase>.IsDefined(playerLoopPhase))
            {
                throw new ArgumentOutOfRangeException(nameof(playerLoopPhase));
            }
            _callback        = callback;
            _state           = state;
            _dueTime         = Timeout.InfiniteTimeSpan;
            _period          = Timeout.InfiniteTimeSpan;
            _playerLoopPhase = playerLoopPhase;
        }

        /// <summary>
        /// 初始化 <see cref="StopwatchPlayerLoopTimer"/> 类的新实例。
        /// </summary>
        /// <param name="callback">当计时器触发时执行的方法。</param>
        /// <param name="state">将传递给 <see cref="callback"/> 的第二个形参。</param>
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
        /// <param name="playerLoopPhase">播放器循环阶段。</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dueTime"/> 或 <paramref name="period"/> 不为 <see cref="Timeout.InfiniteTimeSpan">Timeout.InfiniteTimeSpan</see>，并且它们的毫秒数不在 [0, 4294967294] 范围内；或者 <paramref name="playerLoopPhase"/> 的值未定义。</exception>
        public StopwatchPlayerLoopTimer(
            TimerTriggerCallback callback,
            object               state,
            TimeSpan             dueTime,
            TimeSpan             period,
            PlayerLoopPhase      playerLoopPhase)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            ThrowIfDueTimeOrPeriodIsInvalid(dueTime, nameof(dueTime));
            ThrowIfDueTimeOrPeriodIsInvalid(period,  nameof(period));
            if (!EnumUtility<PlayerLoopPhase>.IsDefined(playerLoopPhase))
            {
                throw new ArgumentOutOfRangeException(nameof(playerLoopPhase));
            }
            _callback        = callback;
            _state           = state;
            _dueTime         = dueTime;
            _period          = period;
            _playerLoopPhase = playerLoopPhase;
            Launch();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowIfDueTimeOrPeriodIsInvalid(TimeSpan dueTimeOrPeriod, string paramName)
        {
            if (dueTimeOrPeriod == Timeout.InfiniteTimeSpan || dueTimeOrPeriod >= TimeSpan.Zero &&
                dueTimeOrPeriod <= Constant.TimeSpan.TimerMaxSupportedTimeout)
            {
                return;
            }
            throw new ArgumentOutOfRangeException(paramName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Launch()
        {
            if (_dueTime == TimeSpan.Zero)
            {
                var version = _version;
                Trigger();
                if (_disposed || version != _version)
                {
                    return false;
                }
                _firstCallbackInvoked = true;
                if (_period == Timeout.InfiniteTimeSpan)
                {
                    return true;
                }
                PlayerLoopUtility.AddPlayerLoopItem(this, _playerLoopPhase);
                _scheduled = true;
                _stopwatch.Restart();
            }
            else if (_dueTime > TimeSpan.Zero)
            {
                PlayerLoopUtility.AddPlayerLoopItem(this, _playerLoopPhase);
                _scheduled = true;
                _stopwatch.Restart();
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Trigger()
        {
            _callback(this, _state);
        }

        void IPlayerLoopItem.Run(PlayerLoopPhase playerLoopPhase)
        {
            var lockTaken = false;
            try
            {
                Monitor.TryEnter(_lock, ref lockTaken);
                if (!lockTaken)
                {
                    return;
                }
                if (_disposed)
                {
                    return;
                }
                if (!_firstCallbackInvoked)
                {
                    if (_stopwatch.Elapsed < _dueTime)
                    {
                        return;
                    }
                    var version = _version;
                    Trigger();
                    if (_disposed || version != _version)
                    {
                        return;
                    }
                    _firstCallbackInvoked = true;
                    if (_period == Timeout.InfiniteTimeSpan)
                    {
                        PlayerLoopUtility.RemovePlayerLoopItem(this, _playerLoopPhase);
                        _scheduled = false;
                    }
                    else
                    {
                        _stopwatch.Restart();
                    }
                }
                else
                {
                    if (_stopwatch.Elapsed < _period)
                    {
                        return;
                    }
                    var version = _version;
                    Trigger();
                    if (_disposed || version != _version)
                    {
                        return;
                    }
                    _stopwatch.Restart();
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(_lock);
                }
            }
        }

        /// <inheritdoc />
        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            ThrowIfDueTimeOrPeriodIsInvalid(dueTime, nameof(dueTime));
            ThrowIfDueTimeOrPeriodIsInvalid(period,  nameof(period));
            lock (_lock)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(typeof(StopwatchPlayerLoopTimer).FullName);
                }
                _dueTime = dueTime;
                _period  = period;
                if (_scheduled)
                {
                    PlayerLoopUtility.RemovePlayerLoopItem(this, _playerLoopPhase);
                    _scheduled = false;
                }
                _firstCallbackInvoked = false;
                Interlocked.Increment(ref _version);
                return Launch();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~StopwatchPlayerLoopTimer()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (_lock)
                {
                    if (_disposed)
                    {
                        return;
                    }
                    _callback = null;
                    _state    = null;
                    if (_scheduled)
                    {
                        PlayerLoopUtility.RemovePlayerLoopItem(this, _playerLoopPhase);
                        _scheduled = false;
                    }
                    _disposed = true;
                }
            }
            else
            {
                _callback = null;
                _state    = null;
            }
        }
    }
}
