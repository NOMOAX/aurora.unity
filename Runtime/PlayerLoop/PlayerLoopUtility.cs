using System;
using System.Runtime.InteropServices;
using Aurora.Collections;
using Aurora.Diagnostics;
using UnityEngine.Assertions;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Aurora.Unity.PlayerLoop
{
    /// <summary>
    /// Provides utilities related to the Unity player loop.
    /// </summary>
    public static class PlayerLoopUtility
    {
        private const string PlayerLoopRootDescription = "the player loop root";

        private static readonly PlayerLoopRunner[] PlayerLoopRunners;

        private static readonly ContinuationRunner[] ContinuationRunners;

        private static readonly Plan[] Plans =
        {
            Plan.Before(
                Position.At(typeof(FixedUpdate), typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate)),
                typeof(FixedUpdatingForPlayerLoopRunner),
                typeof(FixedUpdatingForContinuationRunner)
            ),
            Plan.After(
                Position.At(typeof(FixedUpdate), typeof(FixedUpdate.ScriptRunBehaviourFixedUpdate)),
                typeof(FixedUpdatedForPlayerLoopRunner),
                typeof(FixedUpdatedForContinuationRunner)
            ),
            Plan.Before(
                Position.At(typeof(Update), typeof(Update.ScriptRunBehaviourUpdate)),
                typeof(UpdatingForPlayerLoopRunner),
                typeof(UpdatingForContinuationRunner)
            ),
            Plan.After(
                Position.At(typeof(Update), typeof(Update.ScriptRunBehaviourUpdate)),
                typeof(UpdatedForPlayerLoopRunner),
                typeof(UpdatedForContinuationRunner)
            ),
            Plan.After(
                Position.At(typeof(Update), typeof(Update.ScriptRunDelayedDynamicFrameRate)),
                typeof(UpdateYieldedForPlayerLoopRunner),
                typeof(UpdateYieldedForContinuationRunner)
            ),
            Plan.After(
                Position.At(typeof(Update), typeof(Update.ScriptRunDelayedTasks)),
                typeof(UpdatePostedForPlayerLoopRunner),
                typeof(UpdatePostedForContinuationRunner)
            ),
            Plan.Before(
                Position.At(typeof(PreLateUpdate), typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate)),
                typeof(LateUpdatingForPlayerLoopRunner),
                typeof(LateUpdatingForContinuationRunner)
            ),
            Plan.After(
                Position.At(typeof(PreLateUpdate), typeof(PreLateUpdate.ScriptRunBehaviourLateUpdate)),
                typeof(LateUpdatedForPlayerLoopRunner),
                typeof(LateUpdatedForContinuationRunner)
            )
        };

        private static readonly ParameterizedPredicate<PlayerLoopSystem, Type> PlayerLoopSystemHasType =
            (playerLoopSystem, type) => playerLoopSystem.type == type;

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
        /// The current player loop phase.
        /// </summary>
        public static PlayerLoopPhase? CurrentPhase { get; internal set; }

        /// <summary>
        /// Adds a player loop item.
        /// </summary>
        /// <param name="item">The player loop item.</param>
        /// <param name="phase">The player loop phase.</param>
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
            var playerLoopRunner = PlayerLoopRunners[(int)phase];
            playerLoopRunner.Add(item);
        }

        /// <summary>
        /// Removes a player loop item.
        /// </summary>
        /// <param name="item">The player loop item.</param>
        /// <param name="phase">The player loop phase.</param>
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
            var playerLoopRunner = PlayerLoopRunners[(int)phase];
            playerLoopRunner.Remove(item);
        }

        /// <summary>
        /// Adds a continuation.
        /// </summary>
        /// <param name="continuation">The continuation.</param>
        /// <param name="phase">The player loop phase.</param>
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
            var continuationRunner = ContinuationRunners[(int)phase];
            continuationRunner.Add(continuation);
        }

        /// <summary>
        /// Adds a continuation.
        /// </summary>
        /// <param name="continuation">The continuation.</param>
        /// <param name="state">The argument used by the continuation.</param>
        /// <param name="phase">The player loop phase.</param>
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
            var runner = ContinuationRunners[(int)phase];
            runner.Add(continuation, state);
        }

        /// <summary>
        /// Adds a continuation.
        /// </summary>
        /// <param name="continuationInvocation">The continuation.</param>
        /// <param name="phase">The player loop phase.</param>
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
            var continuationRunner = ContinuationRunners[(int)phase];
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
                var playerLoopSubsystemUpdateDelegate   = (PlayerLoopSystem.UpdateFunction)PlayerLoopRunners[i].Run;
                var continuationSubsystemUpdateDelegate = (PlayerLoopSystem.UpdateFunction)ContinuationRunners[i].Run;
                hasChanged |= ApplyPlanToPlayerLoop(
                    ref system,
                    Plans[i],
                    playerLoopSubsystemUpdateDelegate,
                    continuationSubsystemUpdateDelegate
                );
            }
            if (hasChanged)
            {
                UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(system);
            }
        }

        /// <summary>
        /// Removes the subsystem pair of a plan if it is already present, and inserts it before or after the system at the plan's position. The types of the position are searched starting from the root's sub-system list, so the last type is the anchor subsystem.
        /// </summary>
        /// <param name="system">The player loop system to search from.</param>
        /// <param name="plan">The plan.</param>
        /// <param name="playerLoopSubsystemUpdateDelegate">The update delegate of the player loop subsystem.</param>
        /// <param name="continuationSubsystemUpdateDelegate">The update delegate of the continuation subsystem.</param>
        /// <returns><see langword="true"/> if the player loop has been changed; otherwise, <see langword="false"/>.</returns>
        private static bool ApplyPlanToPlayerLoop(
            ref PlayerLoopSystem            system,
            Plan                            plan,
            PlayerLoopSystem.UpdateFunction playerLoopSubsystemUpdateDelegate,
            PlayerLoopSystem.UpdateFunction continuationSubsystemUpdateDelegate)
        {
            var subsystemTypes = plan.Position.SubsystemTypes;

            // Walk down to the system whose sub-system list contains the anchor subsystem.
            ref var anchorParent = ref system;
            for (var i = 0; i < subsystemTypes.Length - 1; i++)
            {
                ref var subsystems = ref anchorParent.subSystemList;
                if (subsystems is null)
                {
                    Log.E(
                        $"The player loop plan is not applied: {TypeUtility.GetNicelyFormattedName(subsystemTypes[i])} cannot be reached because {(anchorParent.type is not null ? TypeUtility.GetNicelyFormattedName(anchorParent.type) : PlayerLoopRootDescription)} has no sub-system list."
                    );
                    return false;
                }
                var subsystemIndex = subsystems.FindIndex(PlayerLoopSystemHasType, subsystemTypes[i]);
                if (subsystemIndex < 0)
                {
                    Log.E(
                        $"The player loop plan is not applied: {TypeUtility.GetNicelyFormattedName(subsystemTypes[i])} was not found in the sub-system list of {(anchorParent.type is not null ? TypeUtility.GetNicelyFormattedName(anchorParent.type) : PlayerLoopRootDescription)}."
                    );
                    return false;
                }
                anchorParent = ref subsystems[subsystemIndex];
            }

            var playerLoopSubsystem = new PlayerLoopSystem
            {
                type           = plan.PlayerLoopSubsystemType,
                updateDelegate = playerLoopSubsystemUpdateDelegate
            };
            var continuationSubsystem = new PlayerLoopSystem
            {
                type           = plan.ContinuationSubsystemType,
                updateDelegate = continuationSubsystemUpdateDelegate
            };

            // The pair may be left over from a previous initialization; it has to be removed before the anchor is located, because the removal shortens the list and shifts the anchor's index.
            var removePlayerLoopSubsystem   = RemoveSubsystem(ref anchorParent, plan.PlayerLoopSubsystemType);
            var removeContinuationSubsystem = RemoveSubsystem(ref anchorParent, plan.ContinuationSubsystemType);
            var insertPlayerLoopSubsystemAndContinuationSubsystem = InsertPlayerLoopSubsystemAndContinuationSubsystem(
                ref anchorParent,
                subsystemTypes[^1],
                plan.AfterPosition,
                playerLoopSubsystem,
                continuationSubsystem
            );
            return removePlayerLoopSubsystem || removeContinuationSubsystem ||
                   insertPlayerLoopSubsystemAndContinuationSubsystem;
        }

        private static bool InsertPlayerLoopSubsystemAndContinuationSubsystem(
            ref PlayerLoopSystem anchorParent,
            Type                 anchorSubsystemType,
            bool                 insertAfter,
            PlayerLoopSystem     playerLoopSubsystem,
            PlayerLoopSystem     continuationSubsystem)
        {
            ref var anchorSiblings = ref anchorParent.subSystemList;
            if (anchorSiblings is null)
            {
                Log.E(
                    $"The player loop plan is not applied: {TypeUtility.GetNicelyFormattedName(anchorSubsystemType)} cannot be anchored because {(anchorParent.type is not null ? TypeUtility.GetNicelyFormattedName(anchorParent.type) : PlayerLoopRootDescription)} has no sub-system list."
                );
                return false;
            }
            var anchorIndex = anchorSiblings.FindIndex(PlayerLoopSystemHasType, anchorSubsystemType);
            if (anchorIndex < 0)
            {
                Log.E(
                    $"The player loop plan is not applied: {TypeUtility.GetNicelyFormattedName(anchorSubsystemType)} was not found in the sub-system list of {(anchorParent.type is not null ? TypeUtility.GetNicelyFormattedName(anchorParent.type) : PlayerLoopRootDescription)}."
                );
                return false;
            }
            Insert(
                ref anchorSiblings,
                playerLoopSubsystem,
                continuationSubsystem,
                anchorIndex + Convert.ToInt32(insertAfter)
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

        private static bool RemoveSubsystem(ref PlayerLoopSystem category, Type subsystemType)
        {
            ref var subsystems = ref category.subSystemList;
            if (subsystems is null)
            {
                return false;
            }
            var subsystemIndex = subsystems.FindIndex(PlayerLoopSystemHasType, subsystemType);
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

        internal readonly struct Position
        {
            internal readonly Type[] SubsystemTypes;

            private Position(Type[] subsystemTypes)
            {
                SubsystemTypes = subsystemTypes;
            }

            internal static Position At(Type subsystemType)
            {
                return new Position(new[] { subsystemType });
            }

            internal static Position At(Type subsystemType1, Type subsystemType2)
            {
                return new Position(new[] { subsystemType1, subsystemType2 });
            }

            internal static Position At(Type subsystemType1, Type subsystemType2, Type subsystemType3)
            {
                return new Position(new[] { subsystemType1, subsystemType2, subsystemType3 });
            }

            internal static Position At(params Type[] subsystemTypes)
            {
                return new Position(subsystemTypes);
            }
        }

        internal readonly struct Plan
        {
            internal readonly Position Position;

            internal readonly bool AfterPosition;

            internal readonly Type PlayerLoopSubsystemType;

            internal readonly Type ContinuationSubsystemType;

            private Plan(
                Position position,
                bool     afterPosition,
                Type     playerLoopSubsystemType,
                Type     continuationSubsystemType)
            {
                Position                  = position;
                AfterPosition             = afterPosition;
                PlayerLoopSubsystemType   = playerLoopSubsystemType;
                ContinuationSubsystemType = continuationSubsystemType;
            }

            internal static Plan Before(Position position, Type playerLoopSubsystemType, Type continuationSubsystemType)
            {
                return new Plan(position, false, playerLoopSubsystemType, continuationSubsystemType);
            }

            internal static Plan After(Position position, Type playerLoopSubsystemType, Type continuationSubsystemType)
            {
                return new Plan(position, true, playerLoopSubsystemType, continuationSubsystemType);
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
