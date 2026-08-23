using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Aurora.Interpolations;
using Aurora.Unity.PlayerLoop;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Aurora.Unity.Threading
{
    /// <summary>
    /// A timer that counts from a specified start point to an end point within a specific player loop phase.
    /// </summary>
    public sealed class PlayerLoopFromToTimer : IFromToTimer, IPlayerLoopItem
    {
        private static readonly FromToTimerValueChangedEventHandler TimeChangedEventHandler = OnTimeChanged;

        private static readonly FromToTimerValueChangedEventHandler TimeTruncatedChangedEventHandler =
            OnTimeTruncatedChanged;

        private static readonly FromToTimerValueChangedEventHandler ProgressChangedEventHandler = OnProgressChanged;

        private readonly object _lock = new();

        private volatile bool _disposed;

        private readonly PlayerLoopPhase _playerLoopPhase;

        private bool _running;

        private bool _useUnscaledTime;

        private double _from;

        private double _to;

        private double _time;

        private double _timeTruncated;

        /// <remarks>Because the initial values of <see cref="_from"/> and <see cref="_to"/> are equal, the initial progress is 1.</remarks>
        private double _progress = 1;

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
        /// Initializes a new instance of the <see cref="PlayerLoopFromToTimer"/> class.
        /// </summary>
        /// <param name="playerLoopPhase">The player loop phase.</param>
        public PlayerLoopFromToTimer(PlayerLoopPhase playerLoopPhase)
        {
#if UNITY_EDITOR
            ThrowIfNotPlaying();
#endif
            _playerLoopPhase = playerLoopPhase;
        }

        /// <inheritdoc />
        public bool Running
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (_lock)
                {
                    ThrowIfDisposed();
                    return _running;
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                lock (_lock)
                {
                    ThrowIfDisposed();
                    if (_running == value)
                    {
                        return;
                    }
                    if (value)
                    {
                        PlayerLoopUtility.AddPlayerLoopItem(this, _playerLoopPhase);
                    }
                    else
                    {
                        PlayerLoopUtility.RemovePlayerLoopItem(this, _playerLoopPhase);
                    }
                    _running = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether unscaled time is used to advance the current time toward the end point. If <see langword="false"/>, <see cref="UnityEngine.Time.deltaTime">UnityEngine.Time.deltaTime</see> is used; otherwise <see cref="UnityEngine.Time.unscaledDeltaTime">UnityEngine.Time.unscaledDeltaTime</see> is used.
        /// </summary>
        public bool UseUnscaledTime { get => _useUnscaledTime; set => _useUnscaledTime = value; }

        /// <inheritdoc />
        public double From
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (_lock)
                {
                    ThrowIfDisposed();
                    return _from;
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                lock (_lock)
                {
                    ThrowIfDisposed();
                    SetFrom(FromToTimerValueChangingCausation.Modification, value);
                }
            }
        }

        /// <inheritdoc />
        public double To
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (_lock)
                {
                    ThrowIfDisposed();
                    return _to;
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                lock (_lock)
                {
                    ThrowIfDisposed();
                    SetTo(FromToTimerValueChangingCausation.Modification, value);
                }
            }
        }

        /// <inheritdoc />
        public double Time
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (_lock)
                {
                    ThrowIfDisposed();
                    return _time;
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                lock (_lock)
                {
                    ThrowIfDisposed();
                    SetTime(FromToTimerValueChangingCausation.Modification, value);
                }
            }
        }

        /// <inheritdoc />
        public double TimeTruncated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (_lock)
                {
                    ThrowIfDisposed();
                    return _timeTruncated;
                }
            }
        }

        /// <inheritdoc />
        public double Progress
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                lock (_lock)
                {
                    ThrowIfDisposed();
                    return _progress;
                }
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                lock (_lock)
                {
                    ThrowIfDisposed();
                    SetProgress(FromToTimerValueChangingCausation.Modification, value);
                }
            }
        }

        private double DeltaTime
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return _useUnscaledTime switch
                {
                    false => UnityEngine.Time.deltaTime,
                    true  => UnityEngine.Time.unscaledDeltaTime
                };
            }
        }

        private FromToTimerValueChangedEventHandler _timeChanged;

        /// <inheritdoc />
        public event FromToTimerValueChangedEventHandler TimeChanged
        {
            add
            {
                ThrowIfDisposed();
                _timeChanged += value;
            }
            remove
            {
                ThrowIfDisposed();
                _timeChanged -= value;
            }
        }

        private FromToTimerValueChangedEventHandler _timeTruncatedChanged;

        /// <inheritdoc />
        public event FromToTimerValueChangedEventHandler TimeTruncatedChanged
        {
            add
            {
                ThrowIfDisposed();
                _timeTruncatedChanged += value;
            }
            remove
            {
                ThrowIfDisposed();
                _timeTruncatedChanged -= value;
            }
        }

        private FromToTimerValueChangedEventHandler _progressChanged;

        /// <inheritdoc />
        public event FromToTimerValueChangedEventHandler ProgressChanged
        {
            add
            {
                ThrowIfDisposed();
                _progressChanged += value;
            }
            remove
            {
                ThrowIfDisposed();
                _progressChanged -= value;
            }
        }

        private FromToTimerCompletedEventHandler _completed;

        /// <inheritdoc />
        public event FromToTimerCompletedEventHandler Completed
        {
            add
            {
                ThrowIfDisposed();
                _completed += value;
            }
            remove
            {
                ThrowIfDisposed();
                _completed -= value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnTimeChanged(in FromToTimerValueChangedEventArgs args)
        {
            _timeChanged?.Invoke(this, in args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnTimeTruncatedChanged(in FromToTimerValueChangedEventArgs args)
        {
            _timeTruncatedChanged?.Invoke(this, in args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnProgressChanged(in FromToTimerValueChangedEventArgs args)
        {
            _progressChanged?.Invoke(this, in args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnCompleted()
        {
            _completed?.Invoke(this);
        }

        private static void OnTimeChanged(IFromToTimer timer, in FromToTimerValueChangedEventArgs args)
        {
            ((PlayerLoopFromToTimer)timer).OnTimeChanged(in args);
        }

        private static void OnTimeTruncatedChanged(IFromToTimer timer, in FromToTimerValueChangedEventArgs args)
        {
            ((PlayerLoopFromToTimer)timer).OnTimeTruncatedChanged(in args);
        }

        private static void OnProgressChanged(IFromToTimer timer, in FromToTimerValueChangedEventArgs args)
        {
            ((PlayerLoopFromToTimer)timer).OnProgressChanged(in args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetFrom(FromToTimerValueChangingCausation causation, double value)
        {
            if (value is double.NaN || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
            if (_from == value)
            {
                return;
            }
            _from = value;
            var newTime = ClampTime(_time, _from, _to);
            if (SetValueAndRaiseEvent(this, causation, ref _time, newTime, TimeChangedEventHandler))
            {
                var newTimeTruncated = Math.Truncate(_time);
                SetValueAndRaiseEvent(
                    this,
                    causation,
                    ref _timeTruncated,
                    newTimeTruncated,
                    TimeTruncatedChangedEventHandler
                );
            }
            var newProgress = GetProgress(_from, _to, _time);
            SetValueAndRaiseEvent(this, causation, ref _progress, newProgress, ProgressChangedEventHandler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetTo(FromToTimerValueChangingCausation causation, double value)
        {
            if (value is double.NaN)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
            if (_to == value)
            {
                return;
            }
            _to = value;
            var newTime = ClampTime(_time, _from, _to);
            if (SetValueAndRaiseEvent(this, causation, ref _time, newTime, TimeChangedEventHandler))
            {
                var newTimeTruncated = Math.Truncate(_time);
                SetValueAndRaiseEvent(
                    this,
                    causation,
                    ref _timeTruncated,
                    newTimeTruncated,
                    TimeTruncatedChangedEventHandler
                );
            }
            var newProgress = GetProgress(_from, _to, _time);
            SetValueAndRaiseEvent(this, causation, ref _progress, newProgress, ProgressChangedEventHandler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetTime(FromToTimerValueChangingCausation causation, double value)
        {
            if (value is double.NaN)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
            var newTime = ClampTime(value, _from, _to);
            if (SetValueAndRaiseEvent(this, causation, ref _time, newTime, TimeChangedEventHandler))
            {
                var newTimeTruncated = Math.Truncate(_time);
                SetValueAndRaiseEvent(
                    this,
                    causation,
                    ref _timeTruncated,
                    newTimeTruncated,
                    TimeTruncatedChangedEventHandler
                );
                var newProgress = GetProgress(_from, _to, _time);
                SetValueAndRaiseEvent(this, causation, ref _progress, newProgress, ProgressChangedEventHandler);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetProgress(FromToTimerValueChangingCausation causation, double value)
        {
            if (value is double.NaN or < 0 or > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
            var newProgress = _from != _to ? value : 1;
            if (SetValueAndRaiseEvent(this, causation, ref _progress, newProgress, ProgressChangedEventHandler))
            {
                var newTime = Lerp(_from, _to, _progress);
                if (SetValueAndRaiseEvent(this, causation, ref _time, newTime, TimeChangedEventHandler))
                {
                    var newTimeTruncated = Math.Truncate(_time);
                    SetValueAndRaiseEvent(
                        this,
                        causation,
                        ref _timeTruncated,
                        newTimeTruncated,
                        TimeTruncatedChangedEventHandler
                    );
                }
            }
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

                const FromToTimerValueChangingCausation causation = FromToTimerValueChangingCausation.Timing;

                var newTime = MoveTowards(_time, _to, DeltaTime);
                if (SetValueAndRaiseEvent(this, causation, ref _time, newTime, TimeChangedEventHandler))
                {
                    var newTimeTruncated = Math.Truncate(_time);
                    SetValueAndRaiseEvent(
                        this,
                        causation,
                        ref _timeTruncated,
                        newTimeTruncated,
                        TimeTruncatedChangedEventHandler
                    );
                    var newProgress = GetProgress(_from, _to, _time);
                    SetValueAndRaiseEvent(this, causation, ref _progress, newProgress, ProgressChangedEventHandler);
                }
                if (_time != _to)
                {
                    return;
                }
                PlayerLoopUtility.RemovePlayerLoopItem(this, _playerLoopPhase);
                _running = false;
                OnCompleted();
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(_lock);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(typeof(PlayerLoopFromToTimer).FullName);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double MoveTowards(double current, double target, double maxDelta)
        {
            return Math.Abs(target - current) <= maxDelta ? target : current + Sign(target - current) * maxDelta;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Sign(double f)
        {
            return f >= 0 ? 1 : -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double ClampTime(double time, double from, double to)
        {
            return from < to
                       ? Clamp(time, from, to)
                       : from > to
                           ? Clamp(time, to, from)
                           : from;
        }

        private static double Clamp(double value, double min, double max)
        {
            return value < min
                       ? min
                       : value > max
                           ? max
                           : value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double GetProgress(double from, double to, double time)
        {
            return InterpolationUtility.InverseLinearInterpolate(from, to, time, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Lerp(double from, double to, double progress)
        {
            if (!double.IsInfinity(to))
            {
                return InterpolationUtility.LinearInterpolate(from, to, progress);
            }
            return progress == 0 ? from : to;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SetValueAndRaiseEvent(
            IFromToTimer                        timer,
            FromToTimerValueChangingCausation   causation,
            ref double                          value,
            double                              newValue,
            FromToTimerValueChangedEventHandler eventHandler)
        {
            var previousValue = value;
            if (previousValue == newValue)
            {
                return false;
            }
            value = newValue;
            var args = new FromToTimerValueChangedEventArgs(causation, previousValue, newValue);
            eventHandler(timer, in args);
            return true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PlayerLoopFromToTimer()
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
                    _timeChanged          = null;
                    _timeTruncatedChanged = null;
                    _progressChanged      = null;
                    _completed            = null;
                    if (_running)
                    {
                        PlayerLoopUtility.RemovePlayerLoopItem(this, _playerLoopPhase);
                        _running = false;
                    }
                    _disposed = true;
                }
            }
            else
            {
                _timeChanged          = null;
                _timeTruncatedChanged = null;
                _progressChanged      = null;
                _completed            = null;
            }
        }
    }
}
