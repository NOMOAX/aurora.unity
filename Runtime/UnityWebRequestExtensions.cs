using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides extension methods for the <see cref="UnityWebRequest"/> class.
    /// </summary>
    public static class UnityWebRequestExtensions
    {
        /// <summary>
        /// Asynchronously sends a web request using the current <see cref="UnityWebRequest"/>.
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task object of the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> has been disposed, or is disposed during this asynchronous operation.</exception>
        /// <exception cref="UnityException">This method is called from off the Unity main thread.</exception>
        /// <exception cref="ArgumentException"><paramref name="unityWebRequest"/> has already called <see cref="UnityWebRequest.SendWebRequest"/>.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled, or a cancellation request is issued during this asynchronous operation.</exception>
        /// <exception cref="UnityWebRequestException">The web request encountered an error (but not because the HTTP status code does not indicate a success status).</exception>
        public static Task SendWebRequestAsync(
            this UnityWebRequest unityWebRequest,
            CancellationToken    cancellationToken = default)
        {
            return UnityWebRequestUtility.SendWebRequestAsync(unityWebRequest, cancellationToken);
        }

        /// <summary>
        /// Asynchronously sends a web request using the current <see cref="UnityWebRequest"/>, then returns the received data (as a string).
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task object of the asynchronous operation, whose <see cref="Task{TResult}.Result"/> is a string.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> has been disposed, or is disposed during this asynchronous operation.</exception>
        /// <exception cref="ArgumentException"><paramref name="unityWebRequest"/> has already called <see cref="UnityWebRequest.SendWebRequest"/>.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled, or a cancellation request is issued during this asynchronous operation.</exception>
        /// <exception cref="UnityWebRequestException">The web request encountered an error.</exception>
        public static Task<string> GetStringAsync(
            this UnityWebRequest unityWebRequest,
            CancellationToken    cancellationToken = default)
        {
            return UnityWebRequestUtility.GetStringAsync(unityWebRequest, cancellationToken);
        }

        /// <summary>
        /// Asynchronously sends a web request using the current <see cref="UnityWebRequest"/>, then returns the received data (as a byte array).
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task object of the asynchronous operation, whose <see cref="Task{TResult}.Result"/> is a byte array.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> has been disposed, or is disposed during this asynchronous operation.</exception>
        /// <exception cref="ArgumentException"><paramref name="unityWebRequest"/> has already called <see cref="UnityWebRequest.SendWebRequest"/>.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled, or a cancellation request is issued during this asynchronous operation.</exception>
        /// <exception cref="UnityWebRequestException">The web request encountered an error.</exception>
        public static Task<byte[]> GetByteArrayAsync(
            this UnityWebRequest unityWebRequest,
            CancellationToken    cancellationToken = default)
        {
            return UnityWebRequestUtility.GetByteArrayAsync(unityWebRequest, cancellationToken);
        }

        /// <summary>
        /// Asynchronously sends a web request using the current <see cref="UnityWebRequest"/>, then returns the received data (as a stream).
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task object of the asynchronous operation, whose <see cref="Task{TResult}.Result"/> is a stream.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> has been disposed, or is disposed during this asynchronous operation.</exception>
        /// <exception cref="ArgumentException"><paramref name="unityWebRequest"/> has already called <see cref="UnityWebRequest.SendWebRequest"/>.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled, or a cancellation request is issued during this asynchronous operation.</exception>
        /// <exception cref="UnityWebRequestException">The web request encountered an error.</exception>
        public static Task<Stream> GetStreamAsync(
            this UnityWebRequest unityWebRequest,
            CancellationToken    cancellationToken = default)
        {
            return UnityWebRequestUtility.GetStreamAsync(unityWebRequest, cancellationToken);
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the current <see cref="UnityWebRequest"/>'s HTTP status code does not indicate a success status.
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        /// <exception cref="UnityWebRequestException">The HTTP status code of <paramref name="unityWebRequest"/> does not indicate a success status.</exception>
        public static void ThrowIfNotSuccessStatusCode(this UnityWebRequest unityWebRequest)
        {
            UnityWebRequestUtility.ThrowIfNotSuccessStatusCode(unityWebRequest);
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="UnityWebRequest"/> has timed out.
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request.</param>
        /// <returns><see langword="true"/> if <paramref name="unityWebRequest"/> has timed out; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        public static bool IsTimeout(this UnityWebRequest unityWebRequest)
        {
            return UnityWebRequestUtility.IsTimeout(unityWebRequest);
        }

        /// <summary>
        /// Gets a value indicating whether the current <see cref="UnityWebRequest"/>'s HTTP status code indicates a success status.
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request.</param>
        /// <returns><see langword="true"/> if the HTTP status code of <paramref name="unityWebRequest"/> indicates a success status; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        public static bool IsSuccessStatusCode(this UnityWebRequest unityWebRequest)
        {
            return UnityWebRequestUtility.IsSuccessStatusCode(unityWebRequest);
        }
    }
}
