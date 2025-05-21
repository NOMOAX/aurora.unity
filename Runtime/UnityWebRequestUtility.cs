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
    /// 为 <see cref="UnityWebRequest"/> 类提供工具方法。
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
        /// 使用指定的 <see cref="UnityWebRequest"/> 异步发送网络请求。
        /// </summary>
        /// <param name="unityWebRequest">Unity 网络请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> 已释放，或者在此异步操作执行过程中释放。</exception>
        /// <exception cref="UnityException">从非 Unity 主线程调用此方法。</exception>
        /// <exception cref="ArgumentException"><paramref name="unityWebRequest"/> 已调用 <see cref="UnityWebRequest.SendWebRequest"/>。</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> 已取消，或者在此异步操作执行过程中发出取消请求。</exception>
        /// <exception cref="UnityWebRequestException">网络请求遇到错误（但不是由于 HTTP 状态码不表示成功状态）。</exception>
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
        /// 如果指定的 <see cref="UnityWebRequest"/> 已释放，则抛出 <see cref="ObjectDisposedException"/>。
        /// </summary>
        /// <param name="unityWebRequest">Unity 网络请求。</param>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> 已释放。</exception>
        public static void ThrowIfDisposed(UnityWebRequest unityWebRequest)
        {
            if (unityWebRequest is null)
            {
                throw new ArgumentNullException(nameof(unityWebRequest));
            }
            InternalThrowIfDisposed(unityWebRequest);
        }

        /// <summary>
        /// 获取一个值，这个值指示 <see cref="UnityWebRequest"/> 是否已释放。
        /// </summary>
        /// <param name="unityWebRequest">Unity 网络请求。</param>
        /// <returns>如果 <paramref name="unityWebRequest"/> 已释放，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        public static bool IsDisposed(UnityWebRequest unityWebRequest)
        {
            if (unityWebRequest is null)
            {
                throw new ArgumentNullException(nameof(unityWebRequest));
            }
            return InternalIsDisposed(unityWebRequest);
        }

        /// <summary>
        /// 如果指定的 <see cref="UnityWebRequest"/> 遇到错误，则抛出 <see cref="UnityWebRequestException"/>。
        /// </summary>
        /// <param name="unityWebRequest">Unity 网络请求。</param>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> 已释放。</exception>
        /// <exception cref="UnityException">从非 Unity 主线程调用此方法。</exception>
        /// <exception cref="ArgumentException"><paramref name="unityWebRequest"/> 正处于请求过程中。</exception>
        /// <exception cref="UnityWebRequestException">网络请求遇到错误。</exception>
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
        /// 如果指定的 <see cref="UnityWebRequest"/> 的 HTTP 状态码不表示成功状态，则抛出 <see cref="ArgumentException"/>。
        /// </summary>
        /// <param name="unityWebRequest">Unity 网络请求。</param>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="UnityException">从非 Unity 主线程调用此方法。</exception>
        /// <exception cref="UnityWebRequestException"><paramref name="unityWebRequest"/> 的 HTTP 状态码不表示成功状态。</exception>
        public static void ThrowIfNotSuccessStatusCode(UnityWebRequest unityWebRequest)
        {
            if (unityWebRequest is null)
            {
                throw new ArgumentNullException(nameof(unityWebRequest));
            }
            InternalThrowIfNotSuccessStatusCode(unityWebRequest);
        }

        /// <summary>
        /// 使用指定的 <see cref="UnityWebRequest"/> 异步发送网络请求，然后返回接收到的数据（以字符串的形式）。
        /// </summary>
        /// <param name="unityWebRequest">Unity 网络请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>异步操作的任务对象，它的 <see cref="Task{TResult}.Result"/> 为字符串。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> 已释放，或者在此异步操作执行过程中释放。</exception>
        /// <exception cref="UnityException">从非 Unity 主线程调用此方法。</exception>
        /// <exception cref="ArgumentException"><paramref name="unityWebRequest"/> 已调用 <see cref="UnityWebRequest.SendWebRequest"/>。</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> 已取消，或者在此异步操作执行过程中发出取消请求。</exception>
        /// <exception cref="UnityWebRequestException">网络请求遇到错误。</exception>
        public static async Task<string> GetStringAsync(
            UnityWebRequest   unityWebRequest,
            CancellationToken cancellationToken = default)
        {
            await SendWebRequestAsync(unityWebRequest, cancellationToken);
            InternalThrowIfNotSuccessStatusCode(unityWebRequest);
            return unityWebRequest.downloadHandler.text;
        }

        /// <summary>
        /// 使用指定的 <see cref="UnityWebRequest"/> 异步发送网络请求，然后返回接收到的数据（以字节数组的形式）。
        /// </summary>
        /// <param name="unityWebRequest">Unity 网络请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>异步操作的任务对象，它的 <see cref="Task{TResult}.Result"/> 为字节数组。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> 已释放，或者在此异步操作执行过程中释放。</exception>
        /// <exception cref="UnityException">从非 Unity 主线程调用此方法。</exception>
        /// <exception cref="ArgumentException"><paramref name="unityWebRequest"/> 已调用 <see cref="UnityWebRequest.SendWebRequest"/>。</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> 已取消，或者在此异步操作执行过程中发出取消请求。</exception>
        /// <exception cref="UnityWebRequestException">网络请求遇到错误。</exception>
        public static async Task<byte[]> GetByteArrayAsync(
            UnityWebRequest   unityWebRequest,
            CancellationToken cancellationToken = default)
        {
            await SendWebRequestAsync(unityWebRequest, cancellationToken);
            InternalThrowIfNotSuccessStatusCode(unityWebRequest);
            return unityWebRequest.downloadHandler.data;
        }

        /// <summary>
        /// 使用指定的 <see cref="UnityWebRequest"/> 异步发送网络请求，然后返回接收到的数据（以流的形式）。
        /// </summary>
        /// <param name="unityWebRequest">Unity 网络请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>异步操作的任务对象，它的 <see cref="Task{TResult}.Result"/> 为流。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> 已释放，或者在此异步操作执行过程中释放。</exception>
        /// <exception cref="UnityException">从非 Unity 主线程调用此方法。</exception>
        /// <exception cref="ArgumentException"><paramref name="unityWebRequest"/> 已调用 <see cref="UnityWebRequest.SendWebRequest"/>。</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> 已取消，或者在此异步操作执行过程中发出取消请求。</exception>
        /// <exception cref="UnityWebRequestException">网络请求遇到错误。</exception>
        public static async Task<Stream> GetStreamAsync(
            UnityWebRequest   unityWebRequest,
            CancellationToken cancellationToken = default)
        {
            await SendWebRequestAsync(unityWebRequest, cancellationToken);
            InternalThrowIfNotSuccessStatusCode(unityWebRequest);
            return new MemoryStream(unityWebRequest.downloadHandler.data);
        }

        /// <summary>
        /// 获取一个值，这个值指示 <see cref="UnityWebRequest"/> 是否超时。
        /// </summary>
        /// <param name="unityWebRequest">Unity 网络请求。</param>
        /// <returns>如果 <paramref name="unityWebRequest"/> 超时，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="UnityException">从非 Unity 主线程调用此方法。</exception>
        public static bool IsTimeout(UnityWebRequest unityWebRequest)
        {
            if (unityWebRequest is null)
            {
                throw new ArgumentNullException(nameof(unityWebRequest));
            }
            return unityWebRequest.error == UnityUtility.UnityWebRequestTimeoutString;
        }

        /// <summary>
        /// 获取一个值，这个值指示 <see cref="UnityWebRequest"/> 的 HTTP 状态码是否表示成功状态。
        /// </summary>
        /// <param name="unityWebRequest">Unity 网络请求。</param>
        /// <returns>如果 <paramref name="unityWebRequest"/> 的 HTTP 状态码表示成功状态，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="UnityException">从非 Unity 主线程调用此方法。</exception>
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
            var ptr = (IntPtr) PtrFieldInfo.GetValue(unityWebRequest);
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
                string.Format(NotSuccessStatusCodeMessage, (int) unityWebRequest.responseCode)
            );
        }

        private static bool InternalIsSuccessStatusCode(UnityWebRequest unityWebRequest)
        {
            return IsSuccessStatusCode((HttpStatusCode) unityWebRequest.responseCode);
        }

        private static bool IsSuccessStatusCode(HttpStatusCode statusCode)
        {
            return statusCode is >= HttpStatusCode.OK and <= (HttpStatusCode) 299;
        }
    }
}
