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
    /// Represents an object that handles the low-level work of queueing tasks onto the Unity main thread.
    /// </summary>
    public class UnityMainThreadTaskScheduler : TaskScheduler, IPlayerLoopItem
    {
        private readonly Deque<Task> _tasks = new();

        private const PlayerLoopPhase PlayerLoopPhase = PlayerLoop.PlayerLoopPhase.UpdateYielded;

        /// <summary>
        /// Gets a value indicating whether a new round of processing can begin for queued tasks.
        /// </summary>
        /// <remarks>Guaranteed to be called only on the Unity main thread.</remarks>
        protected virtual bool BeginProcess => true;

        /// <summary>
        /// Gets a value indicating whether the next queued task can be processed.
        /// </summary>
        /// <remarks>Guaranteed to be called only on the Unity main thread.</remarks>
        protected virtual bool Continue => true;

        /// <inheritdoc />
        public sealed override int MaximumConcurrencyLevel => 1;

        /// <inheritdoc />
        protected sealed override void QueueTask(Task task)
        {
            if ((task.CreationOptions & TaskCreationOptions.LongRunning) != 0)
            {
                Log.E(
                    $"To prevent deadlocking the Unity main thread, it is forbidden to queue \"tasks with the {nameof(TaskCreationOptions.LongRunning)} task creation option\" and \"continuation tasks with the {nameof(TaskContinuationOptions.LongRunning)} task continuation option\" to this task scheduler!"
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
                throw new NotSupportedException(
                    "Getting queued tasks from this task scheduler is not supported off the Unity main thread"
                );
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
