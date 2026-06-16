using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Threading;
using Aurora.Unity.PlayerLoop;
using UnityEngine;

namespace Aurora.Unity.Threading.Tasks
{
    /// <summary>
    /// 提供一组返回值为 <see cref="Task"/> 或 <see cref="Task{TResult}"/> 的方法。
    /// </summary>
    public static class UnityTasks
    {
        #region PlayerLoopPhase、Any PlayerLoopPhase

        /// <summary>
        /// 创建一个任务，该任务将在处于指定的主循环阶段时完成。
        /// </summary>
        /// <param name="playerLoopPhase">主循环阶段。</param>
        /// <returns>在处于指定的主循环阶段时完成的任务。</returns>
        public static Task WhenPlayerLoopPhase(PlayerLoopPhase playerLoopPhase)
        {
            return InternalWhenPlayerLoopPhase(playerLoopPhase, CancellationToken.None);
        }

        /// <summary>
        /// 创建一个任务，该任务将在处于指定的主循环阶段时完成。
        /// </summary>
        /// <param name="playerLoopPhase">主循环阶段。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>在处于指定的主循环阶段时完成的任务。</returns>
        public static Task WhenPlayerLoopPhase(PlayerLoopPhase playerLoopPhase, CancellationToken cancellationToken)
        {
            return InternalWhenPlayerLoopPhase(playerLoopPhase, cancellationToken);
        }

        private static Task InternalWhenPlayerLoopPhase(
            PlayerLoopPhase   playerLoopPhase,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }
            if (PlayerLoopUtility.CurrentPhase == playerLoopPhase)
            {
                return Task.CompletedTask;
            }
            var playerLoopPhasePromise = cancellationToken.CanBeCanceled switch
            {
                false => new PlayerLoopPhasePromise(playerLoopPhase),
                true  => new PlayerLoopPhasePromiseWithCancellation(playerLoopPhase, cancellationToken)
            };
            return playerLoopPhasePromise.Task;
        }

        /// <summary>
        /// 创建一个任务，该任务将在处于多个指定的主循环阶段中的任何一个时完成。
        /// </summary>
        /// <param name="playerLoopPhases">多个主循环阶段。</param>
        /// <returns>在处于多个指定的主循环阶段中的任何一个时完成的任务。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="playerLoopPhases"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="playerLoopPhases"/> 的长度为 0。</exception>
        public static Task WhenAnyPlayerLoopPhase(PlayerLoopPhase[] playerLoopPhases)
        {
            if (playerLoopPhases == null)
            {
                throw new ArgumentNullException(nameof(playerLoopPhases));
            }
            if (playerLoopPhases.Length == 0)
            {
                throw new ArgumentException();
            }
            return InternalWhenAnyPlayerLoopPhase(playerLoopPhases, CancellationToken.None);
        }

        /// <summary>
        /// 创建一个任务，该任务将在处于多个指定的主循环阶段中的任何一个时完成。
        /// </summary>
        /// <param name="playerLoopPhases">多个主循环阶段。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>在处于多个指定的主循环阶段中的任何一个时完成的任务。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="playerLoopPhases"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException"><paramref name="playerLoopPhases"/> 的长度为 0。</exception>
        public static Task WhenAnyPlayerLoopPhase(
            PlayerLoopPhase[] playerLoopPhases,
            CancellationToken cancellationToken)
        {
            if (playerLoopPhases == null)
            {
                throw new ArgumentNullException(nameof(playerLoopPhases));
            }
            if (playerLoopPhases.Length == 0)
            {
                throw new ArgumentException();
            }
            return InternalWhenAnyPlayerLoopPhase(playerLoopPhases, cancellationToken);
        }

