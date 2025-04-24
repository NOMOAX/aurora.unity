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

        internal static CancellationTokenSource ExitTokenSource { get; set; }

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
        /// 在编辑器环境下，获取一个在进入播放模式时初始化、退出播放模式时发出取消通知的取消令牌，在其他情况下，将直接返回一个已取消的令牌；
        /// <br/>
        /// 在非编辑器环境下，获取一个在程序开始时初始化、程序结束时发出取消通知的取消令牌。
        /// </summary>
        public static CancellationToken ExitToken
        {
            get
            {
#if UNITY_EDITOR
                if (ExitTokenSource == null)
                {
                    return new CancellationToken(true);
                }
#endif
                return ExitTokenSource.Token;
            }
        }

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
