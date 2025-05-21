using System;
using System.Runtime.InteropServices;
using UnityEngine.Assertions;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Aurora.Unity.PlayerLoop
{
    /// <summary>
    /// 提供与原生播放器循环有关的工具。
    /// </summary>
    public static class PlayerLoopUtility
    {
        private static readonly PlayerLoopRunner[] PlayerLoopRunners;

        private static readonly ContinuationRunner[] ContinuationRunners;

        private static readonly Plan[] Plans =
        {
            new(
                typeof(FixedUpdate),
                typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate),
                false,
                typeof(FixedUpdatingForPlayerLoopRunner),
                typeof(FixedUpdatingForContinuationRunner)
            ),
            new(
                typeof(FixedUpdate),
                typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate),
                true,
                typeof(FixedUpdatedForPlayerLoopRunner),
                typeof(FixedUpdatedForContinuationRunner)
            ),
            new(
                typeof(Update),
                typeof(Update.ScriptRunBehaviourUpdate),
                false,
                typeof(UpdatingForPlayerLoopRunner),
                typeof(UpdatingForContinuationRunner)
            ),
            new(
                typeof(Update),
                typeof(Update.ScriptRunBehaviourUpdate),
                true,
                typeof(UpdatedForPlayerLoopRunner),
                typeof(UpdatedForContinuationRunner)
            ),
            new(
                typeof(Update),
                typeof(Update.ScriptRunDelayedDynamicFrameRate),
                true,
                typeof(UpdateYieldedForPlayerLoopRunner),
                typeof(UpdateYieldedForContinuationRunner)
            ),
            new(
                typeof(Update),
                typeof(Update.ScriptRunDelayedTasks),
                true,
                typeof(UpdatePostedForPlayerLoopRunner),
                typeof(UpdatePostedForContinuationRunner)
            ),
            new(
                typeof(PreLateUpdate),
                typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate),
                false,
                typeof(LateUpdatingForPlayerLoopRunner),
                typeof(LateUpdatingForContinuationRunner)
            ),
            new(
                typeof(PreLateUpdate),
                typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate),
                true,
                typeof(LateUpdatedForPlayerLoopRunner),
                typeof(LateUpdatedForContinuationRunner)
            )
        };

#if UNITY_EDITOR
        private static bool IsClearing { get; set; }
#endif

        static PlayerLoopUtility()
        {
            Assert.AreEqual(EnumUtility<PlayerLoopPhase>.Count, Plans.Length);

            PlayerLoopRunners = new PlayerLoopRunner[EnumUtility<PlayerLoopPhase>.Count];
            for (var i = 0; i < EnumUtility<PlayerLoopPhase>.Count; i++)
            {
                PlayerLoopRunners[i] = new PlayerLoopRunner(EnumUtility<PlayerLoopPhase>.Values[i]);
            }

            ContinuationRunners = new ContinuationRunner[EnumUtility<PlayerLoopPhase>.Count];
            for (var i = 0; i < EnumUtility<PlayerLoopPhase>.Count; i++)
            {
                ContinuationRunners[i] = new ContinuationRunner(EnumUtility<PlayerLoopPhase>.Values[i]);
            }
        }

        /// <summary>
        /// 当前播放器循环阶段。
        /// </summary>
        public static PlayerLoopPhase? CurrentPhase { get; internal set; }

        /// <summary>
        /// 添加播放器更新循环操作。
        /// </summary>
        /// <param name="item">播放器更新循环操作。</param>
        /// <param name="phase">播放器循环阶段。</param>
        public static void AddPlayerLoopItem(IPlayerLoopItem item, PlayerLoopPhase phase)
        {
#if UNITY_EDITOR
            if (IsClearing)
            {
                return;
            }
#endif
            if (item == null)
            {
                return;
            }
            var playerLoopRunner = PlayerLoopRunners[(int) phase];
            playerLoopRunner.Add(item);
        }

        /// <summary>
        /// 移除播放器更新循环操作。
        /// </summary>
        /// <param name="item">播放器更新循环操作。</param>
        /// <param name="phase">播放器循环阶段。</param>
        public static void RemovePlayerLoopItem(IPlayerLoopItem item, PlayerLoopPhase phase)
        {
#if UNITY_EDITOR
            if (IsClearing)
            {
                return;
            }
#endif
            if (item == null)
            {
                return;
            }
            var playerLoopRunner = PlayerLoopRunners[(int) phase];
            playerLoopRunner.Remove(item);
        }

        /// <summary>
        /// 添加延续操作。
        /// </summary>
        /// <param name="continuation">延续操作。</param>
        /// <param name="phase">播放器循环阶段。</param>
        public static void AddContinuation(Action continuation, PlayerLoopPhase phase)
        {
#if UNITY_EDITOR
            if (IsClearing)
            {
                return;
            }
#endif
            if (continuation == null)
            {
                return;
            }
            var continuationRunner = ContinuationRunners[(int) phase];
            continuationRunner.Add(continuation);
        }

        /// <summary>
        /// 添加延续操作。
        /// </summary>
        /// <param name="continuation">延续操作。</param>
        /// <param name="state">由延续操作使用的参数。</param>
        /// <param name="phase">播放器循环阶段。</param>
        public static void AddContinuation(Action<object> continuation, object state, PlayerLoopPhase phase)
        {
#if UNITY_EDITOR
            if (IsClearing)
            {
                return;
            }
#endif
            if (continuation == null)
            {
                return;
            }
            var runner = ContinuationRunners[(int) phase];
            runner.Add(continuation, state);
        }

        /// <summary>
        /// 添加延续操作。
        /// </summary>
        /// <param name="continuationInvocation">延续操作。</param>
        /// <param name="phase">播放器循环阶段。</param>
        public static void AddContinuation(Invocation continuationInvocation, PlayerLoopPhase phase)
        {
#if UNITY_EDITOR
            if (IsClearing)
            {
                return;
            }
#endif
            if (continuationInvocation == null)
            {
                return;
            }
            var continuationRunner = ContinuationRunners[(int) phase];
            continuationRunner.Add(continuationInvocation);
        }

