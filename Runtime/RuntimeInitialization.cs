using System.Threading;
using System.Threading.Tasks;
using Aurora.Diagnostics;
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
            Debug.Log($"开始执行 {nameof(RuntimeInitialization)}.{nameof(Initialize)}");
            try
            {
                InternalInitialize();
            }
            finally
            {
                Debug.Log($"结束执行 {nameof(RuntimeInitialization)}.{nameof(Initialize)}");
            }
        }

        private static void InternalInitialize()
        {
#if UNITY_WEBGL
            SetIsSingleThreadEnvironment(true);
#else
            SetIsSingleThreadEnvironment(false);
#endif
            SetIsPlaying(true);
#if !UNITY_EDITOR
            SetUnityConsoleLogger();
            SetUnityMainThreadId();
            SetUnitySynchronizationContext();
            SetUnitySynchronizationContextTaskScheduler();
            SetIsProSkin(false);
            PlayerLoopUtilityInitialize();
            RegisterPrefabLessViewHandler();
#endif
            AlwaysActiveAndEnabled.EnsureInstanceExists();
        }

        internal static void SetIsSingleThreadEnvironment(bool value)
        {
            Environment.IsSingleThreadEnvironment = value;
            Debug.Log($"{nameof(Environment)}.{nameof(Environment.IsSingleThreadEnvironment)} = {value};");
        }

        internal static void SetIsPlaying(bool value)
        {
            UnityEnvironment.IsPlaying = value;
            Debug.Log($"{nameof(UnityEnvironment)}.{nameof(UnityEnvironment.IsPlaying)} = {value};");
        }

        internal static void SetUnityConsoleLogger()
        {
            Log.Logger = UnityConsoleLogger.Instance;
            Debug.Log(
                $"{nameof(Log)}.{nameof(Log.Logger)} = {nameof(UnityConsoleLogger)}.{nameof(UnityConsoleLogger.Instance)};"
            );
        }

        internal static void SetUnityMainThreadId()
        {
            UnityEnvironment.UnityMainThreadId = System.Environment.CurrentManagedThreadId;
            Debug.Log(
                $"{nameof(UnityEnvironment)}.{nameof(UnityEnvironment.UnityMainThreadId)} = {nameof(System.Environment)}.{nameof(System.Environment.CurrentManagedThreadId)};"
            );
        }

        internal static void SetUnitySynchronizationContext()
        {
            UnityEnvironment.UnitySynchronizationContext = SynchronizationContext.Current;
            Debug.Log(
                $"{nameof(UnityEnvironment)}.{nameof(UnityEnvironment.UnitySynchronizationContext)} = {nameof(SynchronizationContext)}.{nameof(SynchronizationContext.Current)};"
            );
        }

        internal static void SetUnitySynchronizationContextTaskScheduler()
        {
            UnityEnvironment.UnitySynchronizationContextTaskScheduler =
                TaskScheduler.FromCurrentSynchronizationContext();
            Debug.Log(
                $"{nameof(UnityEnvironment)}.{nameof(UnityEnvironment.UnitySynchronizationContextTaskScheduler)} = {nameof(TaskScheduler)}.{nameof(TaskScheduler.FromCurrentSynchronizationContext)}();"
            );
        }

        internal static void SetIsProSkin(bool value)
        {
            UnityEnvironment.IsProSkin = value;
            Debug.Log($"{nameof(UnityEnvironment)}.{nameof(UnityEnvironment.IsProSkin)} = {value};");
        }

        internal static void PlayerLoopUtilityInitialize()
        {
            PlayerLoopUtility.Initialize();
            Debug.Log($"{nameof(PlayerLoopUtility)}.{nameof(PlayerLoopUtility.Initialize)}();");
        }

        internal static void RegisterPrefabLessViewHandler()
        {
            ViewHandler.Register(PrefabLessViewHandler.Instance);
            Debug.Log(
                $"{nameof(ViewHandler)}.{nameof(ViewHandler.Register)}({nameof(PrefabLessViewHandler)}.{nameof(PrefabLessViewHandler.Instance)});"
            );
        }
    }
}
