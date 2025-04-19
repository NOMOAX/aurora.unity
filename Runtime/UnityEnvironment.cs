using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Aurora.Unity
{
    /// <summary>
    /// 运行环境。
    /// </summary>
    public static class UnityEnvironment
    {
        /// <summary>
        /// 程序是否正在运行。
        /// </summary>
        /// <remarks>当这个值为 <see langword="false"/> 时，应跳过复杂与耗时的操作，让程序尽快结束。</remarks>
        public static bool IsPlaying { get; internal set; }

        /// <summary>
        /// Unity 主线程 ID。
        /// </summary>
        /// <remarks>在 <see cref="RuntimeInitializeLoadType.BeforeSceneLoad"/> 阶段被赋值。</remarks>
        public static int UnityMainThreadId { get; internal set; }

        /// <summary>
        /// Unity 同步上下文。
        /// </summary>
        /// <remarks>在 <see cref="RuntimeInitializeLoadType.BeforeSceneLoad"/> 阶段被赋值。</remarks>
        public static SynchronizationContext UnitySynchronizationContext { get; internal set; }

        /// <summary>
        /// 持有 <see cref="RuntimeInitializeLoadType.BeforeSceneLoad"/> 的任务调度器。
        /// </summary>
        /// <remarks>在 <see cref="UnitySynchronizationContext"/> 阶段被赋值。</remarks>
        public static TaskScheduler UnitySynchronizationContextTaskScheduler { get; internal set; }

        /// <summary>
        /// 获取一个值，这个值指示当前线程是否是 Unity 主线程。
        /// </summary>
        public static bool OnUnityMainThread
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => System.Environment.CurrentManagedThreadId == UnityMainThreadId;
        }

        /// <summary>
        /// 获取一个值，这个值指示当前是否处于编辑器环境，且使用了专业版皮肤。
        /// </summary>
        public static bool IsProSkin { get; internal set; }

        /// <summary>
        /// 字符串剪贴板。
        /// </summary>
        public static string Clipboard
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GUIUtility.systemCopyBuffer;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => GUIUtility.systemCopyBuffer = value;
        }

        /// <summary>
        /// 屏幕尺寸。
        /// </summary>
        public static Vector2Int ScreenSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new Vector2Int(Screen.width, Screen.height);
        }

        /// <summary>
        /// 屏幕宽高比。
        /// </summary>
        public static float ScreenAspectRatio
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (float) Screen.width / Screen.height;
        }

        /// <summary>
        /// 结束此程序。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QuitApplication()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// 结束此程序。
        /// </summary>
        /// <param name="exitCode">退出代码。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QuitApplication(int exitCode)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit(exitCode);
#endif
        }
    }
}
