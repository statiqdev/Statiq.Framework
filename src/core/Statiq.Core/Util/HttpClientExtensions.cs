using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
        /// <param name="request">The request.</param>
        /// <returns>The response.</returns>
        public static async Task<HttpResponseMessage> SendWithRetryAsync(this HttpClient httpClient, HttpRequestMessage request)
        {
            _ = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _ = request ?? throw new ArgumentNullException(nameof(request));

            AsyncRetryPolicy<HttpResponseMessage> retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => r.StatusCode == TooManyRequests)
                .WaitAndRetryAsync(MaxRetry, attempt =>
                {
                    IExecutionContext.CurrentOrNull?.LogDebug($"HttpClient retry {attempt} ({request.RequestUri})");
                    return TimeSpan.FromSeconds(0.5 * Math.Pow(2, attempt));
                });

            return await retryPolicy.ExecuteAsync(async () => await httpClient.SendAsync(request));
        }

        /// <summary>
        /// Retries a GET request with exponential back-off. This helps with websites like GitHub that will give us a 429 (TooManyRequests).
        /// </summary>
        /// <param name="httpClient">The client.</param>
        /// <param name="uri">The request URI.</param>
        /// <returns>The response.</returns>
        public static async Task<HttpResponseMessage> SendWithRetryAsync(this HttpClient httpClient, string uri) =>
            await SendWithRetryAsync(httpClient, new HttpRequestMessage(HttpMethod.Get, uri));

        /// <summary>
        /// Retries a GET request with exponential back-off. This helps with websites like GitHub that will give us a 429 (TooManyRequests).
        /// </summary>
        /// <param name="httpClient">The client.</param>
        /// <param name="uri">The request URI.</param>
        /// <returns>The response.</returns>
        public static async Task<HttpResponseMessage> SendWithRetryAsync(this HttpClient httpClient, Uri uri) =>
            await SendWithRetryAsync(httpClient, new HttpRequestMessage(HttpMethod.Get, uri));
    }
}
