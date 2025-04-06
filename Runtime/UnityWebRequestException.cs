using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Aurora.Unity
{
    /// <summary>
    /// 在 Unity 网络请求遇到错误时引发的异常。
    /// </summary>
    public class UnityWebRequestException : UnityException
    {
        /// <summary>
        /// Unity 网络请求。
        /// </summary>
        public UnityWebRequest UnityWebRequest { get; }

        /// <summary>
        /// 使用指定的 Unity 网络请求初始化 <see cref="UnityWebRequestException"/> 类的新实例。
        /// </summary>
        /// <param name="unityWebRequest">Unity 网络请求。</param>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        public UnityWebRequestException(UnityWebRequest unityWebRequest)
        {
            UnityWebRequest = unityWebRequest ?? throw new ArgumentNullException(nameof(unityWebRequest));
        }

        /// <summary>
        /// 使用指定的 Unity 网络请求和错误消息初始化 <see cref="UnityWebRequestException"/> 类的新实例。
        /// </summary>
        /// <param name="unityWebRequest">遇到错误的 Unity 网络请求。</param>
        /// <param name="message">描述错误的消息。</param>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        public UnityWebRequestException(UnityWebRequest unityWebRequest, string message) : base(message)
        {
            UnityWebRequest = unityWebRequest ?? throw new ArgumentNullException(nameof(unityWebRequest));
        }

        /// <summary>
        /// 使用指定的 Unity 网络请求、错误消息和内部异常初始化 <see cref="UnityWebRequestException"/> 类的新实例。
        /// </summary>
        /// <param name="unityWebRequest">遇到错误的 Unity 网络请求。</param>
        /// <param name="message">描述错误的消息。</param>
        /// <param name="innerException">造成此异常的异常。</param>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> 为 <see langword="null"/>。</exception>
        public UnityWebRequestException(UnityWebRequest unityWebRequest, string message, Exception innerException) :
            base(message, innerException)
        {
            UnityWebRequest = unityWebRequest ?? throw new ArgumentNullException(nameof(unityWebRequest));
        }
    }
}
