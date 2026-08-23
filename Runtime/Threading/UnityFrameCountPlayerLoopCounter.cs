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
    /// A counter that uses <see cref="Time.frameCount"/> to count and handles non-immediate callbacks in a specific player loop phase.
    /// </summary>
    public sealed class UnityFrameCountPlayerLoopCounter : ICounter, IPlayerLoopItem
    {
        private readonly object _lock = new();

        private volatile bool _disposed;

        private CounterTriggerCallback _callback;

        private object _state;

        private int _dueCount;

        private int _period;

        private readonly PlayerLoopPhase _playerLoopPhase;

        private bool _scheduled;

        private float _targetFrameCount;

        private bool _delayGetTargetFrameCount;

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
        /// Initializes a new instance of the <see cref="UnityFrameCountPlayerLoopCounter"/> class.
        /// </summary>
        /// <param name="callback">The method executed when the counter triggers.</param>
        /// <param name="state">The second parameter passed to <see cref="callback"/>.</param>
        /// <param name="playerLoopPhase">The player loop phase.</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="playerLoopPhase"/> is not a member defined in the <see cref="PlayerLoopPhase"/> enum.</exception>
        public UnityFrameCountPlayerLoopCounter(
            CounterTriggerCallback callback,
            object                 state,
            PlayerLoopPhase        playerLoopPhase)
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
            _dueCount        = -1;
            _period          = -1;
            _playerLoopPhase = playerLoopPhase;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityFrameCountPlayerLoopCounter"/> class.
        /// </summary>
        /// <param name="callback">The method executed when the counter triggers.</param>
        /// <param name="state">The second parameter passed to <see cref="callback"/>.</param>
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
        /// <param name="playerLoopPhase">The player loop phase.</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dueCount"/> or <paramref name="period"/> is less than 0 but not -1; or <paramref name="playerLoopPhase"/>'s value is undefined.</exception>
        public UnityFrameCountPlayerLoopCounter(
            CounterTriggerCallback callback,
            object                 state,
            int                    dueCount,
            int                    period,
            PlayerLoopPhase        playerLoopPhase)
        {
#if UNITY_EDITOR
            ThrowIfNotPlaying();
#endif
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }
            ThrowIfDueCountOrPeriodIsInvalid(dueCount, nameof(dueCount));
            ThrowIfDueCountOrPeriodIsInvalid(period,   nameof(period));
            if (!EnumUtility<PlayerLoopPhase>.IsDefined(playerLoopPhase))
            {
                throw new ArgumentOutOfRangeException(nameof(playerLoopPhase));
            }
            _callback        = callback;
            _state           = state;
            _dueCount        = dueCount;
            _period          = period;
            _playerLoopPhase = playerLoopPhase;
            Launch();
        }

        private static int UnityFrameCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Time.frameCount;
        }

        /// <remarks>When accessing this property, <see cref="_period"/> is either positive or 0.</remarks>
        private int PeriodZeroIsOne
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _period != 0 ? _period : 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowIfDueCountOrPeriodIsInvalid(int dueCountOrPeriod, string paramName)
        {
            if (dueCountOrPeriod is -1 or >= 0)
            {
                return;
            }
            throw new ArgumentOutOfRangeException(paramName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool Launch()
        {
            if (_dueCount == 0)
            {
                var version = _version;
                Trigger();
                if (_disposed || version != _version)
                {
                    return false;
                }
                _firstCallbackInvoked = true;
                if (_period == -1)
                {
                    return true;
                }
                PlayerLoopUtility.AddPlayerLoopItem(this, _playerLoopPhase);
                _scheduled = true;
                if (UnityEnvironment.OnUnityMainThread)
                {
                    _targetFrameCount         = UnityFrameCount + PeriodZeroIsOne;
                    _delayGetTargetFrameCount = false;
                }
                else
                {
                    _delayGetTargetFrameCount = true;
                }
            }
            else if (_dueCount > 0)
            {
                PlayerLoopUtility.AddPlayerLoopItem(this, _playerLoopPhase);
                _scheduled = true;
                if (UnityEnvironment.OnUnityMainThread)
                {
                    _targetFrameCount         = UnityFrameCount + _dueCount;
                    _delayGetTargetFrameCount = false;
                }
                else
                {
                    _delayGetTargetFrameCount = true;
                }
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
                if (_delayGetTargetFrameCount)
                {
                    _targetFrameCount = UnityFrameCount + (!_firstCallbackInvoked ? _dueCount : PeriodZeroIsOne);
                    _delayGetTargetFrameCount = false;
                    return;
                }
                if (!_firstCallbackInvoked)
                {
                    if (UnityFrameCount != _targetFrameCount)
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
                    if (_period == -1)
                    {
                        PlayerLoopUtility.RemovePlayerLoopItem(this, _playerLoopPhase);
                        _scheduled = false;
                    }
                    else
                    {
                        _targetFrameCount = UnityFrameCount + PeriodZeroIsOne;
                    }
                }
                else
                {
                    if (UnityFrameCount != _targetFrameCount)
                    {
                        return;
                    }
                    var version = _version;
                    Trigger();
                    if (_disposed || version != _version)
                    {
                        return;
                    }
                    _targetFrameCount = UnityFrameCount + PeriodZeroIsOne;
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
        public bool Change(int dueCount, int period)
        {
            ThrowIfDueCountOrPeriodIsInvalid(dueCount, nameof(dueCount));
            ThrowIfDueCountOrPeriodIsInvalid(period,   nameof(period));
            lock (_lock)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(typeof(UnityFrameCountPlayerLoopCounter).FullName);
                }
                _dueCount = dueCount;
                _period   = period;
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

        ~UnityFrameCountPlayerLoopCounter()
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
