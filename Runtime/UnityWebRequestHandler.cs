using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Aurora.Unity.CompilerServices;
using Aurora.Unity.PlayerLoop;
using UnityEngine.Networking;

namespace Aurora.Unity
{
    /// <summary>
    /// Implements <see cref="HttpMessageHandler"/> using <see cref="UnityWebRequest"/>.
    /// </summary>
    public sealed class UnityWebRequestHandler : HttpMessageHandler
    {
        private readonly Action<UnityWebRequest> _actionPrepareUnityWebRequest;

        private const string ContentLengthName = "Content-Length";

        private const string ContentTypeName = "Content-Type";

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityWebRequestHandler"/> class.
        /// </summary>
        public UnityWebRequestHandler()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnityWebRequestHandler"/> class.
        /// </summary>
        /// <param name="actionPrepareUnityWebRequest">A method that performs some custom setup on the <see cref="UnityWebRequest"/> instance that will be used to make the request.</param>
        public UnityWebRequestHandler(Action<UnityWebRequest> actionPrepareUnityWebRequest)
        {
            _actionPrepareUnityWebRequest = actionPrepareUnityWebRequest;
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken  cancellationToken)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (!UnityEnvironment.OnUnityMainThread)
            {
                await new PlayerLoopPhaseAwaitable.Any(EnumUtility<PlayerLoopPhase>.Values, cancellationToken);
            }
            using var unityWebRequest = await CreateAndPrepareUnityWebRequestAsync(request, cancellationToken);
            _actionPrepareUnityWebRequest?.Invoke(unityWebRequest);
            try
            {
                await unityWebRequest.SendWebRequestAsync(cancellationToken);
            }
            catch (UnityWebRequestException e)
            {
                throw new HttpRequestException(null, e);
            }
            return CreateResponseMessage(unityWebRequest, request);
        }

        private static async Task<UnityWebRequest> CreateAndPrepareUnityWebRequestAsync(
            HttpRequestMessage request,
            CancellationToken  cancellationToken)
        {
            UploadHandler      uploadHandler;
            HttpContentHeaders contentHeaders;
            bool               contentTypeSet;
            var                content = request.Content;
            if (content != null)
            {
                var contentByteArray = await content.ReadAsByteArrayAsync();
                cancellationToken.ThrowIfCancellationRequested();
                uploadHandler  = new UploadHandlerRaw(contentByteArray);
                contentHeaders = content.Headers;
                contentTypeSet = SetContentType(uploadHandler, contentHeaders);
            }
            else
            {
                uploadHandler  = null;
                contentHeaders = null;
                contentTypeSet = false;
            }
            var unityWebRequest = new UnityWebRequest(
                request.RequestUri,
                request.Method.Method,
                new DownloadHandlerBuffer(),
                uploadHandler
            );
            if (contentTypeSet)
            {
                SetHeadersExceptContentType(unityWebRequest, contentHeaders);
            }
            else if (contentHeaders != null)
            {
                SetHeaders(unityWebRequest, contentHeaders);
            }
            SetHeaders(unityWebRequest, request.Headers);
            return unityWebRequest;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool SetContentType(UploadHandler uploadHandler, HttpHeaders headers)
        {
            if (!headers.TryGetValues(ContentTypeName, out var values))
            {
                return false;
            }
            uploadHandler.contentType = CombineValues(values);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetHeadersExceptContentType(UnityWebRequest unityWebRequest, HttpHeaders headers)
        {
            foreach (var (name, values) in headers)
            {
                if (string.Compare(name, ContentTypeName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    continue;
                }
                SetHeader(unityWebRequest, name, values);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetHeaders(UnityWebRequest unityWebRequest, HttpHeaders headers)
        {
            foreach (var (name, values) in headers)
            {
                SetHeader(unityWebRequest, name, values);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetHeader(UnityWebRequest unityWebRequest, string name, IEnumerable<string> values)
        {
            SetHeader(unityWebRequest, name, CombineValues(values));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetHeader(UnityWebRequest unityWebRequest, string name, string value)
        {
            unityWebRequest.SetRequestHeader(name, value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string CombineValues(IEnumerable<string> values)
        {
            return string.Join(", ", values);
        }

        private static HttpResponseMessage CreateResponseMessage(
            UnityWebRequest    unityWebRequest,
            HttpRequestMessage request)
        {
            var responseMessage = new HttpResponseMessage((HttpStatusCode)unityWebRequest.responseCode);
            responseMessage.ReasonPhrase = unityWebRequest.error;
            responseMessage.RequestMessage = request;
            responseMessage.Content = new ByteArrayContent(unityWebRequest.downloadHandler.data ?? Array.Empty<byte>());
            request.RequestUri = unityWebRequest.uri;
            var headers = unityWebRequest.GetResponseHeaders();
            if (headers != null)
            {
                var contentHeaders  = responseMessage.Content.Headers;
                var responseHeaders = responseMessage.Headers;
                if (unityWebRequest.GetResponseHeader(ContentLengthName) is var contentLengthValue &&
                    long.TryParse(contentLengthValue, out var contentLength) && contentLength >= 0L)
                {
                    contentHeaders.ContentLength = contentLength;
                }
                else if (unityWebRequest.downloadedBytes <= long.MaxValue)
                {
                    contentHeaders.ContentLength = (long)unityWebRequest.downloadedBytes;
                }
                foreach (var (name, value) in headers)
                {
                    if (string.Compare(name, ContentLengthName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        continue;
                    }
                    if (!responseHeaders.TryAddWithoutValidation(name, value))
                    {
                        contentHeaders.TryAddWithoutValidation(name, value);
                    }
                }
            }
            return responseMessage;
        }
    }
}
