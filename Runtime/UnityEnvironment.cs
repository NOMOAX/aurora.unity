using System;
using System.Collections.Generic;
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
    /// Provides values and methods related to the Unity environment.
    /// </summary>
    public static class UnityEnvironment
    {
        /// <summary>
        /// Gets a value indicating whether the program is running; in the editor environment, indicates that play mode is active.
        /// </summary>
        /// <remarks>
        /// The value is set to <see langword="true"/> when the program starts, and to <see langword="false"/> when the program ends; in the editor environment, these correspond to entering and exiting play mode.
        /// <br/>
        /// When the program is ending, complex and time-consuming operations should be skipped so that it can end as quickly as possible.
        /// </remarks>
        public static bool IsPlaying { get; internal set; }

        internal static Transform AuroraContainer { get; set; }

        /// <summary>
        /// A <see cref="Transform"/> that is always inactive during play mode. Use it as the parent when instantiating objects so that their <see cref="MonoBehaviour"/><c>.OnEnable</c>s are not executed immediately.
        /// </summary>
        /// <remarks>
        /// This transform stays inactive for the whole play mode. Do not set it active. It is created at runtime when play mode starts and is destroyed when play mode ends or when the program exits.
        /// </remarks>
        public static Transform InactiveContainer { get; internal set; }

        internal static readonly Stack<IDisposable> Disposables = new();

        internal static CancellationTokenSource ExitTokenSource { get; set; }

        /// <summary>
        /// The Unity main thread ID.
        /// </summary>
        /// <remarks>Assigned during the <see cref="RuntimeInitializeLoadType.BeforeSceneLoad"/> phase.</remarks>
        public static int UnityMainThreadId { get; internal set; }

        /// <summary>
        /// The Unity synchronization context.
        /// </summary>
        /// <remarks>Assigned during the <see cref="RuntimeInitializeLoadType.BeforeSceneLoad"/> phase.</remarks>
        public static SynchronizationContext UnitySynchronizationContext { get; internal set; }

        /// <summary>
        /// The task scheduler that holds the <see cref="RuntimeInitializeLoadType.BeforeSceneLoad"/>.
        /// </summary>
        /// <remarks>Assigned during the <see cref="UnitySynchronizationContext"/> phase.</remarks>
        public static TaskScheduler UnitySynchronizationContextTaskScheduler { get; internal set; }

        /// <summary>
        /// Gets a cancellation token that is canceled when the program ends; in the editor environment, when exiting play mode.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The token is created when the program starts and canceled when the program ends.
        /// <br/>
        /// In the editor environment, the token is created when play mode starts and canceled when exiting play mode; if play mode is not active, an already-canceled token is returned.
        /// <br/>
        /// Pass this token to asynchronous or long-running operations so that they can be canceled when the program ends.
        /// </para>
        /// <para>
        /// Callbacks registered on this token should not throw; exceptions thrown in callbacks are logged when the program ends.
        /// </para>
        /// </remarks>
        public static CancellationToken ExitToken
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        /// Gets a value indicating whether the current thread is the Unity main thread.
        /// </summary>
        public static bool OnUnityMainThread
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => System.Environment.CurrentManagedThreadId == UnityMainThreadId;
        }

        /// <summary>
        /// Gets a value indicating whether the current environment is the editor environment with the pro skin.
        /// </summary>
        public static bool IsProSkin { get; internal set; }

        /// <summary>
        /// The value of the "Default" layer.
        /// </summary>
        public static int DefaultLayer { get; internal set; }

        /// <summary>
        /// The value of the "Ignore Raycast" layer.
        /// </summary>
        public static int IgnoreRaycastLayer { get; internal set; }

        /// <summary>
        /// The value of the "UI" layer.
        /// </summary>
        public static int UILayer { get; internal set; }

        /// <summary>
        /// The string clipboard.
        /// </summary>
        public static string Clipboard
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GUIUtility.systemCopyBuffer;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => GUIUtility.systemCopyBuffer = value;
        }

        /// <summary>
        /// The screen size.
        /// </summary>
        public static Vector2Int ScreenSize
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new(Screen.width, Screen.height);
        }

        /// <summary>
        /// The screen aspect ratio.
        /// </summary>
        public static float ScreenAspectRatio
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (float)Screen.width / Screen.height;
        }

        /// <summary>
        /// Quits this program.
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
        /// Quits this program.
        /// </summary>
        /// <param name="exitCode">The exit code.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void QuitApplication(int exitCode)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit(exitCode);
#endif
        }

        /// <summary>
        /// Adds an instance that is released when the program ends.
        /// </summary>
        /// <param name="disposable">The instance to release when the program ends.</param>
        /// <exception cref="ArgumentNullException"><paramref name="disposable"/> is <see langword="null"/>.</exception>
        /// <exception cref="InvalidOperationException">The program is not running (<see cref="IsPlaying"/> is <see langword="false"/>).</exception>
        /// <remarks>Multiple <see cref="IDisposable"/> instances added via <see cref="DisposeOnApplicationQuit"/> are released in last-in-first-out order when the program ends.</remarks>
        public static void DisposeOnApplicationQuit(IDisposable disposable)
        {
            if (disposable == null)
            {
                throw new ArgumentNullException();
            }
            if (!IsPlaying)
            {
                throw new InvalidOperationException();
            }
            Disposables.Push(disposable);
        }
    }
}
