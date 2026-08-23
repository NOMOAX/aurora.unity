using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Aurora.Unity
{
    /// <summary>
    /// The exception thrown when a Unity web request encounters an error.
    /// </summary>
    public class UnityWebRequestException : UnityException
    {
        /// <summary>
        /// The Unity web request.
        /// </summary>
        public UnityWebRequest UnityWebRequest { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityWebRequestException"/> class with the specified Unity web request.
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request.</param>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        public UnityWebRequestException(UnityWebRequest unityWebRequest)
        {
            UnityWebRequest = unityWebRequest ?? throw new ArgumentNullException(nameof(unityWebRequest));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityWebRequestException"/> class with the specified Unity web request and error message.
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request that encountered the error.</param>
        /// <param name="message">The message describing the error.</param>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        public UnityWebRequestException(UnityWebRequest unityWebRequest, string message) : base(message)
        {
            UnityWebRequest = unityWebRequest ?? throw new ArgumentNullException(nameof(unityWebRequest));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityWebRequestException"/> class with the specified Unity web request, error message, and inner exception.
        /// </summary>
        /// <param name="unityWebRequest">The Unity web request that encountered the error.</param>
        /// <param name="message">The message describing the error.</param>
        /// <param name="innerException">The exception that caused this exception.</param>
        /// <exception cref="ArgumentNullException"><paramref name="unityWebRequest"/> is <see langword="null"/>.</exception>
        public UnityWebRequestException(UnityWebRequest unityWebRequest, string message, Exception innerException) :
            base(message, innerException)
        {
            UnityWebRequest = unityWebRequest ?? throw new ArgumentNullException(nameof(unityWebRequest));
        }
    }
}
