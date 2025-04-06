using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Aurora.Unity
{
    /// <summary>
    /// 为 <see cref="UnityWebRequest"/> 类提供扩展方法。
    /// </summary>
    public static class UnityWebRequestExtensions
    {
        /// <summary>
        /// 使用当前 <see cref="UnityWebRequest"/> 异步发送网络请求。
        /// </summary>
        /// <param name="unityWebRequest">Unity 网络请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>异步操作的任务对象。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> 已释放，或者在此异步操作执行过程中释放。</exception>
        /// <exception cref="UnityException">从非 Unity 主线程调用此方法。</exception>
        /// <exception cref="ArgumentException"><paramref name="unityWebRequest"/> 已调用 <see cref="UnityWebRequest.SendWebRequest"/>。</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> 已取消，或者在此异步操作执行过程中发出取消请求。</exception>
        /// <exception cref="UnityWebRequestException">网络请求遇到错误（但不是由于 HTTP 状态码不表示成功状态）。</exception>
        public static Task SendWebRequestAsync(
            this UnityWebRequest unityWebRequest,
            CancellationToken    cancellationToken = default)
        {
            return UnityWebRequestUtility.SendWebRequestAsync(unityWebRequest, cancellationToken);
        }

        /// <summary>
        /// 使用当前 <see cref="UnityWebRequest"/> 异步发送网络请求，然后返回接收到的数据（以字符串的形式）。
        /// </summary>
        /// <param name="unityWebRequest">Unity 网络请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>异步操作的任务对象，它的 <see cref="Task{TResult}.Result"/> 为字符串。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> 已释放，或者在此异步操作执行过程中释放。</exception>
        /// <exception cref="ArgumentException"><paramref name="unityWebRequest"/> 已调用 <see cref="UnityWebRequest.SendWebRequest"/>。</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> 已取消，或者在此异步操作执行过程中发出取消请求。</exception>
        /// <exception cref="UnityWebRequestException">网络请求遇到错误。</exception>
        public static Task<string> GetStringAsync(
            this UnityWebRequest unityWebRequest,
            CancellationToken    cancellationToken = default)
        {
            return UnityWebRequestUtility.GetStringAsync(unityWebRequest, cancellationToken);
        }

        /// <summary>
        /// 使用当前 <see cref="UnityWebRequest"/> 异步发送网络请求，然后返回接收到的数据（以字节数组的形式）。
        /// </summary>
        /// <param name="unityWebRequest">Unity 网络请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>异步操作的任务对象，它的 <see cref="Task{TResult}.Result"/> 为字节数组。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> 已释放，或者在此异步操作执行过程中释放。</exception>
        /// <exception cref="ArgumentException"><paramref name="unityWebRequest"/> 已调用 <see cref="UnityWebRequest.SendWebRequest"/>。</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> 已取消，或者在此异步操作执行过程中发出取消请求。</exception>
        /// <exception cref="UnityWebRequestException">网络请求遇到错误。</exception>
        public static Task<byte[]> GetByteArrayAsync(
            this UnityWebRequest unityWebRequest,
            CancellationToken    cancellationToken = default)
        {
            return UnityWebRequestUtility.GetByteArrayAsync(unityWebRequest, cancellationToken);
        }

        /// <summary>
        /// 使用当前 <see cref="UnityWebRequest"/> 异步发送网络请求，然后返回接收到的数据（以流的形式）。
        /// </summary>
        /// <param name="unityWebRequest">Unity 网络请求。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>异步操作的任务对象，它的 <see cref="Task{TResult}.Result"/> 为流。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ObjectDisposedException"><paramref name="unityWebRequest"/> 已释放，或者在此异步操作执行过程中释放。</exception>
        /// <exception cref="ArgumentException"><paramref name="unityWebRequest"/> 已调用 <see cref="UnityWebRequest.SendWebRequest"/>。</exception>
        /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> 已取消，或者在此异步操作执行过程中发出取消请求。</exception>
        /// <exception cref="UnityWebRequestException">网络请求遇到错误。</exception>
        public static Task<Stream> GetStreamAsync(
            this UnityWebRequest unityWebRequest,
            CancellationToken    cancellationToken = default)
        {
            return UnityWebRequestUtility.GetStreamAsync(unityWebRequest, cancellationToken);
        }

        /// <summary>
        /// 如果当前 <see cref="UnityWebRequest"/> 的 HTTP 状态码不表示成功状态，则抛出 <see cref="ArgumentException"/>。
        /// </summary>
        /// <param name="unityWebRequest">Unity 网络请求。</param>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="UnityWebRequestException"><paramref name="unityWebRequest"/> 的 HTTP 状态码不表示成功状态。</exception>
        public static void ThrowIfNotSuccessStatusCode(this UnityWebRequest unityWebRequest)
        {
            UnityWebRequestUtility.ThrowIfNotSuccessStatusCode(unityWebRequest);
        }

        /// <summary>
        /// 获取一个值，这个值指示当前 <see cref="UnityWebRequest"/> 是否超时。
        /// </summary>
        /// <param name="unityWebRequest">Unity 网络请求。</param>
        /// <returns>如果 <paramref name="unityWebRequest"/> 超时，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        public static bool IsTimeout(this UnityWebRequest unityWebRequest)
        {
            return UnityWebRequestUtility.IsTimeout(unityWebRequest);
        }

        /// <summary>
        /// 获取一个值，这个值指示当前 <see cref="UnityWebRequest"/> 的 HTTP 状态码是否表示成功状态。
        /// </summary>
        /// <param name="unityWebRequest">Unity 网络请求。</param>
        /// <returns>如果 <paramref name="unityWebRequest"/> 的 HTTP 状态码表示成功状态，则为 <see langword="true"/>；否则为 <see langword="false"/>。</returns>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        public static bool IsSuccessStatusCode(this UnityWebRequest unityWebRequest)
        {
            return UnityWebRequestUtility.IsSuccessStatusCode(unityWebRequest);
        }
    }
}
