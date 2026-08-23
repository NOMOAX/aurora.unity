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
    /// Wraps common functionality.
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
            var cancellationTokenSource = (CancellationTokenSource)state;
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
        /// The maximum interval between two clicks in a combo.
        /// </summary>
        public const float ClickDelayTime = 0.3f;

        /// <summary>
        /// The value of <see cref="UnityWebRequest"/><c>.</c><see cref="UnityWebRequest.error"/> when it completes due to a timeout.
        /// </summary>
        public const string UnityWebRequestTimeoutString = "Request timeout";

        /// <summary>
        /// The maximum number of vertices that can be contained in each mesh.
        /// </summary>
        public const int VertexCountPerMeshMaxValue = 65000 - 1;

        /// <summary>
        /// Optimizes the name of a Unity object.
        /// </summary>
        /// <param name="object">The Unity object.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="object"/> is <see langword="null"/>.</exception>
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
        /// If the specified game object is the current selected game object of the current event system, sets the current selected game object of the event system to <see langword="null"/>.
        /// </summary>
        /// <param name="gameObject">The game object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DeselectEventSystemCurrentSelectedGameObject(GameObject gameObject)
        {
            InternalDeselectEventSystemCurrentSelectedGameObject(gameObject, EventSystem.current);
        }

        /// <summary>
        /// If the specified game object is the current selected game object of the specified event system, sets the current selected game object of the event system to <see langword="null"/>.
        /// </summary>
        /// <param name="gameObject">The game object.</param>
        /// <param name="eventSystem">The event system.</param>
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
        /// Schedules a cancellation request.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="delay">The wait time before the cancellation request is issued.</param>
        /// <returns>A disposable object; dispose it to terminate this method.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cancellationTokenSource"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="delay"/> is not <see cref="Timeout.InfiniteTimeSpan"/>, and its milliseconds are not in the [0, 4294967294] range.</exception>
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
        /// Captures a screenshot, but does not wait for the process to finish.
        /// </summary>
        /// <param name="path">The path where the screenshot file is saved.</param>
        /// <remarks>The screenshot is a PNG file.</remarks>
        public static void BeginCaptureScreenshot(string path)
        {
            var fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                // Check whether the file is locked
                using (new FileStream(fullPath, FileMode.Open, FileAccess.Write, FileShare.Read))
                {
                }
            }
            ScreenCapture.CaptureScreenshot(path);
        }

        /// <summary>
        /// Asynchronously captures a screenshot.
        /// </summary>
        /// <param name="path">The path where the screenshot file is saved.</param>
        /// <returns>The task object of the asynchronous operation.</returns>
        /// <remarks>The screenshot is a PNG file.</remarks>
        public static Task CaptureScreenshotAsync(string path)
        {
            return UnityTasks.WhenScreenshotCaptured(path);
        }

        /// <summary>
        /// Asynchronously captures a screenshot.
        /// </summary>
        /// <param name="path">The path where the screenshot file is saved.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task object of the asynchronous operation.</returns>
        /// <remarks>The screenshot is a PNG file.</remarks>
        public static Task CaptureScreenshotAsync(string path, CancellationToken cancellationToken)
        {
            return UnityTasks.WhenScreenshotCaptured(path, cancellationToken);
        }
    }
}
