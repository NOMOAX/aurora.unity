using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Aurora.Unity.PlayerLoop;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Aurora.Unity.Threading
{
    /// <summary>
    /// A timer that uses <see cref="Time.unscaledTime">Time.unscaledTime</see> to time and handles non-immediate callbacks in a specific player loop phase.
    /// </summary>
    public sealed class UnityUnscaledTimePlayerLoopTimer : ITimer, IPlayerLoopItem
    {
        private readonly object _lock = new();

        private volatile bool _disposed;

        private TimerTriggerCallback _callback;

        private object _state;

        private TimeSpan _dueTime;

        private TimeSpan _period;

        private readonly PlayerLoopPhase _playerLoopPhase;

        private bool _scheduled;

        private double _startTime;

        private bool _delayGetStartTime;

        private bool _firstCallbackInvoked;

        private volatile int _version;

#if UNITY_EDITOR
        private static void ThrowIfNotPlaying()
        {
            if (!EditorApplication.isPlaying)
            {
                throw new NotSupportedException();
            }
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityUnscaledTimePlayerLoopTimer"/> class.
        /// </summary>
        /// <param name="callback">The method executed when the timer triggers.</param>
        /// <param name="state">The second parameter passed to <see cref="callback"/>.</param>
        /// <param name="playerLoopPhase">The player loop phase.</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="playerLoopPhase"/> is not a member defined in the <see cref="PlayerLoopPhase"/> enum.</exception>
        public UnityUnscaledTimePlayerLoopTimer(
            TimerTriggerCallback callback,
            object               state,
            PlayerLoopPhase      playerLoopPhase)
        {
#if UNITY_EDITOR
            ThrowIfNotPlaying();
#endif
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
        /// Initializes a new instance of the <see cref="UnityUnscaledTimePlayerLoopTimer"/> class.
        /// </summary>
        /// <param name="callback">The method executed when the timer triggers.</param>
        /// <param name="state">The second parameter passed to <see cref="callback"/>.</param>
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
        /// <param name="playerLoopPhase">The player loop phase.</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dueTime"/> or <paramref name="period"/> is not <see cref="Timeout.InfiniteTimeSpan">Timeout.InfiniteTimeSpan</see>, and their milliseconds are not in the [0, 4294967294] range; or <paramref name="playerLoopPhase"/>'s value is undefined.</exception>
        public UnityUnscaledTimePlayerLoopTimer(
            TimerTriggerCallback callback,
            object               state,
            TimeSpan             dueTime,
            TimeSpan             period,
            PlayerLoopPhase      playerLoopPhase)
        {
#if UNITY_EDITOR
            ThrowIfNotPlaying();
#endif
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

        private static double UnityTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Time.unscaledTimeAsDouble;
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
                InvokeCallback();
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
                if (UnityEnvironment.OnUnityMainThread)
                {
                    _startTime         = UnityTime;
                    _delayGetStartTime = false;
                }
                else
                {
                    _delayGetStartTime = true;
                }
            }
            else if (_dueTime > TimeSpan.Zero)
            {
                PlayerLoopUtility.AddPlayerLoopItem(this, _playerLoopPhase);
                _scheduled = true;
                if (UnityEnvironment.OnUnityMainThread)
                {
                    _startTime         = UnityTime;
                    _delayGetStartTime = false;
                }
                else
                {
                    _delayGetStartTime = true;
                }
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InvokeCallback()
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
                if (_delayGetStartTime)
                {
                    _startTime         = UnityTime;
                    _delayGetStartTime = false;
                    return;
                }
                if (!_firstCallbackInvoked)
                {
                    if (UnityTime - _startTime < _dueTime.TotalSeconds)
                    {
                        return;
                    }
                    var version = _version;
                    InvokeCallback();
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
                        _startTime = UnityTime;
                    }
                }
                else
                {
                    if (UnityTime - _startTime < _period.TotalSeconds)
                    {
                        return;
                    }
                    var version = _version;
                    InvokeCallback();
                    if (_disposed || version != _version)
                    {
                        return;
                    }
                    _startTime = UnityTime;
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
                    throw new ObjectDisposedException(typeof(UnityUnscaledTimePlayerLoopTimer).FullName);
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

        ~UnityUnscaledTimePlayerLoopTimer()
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
