using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Statiq.Common;

namespace Statiq.Core
{
    public static class HttpClientExtensions
    {
        private const int MaxRetry = 5;
        private const HttpStatusCode TooManyRequests = (HttpStatusCode)429;

        /// <summary>
        /// Retries a request with exponential back-off. This helps with websites like GitHub that will give us a 429 (TooManyRequests).
        /// </summary>
        /// <param name="httpClient">The client.</param>
        /// <param name="requestFactory">A factory that creates the request message to send (a fresh message is needed for each request).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The response.</returns>
        public static async Task<HttpResponseMessage> SendWithRetryAsync(this HttpClient httpClient, Func<HttpRequestMessage> requestFactory, CancellationToken cancellationToken = default)
        {
            _ = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _ = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));

            AsyncRetryPolicy<HttpResponseMessage> retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => r.StatusCode == TooManyRequests)
                .WaitAndRetryAsync(MaxRetry, attempt =>
                {
                    IExecutionContext.CurrentOrNull?.LogDebug($"HttpClient retry {attempt}");
                    return TimeSpan.FromSeconds(0.5 * Math.Pow(2, attempt));
                });

            return await retryPolicy.ExecuteAsync(
                async ct =>
                {
                    HttpRequestMessage request = requestFactory();
                    return await httpClient.SendAsync(request, ct);
                },
                cancellationToken);
        }

        /// <summary>
        /// Retries a GET request with exponential back-off. This helps with websites like GitHub that will give us a 429 (TooManyRequests).
        /// </summary>
        /// <param name="httpClient">The client.</param>
        /// <param name="uri">The request URI.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The response.</returns>
        public static async Task<HttpResponseMessage> SendWithRetryAsync(this HttpClient httpClient, string uri, CancellationToken cancellationToken = default) =>
            await SendWithRetryAsync(httpClient, () => new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken);

        /// <summary>
        /// Retries a GET request with exponential back-off. This helps with websites like GitHub that will give us a 429 (TooManyRequests).
        /// </summary>
        /// <param name="httpClient">The client.</param>
        /// <param name="uri">The request URI.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The response.</returns>
        public static async Task<HttpResponseMessage> SendWithRetryAsync(this HttpClient httpClient, Uri uri, CancellationToken cancellationToken = default) =>
            await SendWithRetryAsync(httpClient, () => new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken);
    }
}
