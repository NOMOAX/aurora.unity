using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Threading;
using Aurora.Unity.PlayerLoop;
using Aurora.Unity.Threading;
using Aurora.Unity.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Aurora.Unity
{
    /// <summary>
    /// 封装常用功能。
    /// </summary>
    public static class UnityUtility
    {
        private static readonly Regex OptimizeNameRegex = new(
            @"(?:\(Clone\)| \([1-9][0-9]*\))+\Z",
            RegexOptions.Compiled
        );

        private static readonly TimerTriggerCallback CancelCancellationTokenSource = (timer, state) =>
        {
            timer.Dispose();
            var cancellationTokenSource = (CancellationTokenSource) state;
            if (CancellationTokenSourceUtility.IsDisposed(cancellationTokenSource))
            {
                return;
            }
            try
            {
                cancellationTokenSource.Cancel();
            }
            catch (ObjectDisposedException)
            {
                if (!CancellationTokenSourceUtility.IsDisposed(cancellationTokenSource))
                {
                    throw;
                }
            }
        };

        /// <summary>
        /// 连击时，两次点击的最大间隔时间。
        /// </summary>
        public const float ClickDelayTime = 0.3f;

        /// <summary>
        /// 由于超时的原因而完成时，<see cref="UnityWebRequest"/><c>.</c><see cref="UnityWebRequest.error"/> 的值。
        /// </summary>
        public const string UnityWebRequestTimeoutString = "Request timeout";

        /// <summary>
        /// 每个网格可包含的最大顶点数量。
        /// </summary>
        public const int VertexCountPerMeshMaxValue = 65000 - 1;

        /// <summary>
        /// 优化 Unity 对象的名称。
        /// </summary>
        /// <param name="object">Unity 对象。</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="object"/> 为 <see langword="null"/>。</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OptimizeName(Object @object)
        {
            if (@object is null)
            {
                throw new ArgumentNullException(nameof(@object));
            }
            if (!@object)
            {
                return;
            }
            var name = @object.name;
            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            var newName = OptimizeNameRegex.Replace(name, string.Empty);
            if (name == newName)
            {
                return;
            }
            @object.name = newName;
#if UNITY_EDITOR
            EditorUtility.SetDirty(@object);
#endif
        }

        /// <summary>
        /// 如果指定的游戏物体是当前事件系统的当前选定的游戏物体，就把事件系统的当前选定的游戏物体设为 <see langword="null"/>。
        /// </summary>
        /// <param name="gameObject">游戏物体。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeselectEventSystemCurrentSelectedGameObject(GameObject gameObject)
        {
            InternalDeselectEventSystemCurrentSelectedGameObject(gameObject, EventSystem.current);
        }

        /// <summary>
        /// 如果指定的游戏物体是指定的事件系统的当前选定的游戏物体，就把事件系统的当前选定的游戏物体设为 <see langword="null"/>。
        /// </summary>
        /// <param name="gameObject">游戏物体。</param>
        /// <param name="eventSystem">事件系统。</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeselectEventSystemCurrentSelectedGameObject(GameObject gameObject, EventSystem eventSystem)
        {
            InternalDeselectEventSystemCurrentSelectedGameObject(gameObject, eventSystem);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void InternalDeselectEventSystemCurrentSelectedGameObject(
            Object      gameObject,
            EventSystem eventSystem)
        {
            if (!gameObject || !eventSystem)
            {
                return;
            }
            if (eventSystem.currentSelectedGameObject == gameObject)
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }

        /// <summary>
        /// 安排取消请求。
        /// </summary>
        /// <param name="cancellationTokenSource">取消令牌源。</param>
        /// <param name="delay">发出取消请求前的等待时间。</param>
        /// <returns>一个可释放对象，释放该对象以终止此方法。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cancellationTokenSource"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delay"/> 不为 <see cref="Timeout.InfiniteTimeSpan"/>，并且它的毫秒数不在 [0, 4294967294] 范围内。</exception>
        public static IDisposable CancelAfter(CancellationTokenSource cancellationTokenSource, TimeSpan delay)
        {
            if (cancellationTokenSource == null)
            {
                throw new ArgumentNullException(nameof(cancellationTokenSource));
            }
            if (delay != Timeout.InfiniteTimeSpan &&
                (delay < TimeSpan.Zero || delay > Constant.TimeSpan.TimerMaxSupportedTimeout))
            {
                throw new ArgumentOutOfRangeException(nameof(delay));
            }
            if (delay == Timeout.InfiniteTimeSpan)
            {
                return NullDisposable.Instance;
            }
            if (delay == TimeSpan.Zero)
            {
                cancellationTokenSource.Cancel();
                return NullDisposable.Instance;
            }
            var timer = new StopwatchPlayerLoopTimer(
                CancelCancellationTokenSource,
                cancellationTokenSource,
                delay,
                Timeout.InfiniteTimeSpan,
                PlayerLoopPhase.Updating
            );
            return timer;
        }

        /// <summary>
        /// 捕获屏幕截图，但不会等待该过程完成。
        /// </summary>
        /// <param name="path">截图文件保存的路径。</param>
        /// <remarks>截图为 PNG 文件。</remarks>
        public static void BeginCaptureScreenshot(string path)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                // 检查文件是否被占用
                using (new FileStream(fullPath, FileMode.Open, FileAccess.Write, FileShare.Read))
                {
                }
            }
            ScreenCapture.CaptureScreenshot(path);
        }

        /// <summary>
        /// 异步捕获屏幕截图。
        /// </summary>
        /// <param name="path">截图文件保存的路径。</param>
        /// <returns>异步操作的任务对象。</returns>
        /// <remarks>截图为 PNG 文件。</remarks>
        public static Task CaptureScreenshotAsync(string path)
        {
            return UnityTasks.WhenScreenshotCaptured(path);
        }

        /// <summary>
        /// 异步捕获屏幕截图。
        /// </summary>
        /// <param name="path">截图文件保存的路径。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>异步操作的任务对象。</returns>
        /// <remarks>截图为 PNG 文件。</remarks>
        public static Task CaptureScreenshotAsync(string path, CancellationToken cancellationToken)
        {
            return UnityTasks.WhenScreenshotCaptured(path, cancellationToken);
        }
    }
}
