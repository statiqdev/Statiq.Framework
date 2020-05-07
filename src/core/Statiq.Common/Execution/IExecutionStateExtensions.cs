using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IExecutionStateExtensions
    {
        /// <summary>
        /// Sends a GET request with exponential back-off.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="uri">The request URI.</param>
        /// <returns>The response.</returns>
        public static async Task<HttpResponseMessage> SendHttpRequestWithRetryAsync(this IExecutionState executionState, string uri) =>
            await executionState.SendHttpRequestWithRetryAsync(() => new HttpRequestMessage(HttpMethod.Get, uri));

        /// <summary>
        /// Sends a GET request with exponential back-off.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="uri">The request URI.</param>
        /// <returns>The response.</returns>
        public static async Task<HttpResponseMessage> SendHttpRequestWithRetryAsync(this IExecutionState executionState, Uri uri) =>
            await executionState.SendHttpRequestWithRetryAsync(() => new HttpRequestMessage(HttpMethod.Get, uri));
    }
}
