using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Diagnostics;
using Aurora.Pooling;
using Aurora.Unity.PlayerLoop;
using Aurora.Unity.UI.ViewSystem;
using UnityEngine;

namespace Aurora.Unity
{
    internal static class RuntimeInitialization
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            var stringBuilder = PredefinedPools.StringBuilder.Get();
            try
            {
                stringBuilder.AppendLine(
                    $"{nameof(Aurora)}.{nameof(Unity)}.{nameof(RuntimeInitialization)}.{nameof(Initialize)}();"
                );
                InternalInitialize(stringBuilder);
                Debug.Log(stringBuilder.ToString());
            }
            finally
            {
                PredefinedPools.StringBuilder.Return(stringBuilder);
            }
        }

        private static void InternalInitialize(StringBuilder stringBuilder)
        {
#if UNITY_WEBGL
            SetIsSingleThreadEnvironment(true, stringBuilder);
#else
            SetIsSingleThreadEnvironment(false, stringBuilder);
#endif
            EnterPlayMode(stringBuilder);
#if !UNITY_EDITOR
            SetUnityConsoleLogger(stringBuilder);
            SetUnityMainThreadId(stringBuilder);
            SetUnitySynchronizationContext(stringBuilder);
            SetUnitySynchronizationContextTaskScheduler(stringBuilder);
            SetIsProSkin(false, stringBuilder);
            PlayerLoopUtilityInitialize(stringBuilder);
            RegisterPrefabLessViewHandler(stringBuilder);
            ApplicationQuitListener.EnsureInstanceExists();
#endif
        }

        internal static void SetIsSingleThreadEnvironment(bool value, StringBuilder stringBuilder)
        {
            Environment.IsSingleThreadEnvironment = value;

            var message = $"{nameof(Environment)}.{nameof(Environment.IsSingleThreadEnvironment)} = {value};";
            if (stringBuilder != null)
            {
                stringBuilder.AppendLine($"    {message}");
            }
            else
            {
                Debug.Log(message);
            }
        }

        /// <remarks>除非程序结束，否则应确保在调用 <see cref="EnterPlayMode"/> 之后无论隔多久总会调用一次 <see cref="ExitPlayMode"/>。</remarks>
        private static void EnterPlayMode(StringBuilder stringBuilder)
        {
            UnityEnvironment.IsPlaying       = true;
            UnityEnvironment.ExitTokenSource = new CancellationTokenSource();

            var message = $"{nameof(UnityEnvironment)}.{nameof(UnityEnvironment.IsPlaying)} = {true};";
            if (stringBuilder != null)
            {
                stringBuilder.AppendLine($"    {message}");
            }
            else
            {
                Debug.Log(message);
            }
        }

        /// <remarks>除非程序结束，否则应确保在调用 <see cref="EnterPlayMode"/> 之后无论隔多久总会调用一次 <see cref="ExitPlayMode"/>。</remarks>
        internal static void ExitPlayMode(StringBuilder stringBuilder)
        {
            UnityEnvironment.IsPlaying = false;
            try
            {
                UnityEnvironment.ExitTokenSource.Cancel();
            }
            catch (Exception e)
            {
                Log.E(e);
                Log.W($"不应在 {nameof(UnityEnvironment)}.{nameof(UnityEnvironment.ExitToken)} 令牌的回调函数中抛出异常。");
            }
            finally
            {
                UnityEnvironment.ExitTokenSource.Dispose();
                UnityEnvironment.ExitTokenSource = null;
            }

            var message = $"{nameof(UnityEnvironment)}.{nameof(UnityEnvironment.IsPlaying)} = {false};";
            if (stringBuilder != null)
            {
                stringBuilder.AppendLine($"    {message}");
            }
            else
            {
                Debug.Log(message);
            }
        }

        internal static void SetUnityConsoleLogger(StringBuilder stringBuilder)
        {
            Log.Logger = UnityConsoleLogger.Instance;

            var message =
                $"{nameof(Log)}.{nameof(Log.Logger)} = {nameof(UnityConsoleLogger)}.{nameof(UnityConsoleLogger.Instance)};";
            if (stringBuilder != null)
            {
                stringBuilder.AppendLine($"    {message}");
            }
            else
            {
                Debug.Log(message);
            }
        }

        internal static void SetUnityMainThreadId(StringBuilder stringBuilder)
        {
            var unityMainThreadId = System.Environment.CurrentManagedThreadId;
            UnityEnvironment.UnityMainThreadId = unityMainThreadId;

            var message =
                $"{nameof(UnityEnvironment)}.{nameof(UnityEnvironment.UnityMainThreadId)} = {nameof(System.Environment)}.{nameof(System.Environment.CurrentManagedThreadId)}; // {unityMainThreadId}";
            if (stringBuilder != null)
            {
                stringBuilder.AppendLine($"    {message}");
            }
            else
            {
                Debug.Log(message);
            }
        }

        internal static void SetUnitySynchronizationContext(StringBuilder stringBuilder)
        {
            UnityEnvironment.UnitySynchronizationContext = SynchronizationContext.Current;

            var message =
                $"{nameof(UnityEnvironment)}.{nameof(UnityEnvironment.UnitySynchronizationContext)} = {nameof(SynchronizationContext)}.{nameof(SynchronizationContext.Current)};";
            if (stringBuilder != null)
            {
                stringBuilder.AppendLine($"    {message}");
            }
            else
            {
                Debug.Log(message);
            }
        }

        internal static void SetUnitySynchronizationContextTaskScheduler(StringBuilder stringBuilder)
        {
            UnityEnvironment.UnitySynchronizationContextTaskScheduler =
                TaskScheduler.FromCurrentSynchronizationContext();

            var message =
                $"{nameof(UnityEnvironment)}.{nameof(UnityEnvironment.UnitySynchronizationContextTaskScheduler)} = {nameof(TaskScheduler)}.{nameof(TaskScheduler.FromCurrentSynchronizationContext)}();";
            if (stringBuilder != null)
            {
                stringBuilder.AppendLine($"    {message}");
            }
            else
            {
                Debug.Log(message);
            }
        }

        internal static void SetIsProSkin(bool value, StringBuilder stringBuilder)
        {
            UnityEnvironment.IsProSkin = value;

            var message = $"{nameof(UnityEnvironment)}.{nameof(UnityEnvironment.IsProSkin)} = {value};";
            if (stringBuilder != null)
            {
                stringBuilder.AppendLine($"    {message}");
            }
            else
            {
                Debug.Log(message);
            }
        }

        internal static void PlayerLoopUtilityInitialize(StringBuilder stringBuilder)
        {
            PlayerLoopUtility.Initialize();

            var message = $"{nameof(PlayerLoopUtility)}.{nameof(PlayerLoopUtility.Initialize)}();";
            if (stringBuilder != null)
            {
                stringBuilder.AppendLine($"    {message}");
            }
            else
            {
                Debug.Log(message);
            }
        }

        internal static void RegisterPrefabLessViewHandler(StringBuilder stringBuilder)
        {
            ViewHandler.Register(PrefabLessViewHandler.Instance);

            var message =
                $"{nameof(ViewHandler)}.{nameof(ViewHandler.Register)}({nameof(PrefabLessViewHandler)}.{nameof(PrefabLessViewHandler.Instance)});";
            if (stringBuilder != null)
            {
                stringBuilder.AppendLine($"    {message}");
            }
            else
            {
                Debug.Log(message);
            }
        }
    }
}
