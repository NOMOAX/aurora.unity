using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Diagnostics;
using Aurora.Pooling;
using Aurora.Unity.PlayerLoop;
using Aurora.Unity.UI.ViewSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Aurora.Unity
{
    internal static class RuntimeInitialization
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
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
            {
                var stringBuilder = PredefinedPools.StringBuilder.Get();
                try
                {
                    var regex                          = new Regex(" {2,}");
                    var operatingSystemNameWithVersion = regex.Replace(SystemInfo.operatingSystem, " ").Trim();
                    var systemMemorySize               = SystemInfo.systemMemorySize;
                    var processorName                  = regex.Replace(SystemInfo.processorType, " ").Trim();
                    var processorCount                 = SystemInfo.processorCount;
                    var graphicsDeviceName             = regex.Replace(SystemInfo.graphicsDeviceName, " ").Trim();
                    var graphicsMemorySize             = SystemInfo.graphicsMemorySize;

                    stringBuilder.AppendLine($"{operatingSystemNameWithVersion} ({systemMemorySize} MB)");
                    stringBuilder.AppendLine($"{processorName} ({processorCount} Processors)");
                    stringBuilder.AppendLine($"{graphicsDeviceName} ({graphicsMemorySize} MB)");
                    Debug.Log(stringBuilder.ToString());
                }
                finally
                {
                    PredefinedPools.StringBuilder.Return(stringBuilder);
                }
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
            SetDefaultLayer(LayerMask.NameToLayer("Default"), stringBuilder);
            SetIgnoreRaycastLayer(LayerMask.NameToLayer("Ignore Raycast"), stringBuilder);
            SetUILayer(LayerMask.NameToLayer("UI"), stringBuilder);
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

        /// <remarks>Unless the program ends, <see cref="ExitPlayMode"/> must be called once no matter how much time passes after <see cref="EnterPlayMode"/> is called.</remarks>
        private static void EnterPlayMode(StringBuilder stringBuilder)
        {
            UnityEnvironment.IsPlaying         = true;
            UnityEnvironment.ExitTokenSource   = new CancellationTokenSource();
            UnityEnvironment.InactiveContainer = CreateInactiveContainer();

            var message = $"{nameof(UnityEnvironment)}.{nameof(UnityEnvironment.IsPlaying)} = {true};";
            if (stringBuilder != null)
            {
                stringBuilder.AppendLine($"    {message}");
            }
            else
            {
                Debug.Log(message);
            }

            static Transform CreateInactiveContainer()
            {
                const string name       = nameof(UnityEnvironment) + "." + nameof(UnityEnvironment.InactiveContainer);
                var          gameObject = new GameObject(name);
                gameObject.SetActive(false);
                gameObject.hideFlags |= HideFlags.HideAndDontSave;
                return gameObject.transform;
            }
        }

        /// <remarks>Unless the program ends, <see cref="ExitPlayMode"/> must be called once no matter how much time passes after <see cref="EnterPlayMode"/> is called.</remarks>
        internal static void ExitPlayMode(StringBuilder stringBuilder)
        {
            UnityEnvironment.IsPlaying = false;
            while (UnityEnvironment.Disposables.Count != 0)
            {
                var disposable = UnityEnvironment.Disposables.Pop();
                try
                {
                    disposable.Dispose();
                }
                catch (Exception e)
                {
                    Log.E(e);
                }
            }
            try
            {
                UnityEnvironment.ExitTokenSource.Cancel();
            }
            catch (Exception e)
            {
                Log.E(e);
                Log.W(
                    $"An exception should not be thrown in the callback of the {nameof(UnityEnvironment)}.{nameof(UnityEnvironment.ExitToken)} token."
                );
            }
            finally
            {
                UnityEnvironment.ExitTokenSource.Dispose();
                UnityEnvironment.ExitTokenSource = null;
            }
            Object.DestroyImmediate(UnityEnvironment.InactiveContainer.gameObject);
            UnityEnvironment.InactiveContainer = null;

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

        internal static void SetDefaultLayer(int value, StringBuilder stringBuilder)
        {
            UnityEnvironment.DefaultLayer = value;

            var message = $"{nameof(UnityEnvironment)}.{nameof(UnityEnvironment.DefaultLayer)} = {value};";
            if (stringBuilder != null)
            {
                stringBuilder.AppendLine($"    {message}");
            }
            else
            {
                Debug.Log(message);
            }
        }

        internal static void SetIgnoreRaycastLayer(int value, StringBuilder stringBuilder)
        {
            UnityEnvironment.IgnoreRaycastLayer = value;

            var message = $"{nameof(UnityEnvironment)}.{nameof(UnityEnvironment.IgnoreRaycastLayer)} = {value};";
            if (stringBuilder != null)
            {
                stringBuilder.AppendLine($"    {message}");
            }
            else
            {
                Debug.Log(message);
            }
        }

        internal static void SetUILayer(int value, StringBuilder stringBuilder)
        {
            UnityEnvironment.UILayer = value;

            var message = $"{nameof(UnityEnvironment)}.{nameof(UnityEnvironment.UILayer)} = {value};";
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
