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
    /// 使用 <see cref="Time.frameCount"/> 计数并在特定的主循环阶段中处理非立即执行的回调的计数器。
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
        /// 初始化 <see cref="UnityFrameCountPlayerLoopCounter"/> 类的新实例。
        /// </summary>
        /// <param name="callback">当计数器触发时执行的方法。</param>
        /// <param name="state">将传递给 <see cref="callback"/> 的第二个形参。</param>
        /// <param name="playerLoopPhase">主循环阶段。</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="playerLoopPhase"/> 不是在 <see cref="PlayerLoopPhase"/> 枚举中定义的成员。</exception>
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
        /// 初始化 <see cref="UnityFrameCountPlayerLoopCounter"/> 类的新实例。
        /// </summary>
        /// <param name="callback">当计数器触发时执行的方法。</param>
        /// <param name="state">将传递给 <see cref="callback"/> 的第二个形参。</param>
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
        /// <param name="playerLoopPhase">主循环阶段。</param>
        /// <exception cref="ArgumentNullException"><paramref name="callback"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="dueCount"/> 或 <paramref name="period"/> 小于 0，但不为 -1；或者 <paramref name="playerLoopPhase"/> 的值未定义。</exception>
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

        /// <remarks>访问此属性时，<see cref="_period"/> 要么是正数，要么是 0。</remarks>
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