#if UNITY_EDITOR
        internal static void Clear()
        {
            Assert.IsFalse(IsClearing);
            IsClearing = true;
            try
            {
                for (var i = 0; i < EnumUtility<PlayerLoopPhase>.Count; i++)
                {
                    var playerLoopRunner = PlayerLoopRunners[i];
                    playerLoopRunner.Clear();

                    var continuationRunner = ContinuationRunners[i];
                    continuationRunner.Clear();
                }
            }
            finally
            {
                IsClearing = false;
            }
        }
#endif

#if UNITY_EDITOR
        internal static void Run()
        {
            for (var i = 0; i < EnumUtility<PlayerLoopPhase>.Count; i++)
            {
                var playerLoopRunner = PlayerLoopRunners[i];
                playerLoopRunner.Run();

                var continuationRunner = ContinuationRunners[i];
                continuationRunner.Run();
            }
        }
#endif

        internal static void Initialize()
        {
            var system     = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            var hasChanged = false;
            for (var i = 0; i < EnumUtility<PlayerLoopPhase>.Count; i++)
            {
                var plan                                = Plans[i];
                var categoryType                        = plan.CategoryType;
                var anchorSubsystemType                 = plan.AnchorSubsystemType;
                var insertAfter                         = plan.InsertAfter;
                var playerLoopSubsystemType             = plan.PlayerLoopSubsystemType;
                var playerLoopSubsystemUpdateDelegate   = (PlayerLoopSystem.UpdateFunction) PlayerLoopRunners[i].Run;
                var continuationSubsystemType           = plan.ContinuationSubsystemType;
                var continuationSubsystemUpdateDelegate = (PlayerLoopSystem.UpdateFunction) ContinuationRunners[i].Run;
                hasChanged |= RemoveAddPlayerLoopSubsystemAndContinuationSubsystem(
                    ref system,
                    categoryType,
                    anchorSubsystemType,
                    insertAfter,
                    playerLoopSubsystemType,
                    playerLoopSubsystemUpdateDelegate,
                    continuationSubsystemType,
                    continuationSubsystemUpdateDelegate
                );
            }
            if (hasChanged)
            {
                UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(system);
            }
        }

        private static bool RemoveAddPlayerLoopSubsystemAndContinuationSubsystem(
            ref PlayerLoopSystem            system,
            Type                            categoryType,
            Type                            anchorSubsystemType,
            bool                            insertAfter,
            Type                            playerLoopSubsystemType,
            PlayerLoopSystem.UpdateFunction playerLoopSubsystemUpdateDelegate,
            Type                            continuationSubsystemType,
            PlayerLoopSystem.UpdateFunction continuationSubsystemUpdateDelegate)
        {
            var removePlayerLoopSubsystem   = RemoveSubsystem(ref system, categoryType, playerLoopSubsystemType);
            var removeContinuationSubsystem = RemoveSubsystem(ref system, categoryType, continuationSubsystemType);
            var playerLoopSubsystem = new PlayerLoopSystem
            {
                type           = playerLoopSubsystemType,
                updateDelegate = playerLoopSubsystemUpdateDelegate
            };
            var continuationSubsystem = new PlayerLoopSystem
            {
                type           = continuationSubsystemType,
                updateDelegate = continuationSubsystemUpdateDelegate
            };
            var insertPlayerLoopSubsystemAndContinuationSubsystem = InsertPlayerLoopSubsystemAndContinuationSubsystem(
                ref system,
                categoryType,
                anchorSubsystemType,
                insertAfter,
                playerLoopSubsystem,
                continuationSubsystem
            );
            return removePlayerLoopSubsystem || removeContinuationSubsystem ||
                   insertPlayerLoopSubsystemAndContinuationSubsystem;
        }

        private static bool InsertPlayerLoopSubsystemAndContinuationSubsystem(
            ref PlayerLoopSystem system,
            Type                 categoryType,
            Type                 anchorSubsystemType,
            bool                 insertAfter,
            PlayerLoopSystem     playerLoopSubsystem,
            PlayerLoopSystem     continuationSubsystem)
        {
            ref var categories = ref system.subSystemList;
            if (categories is null)
            {
                return false;
            }
            var categoryIndex = Array.FindIndex(categories, e => e.type == categoryType);
            if (categoryIndex < 0)
            {
                return false;
            }
            ref var category = ref categories[categoryIndex];
            return InsertPlayerLoopSubsystemAndContinuationSubsystem(
                ref category,
                anchorSubsystemType,
                insertAfter,
                playerLoopSubsystem,
                continuationSubsystem
            );
        }

        private static bool InsertPlayerLoopSubsystemAndContinuationSubsystem(
            ref PlayerLoopSystem category,
            Type                 anchorSubsystemType,
            bool                 insertAfter,
            PlayerLoopSystem     playerLoopSubsystem,
            PlayerLoopSystem     continuationSubsystem)
        {
            ref var subsystems = ref category.subSystemList;
            if (subsystems is null)
            {
                subsystems = new[] { playerLoopSubsystem, continuationSubsystem };
                return true;
            }
            var subsystemIndex = Array.FindIndex(subsystems, e => e.type == anchorSubsystemType);
            if (subsystemIndex < 0)
            {
                return false;
            }
            Insert(
                ref subsystems,
                playerLoopSubsystem,
                continuationSubsystem,
                subsystemIndex + Convert.ToInt32(insertAfter)
            );
            return true;
        }

        private static void Insert(ref PlayerLoopSystem[] array, PlayerLoopSystem o1, PlayerLoopSystem o2, int index)
        {
            var newArray = new PlayerLoopSystem[array.Length + 2];
            Array.Copy(array, 0, newArray, 0, index);
            newArray[index]     = o1;
            newArray[index + 1] = o2;
            Array.Copy(array, index, newArray, index + 2, array.Length - index);
            array = newArray;
        }

        private static bool RemoveSubsystem(ref PlayerLoopSystem system, Type categoryType, Type subsystemType)
        {
            ref var categories = ref system.subSystemList;
            if (categories is null)
            {
                return false;
            }
            var categoryIndex = Array.FindIndex(categories, e => e.type == categoryType);
            if (categoryIndex < 0)
            {
                return false;
            }
            ref var category = ref categories[categoryIndex];
            return RemoveSubsystem(ref category, subsystemType);
        }

        private static bool RemoveSubsystem(ref PlayerLoopSystem category, Type subsystemType)
        {
            ref var subsystems = ref category.subSystemList;
            if (subsystems is null)
            {
                return false;
            }
            var subsystemIndex = Array.FindIndex(subsystems, e => e.type == subsystemType);
            if (subsystemIndex < 0)
            {
                return false;
            }
            Remove(ref subsystems, subsystemIndex);
            return true;
        }

        private static void Remove(ref PlayerLoopSystem[] array, int index)
        {
            var newArray = new PlayerLoopSystem[array.Length - 1];
            Array.Copy(array, 0,         newArray, 0,     index);
            Array.Copy(array, index + 1, newArray, index, array.Length - index - 1);
            array = newArray;
        }

        private readonly struct Plan
        {
            internal readonly Type CategoryType;

            internal readonly Type AnchorSubsystemType;

            internal readonly bool InsertAfter;

            internal readonly Type PlayerLoopSubsystemType;

            internal readonly Type ContinuationSubsystemType;

            public Plan(
                Type categoryType,
                Type anchorSubsystemType,
                bool insertAfter,
                Type playerLoopSubsystemType,
                Type continuationSubsystemType)
            {
                CategoryType              = categoryType;
                AnchorSubsystemType       = anchorSubsystemType;
                InsertAfter               = insertAfter;
                PlayerLoopSubsystemType   = playerLoopSubsystemType;
                ContinuationSubsystemType = continuationSubsystemType;
            }
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly struct FixedUpdatingForPlayerLoopRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly struct FixedUpdatingForContinuationRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly struct FixedUpdatedForPlayerLoopRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly struct FixedUpdatedForContinuationRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly struct UpdatingForPlayerLoopRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly struct UpdatingForContinuationRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly struct UpdatedForPlayerLoopRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly struct UpdatedForContinuationRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly struct UpdateYieldedForPlayerLoopRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly struct UpdateYieldedForContinuationRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly struct UpdatePostedForPlayerLoopRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly struct UpdatePostedForContinuationRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly struct LateUpdatingForPlayerLoopRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly struct LateUpdatingForContinuationRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly struct LateUpdatedForPlayerLoopRunner
        {
        }

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        private readonly struct LateUpdatedForContinuationRunner
        {
        }
    }
}
