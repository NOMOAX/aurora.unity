using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Collections;
using Aurora.Diagnostics;
using Aurora.Unity.PlayerLoop;

namespace Aurora.Unity.Threading.Tasks
{
    /// <summary>
    /// 表示一个处理将任务排队到 Unity 主线程中的低级工作的对象。
    /// </summary>
    public class UnityMainThreadTaskScheduler : TaskScheduler, IPlayerLoopItem
    {
        private readonly Deque<Task> _tasks = new();

        private const PlayerLoopPhase PlayerLoopPhase = PlayerLoop.PlayerLoopPhase.UpdateYielded;

        /// <summary>
        /// 获取一个值，这个值指示是否可以对已排队的任务开始新一轮的处理。
        /// </summary>
        /// <remarks>保证只在 Unity 主线程中调用此成员。</remarks>
        protected virtual bool BeginProcess => true;

        /// <summary>
        /// 获取一个值，这个值指示是否可以继续处理下一个已排队的任务。
        /// </summary>
        /// <remarks>保证只在 Unity 主线程中调用此成员。</remarks>
        protected virtual bool Continue => true;

        /// <inheritdoc />
        public sealed override int MaximumConcurrencyLevel => 1;

        /// <inheritdoc />
        protected sealed override void QueueTask(Task task)
        {
            if ((task.CreationOptions & TaskCreationOptions.LongRunning) != 0)
            {
                Log.E(
                    $"为防止卡死 Unity 主线程，禁止将“带有 {nameof(TaskCreationOptions.LongRunning)} 任务创建选项的任务”和“带有 {nameof(TaskContinuationOptions.LongRunning)} 任务延续选项的延续任务”排队到此任务调度器！"
                );
                return;
            }
            lock (_tasks)
            {
                if (_tasks.Count == 0)
                {
                    PlayerLoopUtility.AddPlayerLoopItem(this, PlayerLoopPhase);
                }
                _tasks.EnqueueLast(task);
            }
        }

        /// <inheritdoc />
        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return (task.CreationOptions & (TaskCreationOptions.PreferFairness | TaskCreationOptions.LongRunning)) ==
                   0 && UnityEnvironment.OnUnityMainThread && (!taskWasPreviouslyQueued || TryDequeue(task)) &&
                   TryExecuteTask(task);
        }

        /// <inheritdoc />
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            var lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken)
                {
                    return _tasks;
                }
                throw new NotSupportedException("不支持在非 Unity 主线程中获取此任务调度器中的已排队任务");
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(_tasks);
                }
            }
        }

        /// <inheritdoc />
        protected sealed override bool TryDequeue(Task task)
        {
            var lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                return lockTaken && _tasks.Remove(task);
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(_tasks);
                }
            }
        }

        void IPlayerLoopItem.Run(PlayerLoopPhase playerLoopPhase)
        {
            if (_tasks.Count == 0)
            {
                PlayerLoopUtility.RemovePlayerLoopItem(this, playerLoopPhase);
            }
            else if (BeginProcess && _tasks.TryDequeueFirst(out var task))
            {
                do
                {
                    TryExecuteTask(task);
                } while (Continue && _tasks.TryDequeueFirst(out task));

                if (_tasks.Count == 0)
                {
                    PlayerLoopUtility.RemovePlayerLoopItem(this, playerLoopPhase);
                }
            }
        }
    }
}
