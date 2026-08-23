using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Unity.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

namespace Aurora.Unity
{
    /// <summary>
    /// Provides utility methods for the <see cref="UnityWebRequest"/> class.
    /// </summary>
    public static class UnityWebRequestUtility
    {
        private static readonly FieldInfo PtrFieldInfo = typeof(UnityWebRequest).GetField(
            "m_Ptr",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        private const string UnmodifiableMessage = "The " + nameof(UnityWebRequest) + " has already called " +
                                                   nameof(UnityWebRequest.SendWebRequest) + ", use " +
                                                   nameof(AsyncOperationAwaitable) +
                                                   " instead to await it to complete.";

        private const string InProgressMessage = "The request is in progress.";

        private const string ConnectionErrorMessage = "Failed to communicate with the server.";

        private const string DataProcessingErrorMessage =
            "The request succeeded in communicating with the server, but encountered an error when processing the received data.";

        private const string NotSuccessStatusCodeMessage =
            "The HTTP status code ({0}) returned from the server is not a success status code.";

        /// <summary>
        /// Asynchronously sends a web request using the specified <see cref="UnityWebRequest"/>.
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> has been disposed, or is disposed during this asynchronous operation.</exception>
        /// <exception cref="UnityException">This method is called from off the Unity main thread.</exception>
        /// <exception cref="ArgumentException"><paramref name="unityWebRequest"/> has already called <see cref="UnityWebRequest.SendWebRequest"/>.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled, or a cancellation request is issued during this asynchronous operation.</exception>
        /// <exception cref="UnityWebRequestException">The web request encountered an error (but not because the HTTP status code does not indicate a success status).</exception>
        public static async Task SendWebRequestAsync(
            UnityWebRequest   unityWebRequest,
            CancellationToken cancellationToken = default)
        {
            if (unityWebRequest is null)
            {
                throw new ArgumentNullException(nameof(unityWebRequest));
            }
            InternalThrowIfDisposed(unityWebRequest);
            if (!unityWebRequest.isModifiable)
            {
                throw new ArgumentException(UnmodifiableMessage, nameof(unityWebRequest));
            }
            var asyncOperation = unityWebRequest.SendWebRequest();
            await new AsyncOperationAwaitable(asyncOperation, cancellationToken);
            InternalThrowIfDisposed(unityWebRequest);
            InternalThrowIfFaulted(unityWebRequest);
        }

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if the specified <see cref="UnityWebRequest"/> has been disposed.
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> has been disposed.</exception>
        public static void ThrowIfDisposed(UnityWebRequest unityWebRequest)
        {
            if (unityWebRequest is null)
            {
                throw new ArgumentNullException(nameof(unityWebRequest));
            }
            InternalThrowIfDisposed(unityWebRequest);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="UnityWebRequest"/> has been disposed.
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request.</param>
        /// <returns><see langword="true"/> if <paramref name="unityWebRequest"/> has been disposed; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        public static bool IsDisposed(UnityWebRequest unityWebRequest)
        {
            if (unityWebRequest is null)
            {
                throw new ArgumentNullException(nameof(unityWebRequest));
            }
            return InternalIsDisposed(unityWebRequest);
        }

        /// <summary>
        /// Throws a <see cref="UnityWebRequestException"/> if the specified <see cref="UnityWebRequest"/> encounters an error.
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> has been disposed.</exception>
        /// <exception cref="UnityException">This method is called from off the Unity main thread.</exception>
        /// <exception cref="ArgumentException"><paramref name="unityWebRequest"/> is currently in the middle of a request.</exception>
        /// <exception cref="UnityWebRequestException">The web request encountered an error.</exception>
        public static void ThrowIfFaulted(UnityWebRequest unityWebRequest)
        {
            if (unityWebRequest is null)
            {
                throw new ArgumentNullException(nameof(unityWebRequest));
            }
            InternalThrowIfDisposed(unityWebRequest);
            InternalThrowIfFaulted(unityWebRequest);
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the specified <see cref="UnityWebRequest"/>'s HTTP status code does not indicate a success status.
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        /// <exception cref="UnityException">This method is called from off the Unity main thread.</exception>
        /// <exception cref="UnityWebRequestException">The HTTP status code of <paramref name="unityWebRequest"/> does not indicate a success status.</exception>
        public static void ThrowIfNotSuccessStatusCode(UnityWebRequest unityWebRequest)
        {
            if (unityWebRequest is null)
            {
                throw new ArgumentNullException(nameof(unityWebRequest));
            }
            InternalThrowIfNotSuccessStatusCode(unityWebRequest);
        }

        /// <summary>
        /// Asynchronously sends a web request using the specified <see cref="UnityWebRequest"/>, then returns the received data (as a string).
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task object of the asynchronous operation, whose <see cref="Task{TResult}.Result"/> is a string.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> has been disposed, or is disposed during this asynchronous operation.</exception>
        /// <exception cref="UnityException">This method is called from off the Unity main thread.</exception>
        /// <exception cref="ArgumentException"><paramref name="unityWebRequest"/> has already called <see cref="UnityWebRequest.SendWebRequest"/>.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled, or a cancellation request is issued during this asynchronous operation.</exception>
        /// <exception cref="UnityWebRequestException">The web request encountered an error.</exception>
        public static async Task<string> GetStringAsync(
            UnityWebRequest   unityWebRequest,
            CancellationToken cancellationToken = default)
        {
            await SendWebRequestAsync(unityWebRequest, cancellationToken);
            InternalThrowIfNotSuccessStatusCode(unityWebRequest);
            return unityWebRequest.downloadHandler.text;
        }

        /// <summary>
        /// Asynchronously sends a web request using the specified <see cref="UnityWebRequest"/>, then returns the received data (as a byte array).
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task object of the asynchronous operation, whose <see cref="Task{TResult}.Result"/> is a byte array.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> has been disposed, or is disposed during this asynchronous operation.</exception>
        /// <exception cref="UnityException">This method is called from off the Unity main thread.</exception>
        /// <exception cref="ArgumentException"><paramref name="unityWebRequest"/> has already called <see cref="UnityWebRequest.SendWebRequest"/>.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled, or a cancellation request is issued during this asynchronous operation.</exception>
        /// <exception cref="UnityWebRequestException">The web request encountered an error.</exception>
        public static async Task<byte[]> GetByteArrayAsync(
            UnityWebRequest   unityWebRequest,
            CancellationToken cancellationToken = default)
        {
            await SendWebRequestAsync(unityWebRequest, cancellationToken);
            InternalThrowIfNotSuccessStatusCode(unityWebRequest);
            return unityWebRequest.downloadHandler.data;
        }

        /// <summary>
        /// Asynchronously sends a web request using the specified <see cref="UnityWebRequest"/>, then returns the received data (as a stream).
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task object of the asynchronous operation, whose <see cref="Task{TResult}.Result"/> is a stream.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> has been disposed, or is disposed during this asynchronous operation.</exception>
        /// <exception cref="UnityException">This method is called from off the Unity main thread.</exception>
        /// <exception cref="ArgumentException"><paramref name="unityWebRequest"/> has already called <see cref="UnityWebRequest.SendWebRequest"/>.</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> has been canceled, or a cancellation request is issued during this asynchronous operation.</exception>
        /// <exception cref="UnityWebRequestException">The web request encountered an error.</exception>
        public static async Task<Stream> GetStreamAsync(
            UnityWebRequest   unityWebRequest,
            CancellationToken cancellationToken = default)
        {
            await SendWebRequestAsync(unityWebRequest, cancellationToken);
            InternalThrowIfNotSuccessStatusCode(unityWebRequest);
            return new MemoryStream(unityWebRequest.downloadHandler.data);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="UnityWebRequest"/> has timed out.
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request.</param>
        /// <returns><see langword="true"/> if <paramref name="unityWebRequest"/> has timed out; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        /// <exception cref="UnityException">This method is called from off the Unity main thread.</exception>
        public static bool IsTimeout(UnityWebRequest unityWebRequest)
        {
            if (unityWebRequest is null)
            {
                throw new ArgumentNullException(nameof(unityWebRequest));
            }
            return unityWebRequest.error == UnityUtility.UnityWebRequestTimeoutString;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="UnityWebRequest"/>'s HTTP status code indicates a success status.
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request.</param>
        /// <returns><see langword="true"/> if the HTTP status code of <paramref name="unityWebRequest"/> indicates a success status; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        /// <exception cref="UnityException">This method is called from off the Unity main thread.</exception>
        public static bool IsSuccessStatusCode(UnityWebRequest unityWebRequest)
        {
            if (unityWebRequest is null)
            {
                throw new ArgumentNullException(nameof(unityWebRequest));
            }
            return InternalIsSuccessStatusCode(unityWebRequest);
        }

        private static void InternalThrowIfDisposed(UnityWebRequest unityWebRequest)
        {
            if (!InternalIsDisposed(unityWebRequest))
            {
                return;
            }
            throw new ObjectDisposedException(unityWebRequest.GetType().FullName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool InternalIsDisposed(UnityWebRequest unityWebRequest)
        {
            var ptr = (IntPtr)PtrFieldInfo.GetValue(unityWebRequest);
            return ptr == IntPtr.Zero;
        }

        private static void InternalThrowIfFaulted(UnityWebRequest unityWebRequest)
        {
            switch (unityWebRequest.result)
            {
                case UnityWebRequest.Result.InProgress:
                    throw new ArgumentException(InProgressMessage, nameof(unityWebRequest));
                case UnityWebRequest.Result.Success:
                    break;
                case UnityWebRequest.Result.ConnectionError:
                    throw new UnityWebRequestException(unityWebRequest, ConnectionErrorMessage);
                case UnityWebRequest.Result.ProtocolError:
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    throw new UnityWebRequestException(unityWebRequest, DataProcessingErrorMessage);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void InternalThrowIfNotSuccessStatusCode(UnityWebRequest unityWebRequest)
        {
            if (InternalIsSuccessStatusCode(unityWebRequest))
            {
                return;
            }
            throw new UnityWebRequestException(
                unityWebRequest,
                string.Format(NotSuccessStatusCodeMessage, (int)unityWebRequest.responseCode)
            );
        }

        private static bool InternalIsSuccessStatusCode(UnityWebRequest unityWebRequest)
        {
            return IsSuccessStatusCode((HttpStatusCode)unityWebRequest.responseCode);
        }

        private static bool IsSuccessStatusCode(HttpStatusCode statusCode)
        {
            return statusCode is >= HttpStatusCode.OK and <= (HttpStatusCode)299;
        }
    }
}