        private static Task InternalWhenAnyPlayerLoopPhase(
            PlayerLoopPhase[] playerLoopPhases,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }
            if (PlayerLoopUtility.CurrentPhase.HasValue && Array.IndexOf(
                    playerLoopPhases,
                    PlayerLoopUtility.CurrentPhase.Value
                ) >= 0)
            {
                return Task.CompletedTask;
            }
            var playerLoopPhasePromise = cancellationToken.CanBeCanceled switch
            {
                false => new PlayerLoopPhasePromise(playerLoopPhases),
                true  => new PlayerLoopPhasePromiseWithCancellation(playerLoopPhases, cancellationToken)
            };
            return playerLoopPhasePromise.Task;
        }

        private class PlayerLoopPhasePromise : TaskCompletionSource<VoidResult>
        {
            private static readonly Action<object> PromiseComplete =
                state => ((PlayerLoopPhasePromise) state).Complete();

            internal PlayerLoopPhasePromise(PlayerLoopPhase playerLoopPhase)
            {
                PlayerLoopUtility.AddContinuation(PromiseComplete, this, playerLoopPhase);
            }

            internal PlayerLoopPhasePromise(PlayerLoopPhase[] playerLoopPhases)
            {
                foreach (var playerLoopPhase in playerLoopPhases)
                {
                    PlayerLoopUtility.AddContinuation(PromiseComplete, this, playerLoopPhase);
                }
            }

            private void Complete()
            {
                if (TrySetResult(new VoidResult()))
                {
                    CleanUp();
                }
            }

            protected virtual void CleanUp()
            {
            }
        }

        private sealed class PlayerLoopPhasePromiseWithCancellation : PlayerLoopPhasePromise
        {
            private static readonly Action<object> Cancel = state =>
            {
                var (playerLoopPhaseWithCancellation, cancellationToken) =
                    (Tuple<PlayerLoopPhasePromiseWithCancellation, CancellationToken>) state;
                if (playerLoopPhaseWithCancellation.TrySetCanceled(cancellationToken))
                {
                    playerLoopPhaseWithCancellation.CleanUp();
                }
            };

            private readonly CancellationTokenRegistration _cancellationTokenRegistration;

            internal PlayerLoopPhasePromiseWithCancellation(
                PlayerLoopPhase   playerLoopPhase,
                CancellationToken cancellationToken) : base(playerLoopPhase)
            {
                _cancellationTokenRegistration = cancellationToken.Register(
                    Cancel,
                    Tuple.Create(this, cancellationToken)
                );
                if (Task.IsCompleted)
                {
                    _cancellationTokenRegistration.Dispose();
                }
            }

            internal PlayerLoopPhasePromiseWithCancellation(
                PlayerLoopPhase[] playerLoopPhases,
                CancellationToken cancellationToken) : base(playerLoopPhases)
            {
                _cancellationTokenRegistration = cancellationToken.Register(
                    Cancel,
                    Tuple.Create(this, cancellationToken)
                );
                if (Task.IsCompleted)
                {
                    _cancellationTokenRegistration.Dispose();
                }
            }

            protected override void CleanUp()
            {
                _cancellationTokenRegistration.Dispose();
                base.CleanUp();
            }
        }

        #endregion

        #region AsyncOperation

        /// <summary>
        /// 创建一个任务，该任务将在 Unity 异步操作完成时完成。
        /// </summary>
        /// <param name="asyncOperation">Unity 异步操作。</param>
        /// <returns>在 Unity 异步操作完成时完成的任务。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="asyncOperation"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="asyncOperation"/> 已释放。</exception>
        public static Task WhenAsyncOperation(AsyncOperation asyncOperation)
        {
            if (asyncOperation == null)
            {
                throw new ArgumentNullException(nameof(asyncOperation));
            }
            return InternalWhenAsyncOperation(asyncOperation, CancellationToken.None);
        }

        /// <summary>
        /// 创建一个任务，该任务将在 Unity 异步操作完成时完成。
        /// </summary>
        /// <param name="asyncOperation">Unity 异步操作。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>在 Unity 异步操作完成时完成的任务。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="asyncOperation"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="asyncOperation"/> 已释放。</exception>
        public static Task WhenAsyncOperation(AsyncOperation asyncOperation, CancellationToken cancellationToken)
        {
            if (asyncOperation == null)
            {
                throw new ArgumentNullException(nameof(asyncOperation));
            }
            return InternalWhenAsyncOperation(asyncOperation, cancellationToken);
        }

        private static Task InternalWhenAsyncOperation(
            AsyncOperation    asyncOperation,
            CancellationToken cancellationToken)
        {
            if (AsyncOperationUtility.InternalIsDisposed(asyncOperation))
            {
                throw new ObjectDisposedException(asyncOperation.GetType().FullName);
            }
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }
            if (UnityEnvironment.OnUnityMainThread && asyncOperation.isDone)
            {
                return Task.CompletedTask;
            }
            var asyncOperationPromise = cancellationToken.CanBeCanceled switch
            {
                false => new AsyncOperationPromise(asyncOperation),
                true  => new AsyncOperationPromiseWithCancellation(asyncOperation, cancellationToken)
            };
            return asyncOperationPromise.Task;
        }

        private class AsyncOperationPromise : TaskCompletionSource<VoidResult>
        {
            private static readonly Action<Task, object> PromiseCompleteOrRegister =
                (_, state) => ((AsyncOperationPromise) state).CompleteOrRegister();

            private readonly AsyncOperation _asyncOperation;

            internal AsyncOperationPromise(AsyncOperation asyncOperation)
            {
                _asyncOperation = asyncOperation;

                if (UnityEnvironment.OnUnityMainThread)
                {
                    CompleteOrRegister();
                }
                else
                {
                    var whenMainThreadTask = InternalWhenAnyPlayerLoopPhase(
                        EnumUtility<PlayerLoopPhase>.Values,
                        CancellationToken.None
                    );
                    TaskUtility.ContinueWithSynchronously(whenMainThreadTask, PromiseCompleteOrRegister, this);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void CompleteOrRegister()
            {
                if (_asyncOperation.isDone)
                {
                    Complete();
                }
                else
                {
                    _asyncOperation.completed += Complete;
                }
            }

            private void Complete(AsyncOperation asyncOperation)
            {
                Complete();
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Complete()
            {
                if (TrySetResult(new VoidResult()))
                {
                    CleanUp();
                }
            }

            protected virtual void CleanUp()
            {
            }
        }

        private sealed class AsyncOperationPromiseWithCancellation : AsyncOperationPromise
        {
            private static readonly Action<object> Cancel = state =>
            {
                var (asyncOperationPromiseWithCancellation, cancellationToken) =
                    (Tuple<AsyncOperationPromiseWithCancellation, CancellationToken>) state;
                if (asyncOperationPromiseWithCancellation.TrySetCanceled(cancellationToken))
                {
                    asyncOperationPromiseWithCancellation.CleanUp();
                }
            };

            private readonly CancellationTokenRegistration _cancellationTokenRegistration;

            internal AsyncOperationPromiseWithCancellation(
                AsyncOperation    asyncOperation,
                CancellationToken cancellationToken) : base(asyncOperation)
            {
                _cancellationTokenRegistration = cancellationToken.Register(
                    Cancel,
                    Tuple.Create(this, cancellationToken)
                );
                if (Task.IsCompleted)
                {
                    _cancellationTokenRegistration.Dispose();
                }
            }

            protected override void CleanUp()
            {
                _cancellationTokenRegistration.Dispose();
                base.CleanUp();
            }
        }

        #endregion

        #region Delay

        /// <summary>
        /// 创建一个任务，该任务将在指定的延迟后完成。
        /// </summary>
        /// <param name="delay">延迟。</param>
        /// <param name="playerLoopPhase">主循环阶段。</param>
        /// <returns>表示时间延迟的任务。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delay"/> 不为 <see cref="Timeout.InfiniteTimeSpan">Timeout.InfiniteTimeSpan</see>，并且它的毫秒数不在 [0, 4294967294] 范围内。</exception>
        public static Task Delay(TimeSpan delay, PlayerLoopPhase playerLoopPhase)
        {
            ThrowIfDelayIsInvalid(delay, nameof(delay));
            return InternalDelay(delay, playerLoopPhase, CancellationToken.None);
        }

        /// <summary>
        /// 创建一个任务，该任务将在指定的延迟后完成。
        /// </summary>
        /// <param name="delay">延迟。</param>
        /// <param name="playerLoopPhase">主循环阶段。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>表示时间延迟的任务。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delay"/> 不为 <see cref="Timeout.InfiniteTimeSpan">Timeout.InfiniteTimeSpan</see>，并且它的毫秒数不在 [0, 4294967294] 范围内。</exception>
        public static Task Delay(TimeSpan delay, PlayerLoopPhase playerLoopPhase, CancellationToken cancellationToken)
        {
            ThrowIfDelayIsInvalid(delay, nameof(delay));
            return InternalDelay(delay, playerLoopPhase, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Task InternalDelay(
            TimeSpan          delay,
            PlayerLoopPhase   playerLoopPhase,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }
            if (delay == TimeSpan.Zero)
            {
                return Task.CompletedTask;
            }
            var delayPromise = cancellationToken.CanBeCanceled switch
            {
                false => new DelayPromise(delay, playerLoopPhase),
                true  => new DelayPromiseWithCancellation(delay, playerLoopPhase, cancellationToken)
            };
            return delayPromise.Task;
        }

        private class DelayPromise : TaskCompletionSource<VoidResult>
        {
            private static readonly TimerTriggerCallback PromiseComplete =
                (_, state) => ((DelayPromise) state).Complete();

            private readonly ITimer _timer;

            internal DelayPromise(TimeSpan delay, PlayerLoopPhase playerLoopPhase)
            {
                // 对于无限期的延迟，不创建计时器
                if (delay == Timeout.InfiniteTimeSpan)
                {
                    return;
                }
                _timer = new StopwatchPlayerLoopTimer(
                    PromiseComplete,
                    this,
                    delay,
                    Timeout.InfiniteTimeSpan,
                    playerLoopPhase
                );
                if (Task.IsCompleted)
                {
                    _timer.Dispose();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Complete()
            {
                if (TrySetResult(new VoidResult()))
                {
                    CleanUp();
                }
            }

            protected virtual void CleanUp()
            {
                _timer.Dispose();
            }
        }

        private sealed class DelayPromiseWithCancellation : DelayPromise
        {
            private static readonly Action<object> Cancel = state =>
            {
                var (delayPromiseWithCancellation, cancellationToken) =
                    (Tuple<DelayPromiseWithCancellation, CancellationToken>) state;
                if (delayPromiseWithCancellation.TrySetCanceled(cancellationToken))
                {
                    delayPromiseWithCancellation.CleanUp();
                }
            };

            private readonly CancellationTokenRegistration _cancellationTokenRegistration;

            internal DelayPromiseWithCancellation(
                TimeSpan          delay,
                PlayerLoopPhase   playerLoopPhase,
                CancellationToken cancellationToken) : base(delay, playerLoopPhase)
            {
                _cancellationTokenRegistration = cancellationToken.Register(
                    Cancel,
                    Tuple.Create(this, cancellationToken)
                );
                if (Task.IsCompleted)
                {
                    _cancellationTokenRegistration.Dispose();
                }
            }

            protected override void CleanUp()
            {
                _cancellationTokenRegistration.Dispose();
                base.CleanUp();
            }
        }

        #endregion

        #region Delay Unity Time

        /// <summary>
        /// 创建一个任务，该任务将在指定的延迟后完成。
        /// </summary>
        /// <param name="delay">延迟。</param>
        /// <param name="unscaled">如果为 <see langword="false"/>，则使用 <see cref="Time.time">Time.time</see> 来计时；如果为 <see langword="true"/>，则使用 <see cref="Time.unscaledTime">Time.unscaledTime</see> 来计时。</param>
        /// <param name="playerLoopPhase">主循环阶段。</param>
        /// <returns>表示时间延迟的任务。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delay"/> 不为 <see cref="Timeout.InfiniteTimeSpan">Timeout.InfiniteTimeSpan</see>，并且它的毫秒数不在 [0, 4294967294] 范围内。</exception>
        public static Task DelayUnityTime(TimeSpan delay, bool unscaled, PlayerLoopPhase playerLoopPhase)
        {
            ThrowIfDelayIsInvalid(delay, nameof(delay));
            return InternalDelayUnityTime(delay, unscaled, playerLoopPhase, CancellationToken.None);
        }

        /// <summary>
        /// 创建一个任务，该任务将在指定的延迟后完成。
        /// </summary>
        /// <param name="delay">延迟。</param>
        /// <param name="unscaled">如果为 <see langword="false"/>，则使用 <see cref="Time.time">Time.time</see> 来计时；如果为 <see langword="true"/>，则使用 <see cref="Time.unscaledTime">Time.unscaledTime</see> 来计时。</param>
        /// <param name="playerLoopPhase">主循环阶段。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>表示时间延迟的任务。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delay"/> 不为 <see cref="Timeout.InfiniteTimeSpan">Timeout.InfiniteTimeSpan</see>，并且它的毫秒数不在 [0, 4294967294] 范围内。</exception>
        public static Task DelayUnityTime(
            TimeSpan          delay,
            bool              unscaled,
            PlayerLoopPhase   playerLoopPhase,
            CancellationToken cancellationToken)
        {
            ThrowIfDelayIsInvalid(delay, nameof(delay));
            return InternalDelayUnityTime(delay, unscaled, playerLoopPhase, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Task InternalDelayUnityTime(
            TimeSpan          delay,
            bool              unscaled,
            PlayerLoopPhase   playerLoopPhase,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }
            if (delay == TimeSpan.Zero)
            {
                return Task.CompletedTask;
            }
            var delayPromise = cancellationToken.CanBeCanceled switch
            {
                false => new DelayUnityTimePromise(delay, unscaled, playerLoopPhase),
                true  => new DelayUnityTimePromiseWithCancellation(delay, unscaled, playerLoopPhase, cancellationToken)
            };
            return delayPromise.Task;
        }

        private class DelayUnityTimePromise : TaskCompletionSource<VoidResult>
        {
            private static readonly TimerTriggerCallback PromiseComplete = (_, state) =>
                ((DelayUnityTimePromise) state).Complete();

            private readonly ITimer _timer;

            internal DelayUnityTimePromise(TimeSpan delay, bool unscaled, PlayerLoopPhase playerLoopPhase)
            {
                // 对于无限期的延迟，不创建计时器
                if (delay == Timeout.InfiniteTimeSpan)
                {
                    return;
                }

                _timer = unscaled switch
                {
                    false => new UnityUnscaledTimePlayerLoopTimer(
                        PromiseComplete,
                        this,
                        delay,
                        Timeout.InfiniteTimeSpan,
                        playerLoopPhase
                    ),
                    true => new UnityTimePlayerLoopTimer(
                        PromiseComplete,
                        this,
                        delay,
                        Timeout.InfiniteTimeSpan,
                        playerLoopPhase
                    )
                };
                if (Task.IsCompleted)
                {
                    _timer.Dispose();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Complete()
            {
                if (TrySetResult(new VoidResult()))
                {
                    CleanUp();
                }
            }

            protected virtual void CleanUp()
            {
                _timer.Dispose();
            }
        }

        private sealed class DelayUnityTimePromiseWithCancellation : DelayUnityTimePromise
        {
            private static readonly Action<object> Cancel = state =>
            {
                var (delayUnityTimePromiseWithCancellation, cancellationToken) =
                    (Tuple<DelayUnityTimePromiseWithCancellation, CancellationToken>) state;
                if (delayUnityTimePromiseWithCancellation.TrySetCanceled(cancellationToken))
                {
                    delayUnityTimePromiseWithCancellation.CleanUp();
                }
            };

            private readonly CancellationTokenRegistration _cancellationTokenRegistration;

            internal DelayUnityTimePromiseWithCancellation(
                TimeSpan          delay,
                bool              unscaled,
                PlayerLoopPhase   playerLoopPhase,
                CancellationToken cancellationToken) : base(delay, unscaled, playerLoopPhase)
            {
                _cancellationTokenRegistration = cancellationToken.Register(
                    Cancel,
                    Tuple.Create(this, cancellationToken)
                );
                if (Task.IsCompleted)
                {
                    _cancellationTokenRegistration.Dispose();
                }
            }

            protected override void CleanUp()
            {
                _cancellationTokenRegistration.Dispose();
                base.CleanUp();
            }
        }

        #endregion

        #region Delay Frame

        /// <summary>
        /// 创建一个任务，该任务将在几帧后完成。
        /// </summary>
        /// <param name="frameCount">帧数。</param>
        /// <param name="playerLoopPhase">主循环阶段。</param>
        /// <returns>在几帧后完成的任务。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="frameCount"/> 小于 0，但不为 -1。</exception>
        public static Task DelayFrame(int frameCount, PlayerLoopPhase playerLoopPhase)
        {
            ThrowIfDelayFrameCountIsInvalid(frameCount, nameof(frameCount));
            return InternalDelayFrame(frameCount, playerLoopPhase, CancellationToken.None);
        }

        /// <summary>
        /// 创建一个任务，该任务将在几帧后完成。
        /// </summary>
        /// <param name="frameCount">帧数。</param>
        /// <param name="playerLoopPhase">主循环阶段。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>在几帧后完成的任务。</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="frameCount"/> 小于 0，但不为 -1。</exception>
        public static Task DelayFrame(
            int               frameCount,
            PlayerLoopPhase   playerLoopPhase,
            CancellationToken cancellationToken)
        {
            ThrowIfDelayFrameCountIsInvalid(frameCount, nameof(frameCount));
            return InternalDelayFrame(frameCount, playerLoopPhase, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Task InternalDelayFrame(
            int               frameCount,
            PlayerLoopPhase   playerLoopPhase,
            CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }
            if (frameCount == 0)
            {
                return Task.CompletedTask;
            }
            var delayFramePromise = cancellationToken.CanBeCanceled switch
            {
                false => new DelayFramePromise(frameCount, playerLoopPhase),
                true  => new DelayFramePromiseWithCancellation(frameCount, playerLoopPhase, cancellationToken)
            };
            return delayFramePromise.Task;
        }

        private class DelayFramePromise : TaskCompletionSource<VoidResult>
        {
            private static readonly CounterTriggerCallback PromiseComplete = (_, state) =>
                ((DelayFramePromise) state).Complete();

            private readonly ICounter _counter;

            internal DelayFramePromise(int frameCount, PlayerLoopPhase playerLoopPhase)
            {
                if (frameCount == -1)
                {
                    return;
                }
                _counter = new UnityFrameCountPlayerLoopCounter(PromiseComplete, this, frameCount, -1, playerLoopPhase);
                if (Task.IsCompleted)
                {
                    _counter.Dispose();
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void Complete()
            {
                if (TrySetResult(new VoidResult()))
                {
                    CleanUp();
                }
            }

            protected virtual void CleanUp()
            {
                _counter.Dispose();
            }
        }

        private sealed class DelayFramePromiseWithCancellation : DelayFramePromise
        {
            private static readonly Action<object> Cancel = state =>
            {
                var (delayFramePromiseWithCancellation, cancellationToken) =
                    (Tuple<DelayFramePromiseWithCancellation, CancellationToken>) state;
                if (delayFramePromiseWithCancellation.TrySetCanceled(cancellationToken))
                {
                    delayFramePromiseWithCancellation.CleanUp();
                }
            };

            private readonly CancellationTokenRegistration _cancellationTokenRegistration;

            internal DelayFramePromiseWithCancellation(
                int               frameCount,
                PlayerLoopPhase   playerLoopPhase,
                CancellationToken cancellationToken) : base(frameCount, playerLoopPhase)
            {
                _cancellationTokenRegistration = cancellationToken.Register(
                    Cancel,
                    Tuple.Create(this, cancellationToken)
                );
                if (Task.IsCompleted)
                {
                    _cancellationTokenRegistration.Dispose();
                }
            }

            protected override void CleanUp()
            {
                _cancellationTokenRegistration.Dispose();
                base.CleanUp();
            }
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowIfDelayIsInvalid(TimeSpan delay, string paramName)
        {
            if (delay == Timeout.InfiniteTimeSpan ||
                delay >= TimeSpan.Zero && delay <= Constant.TimeSpan.TimerMaxSupportedTimeout)
            {
                return;
            }
            throw new ArgumentOutOfRangeException(paramName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowIfDelayFrameCountIsInvalid(int frameCount, string paramName)
        {
            if (frameCount is -1 or >= 0)
            {
                return;
            }
            throw new ArgumentOutOfRangeException(paramName);
        }

        #region Capture Screenshot

        /// <summary>
        /// 创建一个任务，该任务将在捕获屏幕截图完成时完成。
        /// </summary>
        /// <param name="path">截图文件保存的路径。</param>
        /// <returns>在捕获屏幕截图完成时完成的任务。</returns>
        /// <remarks>截图为 PNG 文件。</remarks>
        public static Task WhenScreenshotCaptured(string path)
        {
            return InternalWhenScreenshotCaptured(path, CancellationToken.None);
        }

        /// <summary>
        /// 创建一个任务，该任务将在捕获屏幕截图完成时完成。
        /// </summary>
        /// <param name="path">截图文件保存的路径。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>在捕获屏幕截图完成时完成的任务。</returns>
        /// <remarks>截图为 PNG 文件。</remarks>
        public static Task WhenScreenshotCaptured(string path, CancellationToken cancellationToken)
        {
            return InternalWhenScreenshotCaptured(path, cancellationToken);
        }

        private static Task InternalWhenScreenshotCaptured(string path, CancellationToken cancellationToken)
        {
            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(path);
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }
            string directoryName;
            string fileName;
            bool   exists;
            try
            {
                var fileInfo = new FileInfo(fullPath);
                directoryName = fileInfo.DirectoryName;
                fileName      = fileInfo.Name;
                exists        = fileInfo.Exists;
                if (exists)
                {
                    // 检查文件是否被占用
                    using (fileInfo.Open(FileMode.Open, FileAccess.Write, FileShare.Read))
                    {
                    }
                }
            }
            catch (Exception e)
            {
                return Task.FromException(e);
            }
            var fileCreatedOrChangedPromise = cancellationToken.CanBeCanceled switch
            {
                false => new FileCreatedOrChangedPromise(directoryName, fileName, exists),
                true => new FileCreatedOrChangedPromiseWithCancellation(
                    directoryName,
                    fileName,
                    exists,
                    cancellationToken
                )
            };
            ScreenCapture.CaptureScreenshot(path);
            return fileCreatedOrChangedPromise.Task;
        }

        private class FileCreatedOrChangedPromise : TaskCompletionSource<VoidResult>
        {
            private readonly FileSystemWatcher _watcher;

            internal FileCreatedOrChangedPromise(string directoryName, string fileName, bool exists)
            {
                _watcher = new FileSystemWatcher(directoryName, fileName);
                if (!exists)
                {
                    _watcher.Created += OnCreatedOrChanged;
                }
                else
                {
                    _watcher.Changed += OnCreatedOrChanged;
                }
                _watcher.Error               += OnError;
                _watcher.EnableRaisingEvents =  true;
            }

            private void OnCreatedOrChanged(object sender, FileSystemEventArgs e)
            {
                if (TrySetResult(new VoidResult()))
                {
                    Cleanup();
                }
            }

            private void OnError(object sender, ErrorEventArgs e)
            {
                if (TrySetException(e.GetException()))
                {
                    Cleanup();
                }
            }

            protected virtual void Cleanup()
            {
                _watcher.Dispose();
            }
        }

        private sealed class FileCreatedOrChangedPromiseWithCancellation : FileCreatedOrChangedPromise
        {
            private static readonly Action<object> Cancel = state =>
            {
                var (fileCreatedOrChangedPromiseWithCancellation, cancellationToken) =
                    (Tuple<FileCreatedOrChangedPromiseWithCancellation, CancellationToken>) state;
                if (fileCreatedOrChangedPromiseWithCancellation.TrySetCanceled(cancellationToken))
                {
                    fileCreatedOrChangedPromiseWithCancellation.Cleanup();
                }
            };

            private readonly CancellationTokenRegistration _cancellationTokenRegistration;

            internal FileCreatedOrChangedPromiseWithCancellation(
                string            directoryName,
                string            fileName,
                bool              exists,
                CancellationToken cancellationToken) : base(directoryName, fileName, exists)
            {
                _cancellationTokenRegistration = cancellationToken.Register(
                    Cancel,
                    Tuple.Create(this, cancellationToken)
                );
                if (Task.IsCompleted)
                {
                    _cancellationTokenRegistration.Dispose();
                }
            }

            protected override void Cleanup()
            {
                _cancellationTokenRegistration.Dispose();
                base.Cleanup();
            }
        }

        #endregion
    }
}
