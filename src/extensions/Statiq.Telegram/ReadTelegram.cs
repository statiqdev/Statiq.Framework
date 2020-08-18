using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Statiq.Common;
using Telegram.Bot;

namespace Statiq.Telegram
{
    /// <summary>
    /// Outputs metadata for information from Telegram.
    /// </summary>
    /// <remarks>
    /// This modules uses the Telegram.Bot library and associated types to submit requests to Telegram. Because
    /// of the large number of different kinds of requests, this module does not attempt to provide a fully abstract wrapper
    /// around the Telegram.Bot library. Instead, it simplifies the housekeeping involved in setting up an
    /// Telegram client and requires you to provide functions that fetch whatever data you need. Each request
    /// will be sent for each input document.
    /// </remarks>
    /// <category>Metadata</category>
    public class ReadTelegram : ParallelSyncModule
    {
        private readonly Dictionary<string, Func<IDocument, IExecutionContext, TelegramBotClient, object>> _requests
            = new Dictionary<string, Func<IDocument, IExecutionContext, TelegramBotClient, object>>();

        private readonly TelegramBotClient _telegram;

        /// <summary>
        /// Creates a connection to the Telegram API with authenticated access.
        /// </summary>
        /// <param name="accessToken">The API token to use.</param>
        /// <param name="httpClient">Http client.</param>
        public ReadTelegram(string accessToken, HttpClient httpClient = null)
        {
            accessToken.ThrowIfNullOrWhiteSpace(nameof(accessToken));

            _telegram = new TelegramBotClient(accessToken, httpClient);
        }

        /// <summary>
        /// Creates a connection to the Telegram API with authenticated access and proxy.
        /// </summary>
        /// <param name="accessToken">The API token to use.</param>
        /// <param name="proxy">Proxy to use Telegram API.</param>
        public ReadTelegram(string accessToken, IWebProxy proxy)
        {
            accessToken.ThrowIfNullOrWhiteSpace(nameof(accessToken));

            _telegram = new TelegramBotClient(accessToken, proxy);
        }

        /// <summary>
        /// Submits requests timeout to the Telegram client.
        /// </summary>
        /// <param name="timeout">Requests timeout.</param>
        /// <returns>The current module instance.</returns>
        public ReadTelegram WithRequestsTimeout(TimeSpan timeout)
        {
            _telegram.Timeout = timeout;
            return this;
        }

        /// <summary>
        /// Submits a request to the Telegram client. This allows you to incorporate data from the execution context in your request.
        /// </summary>
        /// <param name="key">The metadata key in which to store the return value of the request function.</param>
        /// <param name="request">A function with the request to make.</param>
        /// <returns>The current module instance.</returns>
        public ReadTelegram WithRequest(string key, Func<IExecutionContext, TelegramBotClient, object> request)
        {
            key.ThrowIfNullOrWhiteSpace(nameof(key));
            request.ThrowIfNull(nameof(request));

            _requests[key] = (doc, ctx, telegram) => request(ctx, telegram);
            return this;
        }

        /// <summary>
        /// Submits a request to the Telegram client. This allows you to incorporate data from the execution context and current document in your request.
        /// </summary>
        /// <param name="key">The metadata key in which to store the return value of the request function.</param>
        /// <param name="request">A function with the request to make.</param>
        /// <returns>The current module instance.</returns>
        public ReadTelegram WithRequest(string key, Func<IDocument, IExecutionContext, TelegramBotClient, object> request)
        {
            key.ThrowIfNullOrWhiteSpace(nameof(key));

            _requests[key] = request.ThrowIfNull(nameof(request));
            return this;
        }

        /// <inheritdoc/>
        protected override IEnumerable<IDocument> ExecuteInput(IDocument input, IExecutionContext context)
        {
            ConcurrentDictionary<string, object> results = new ConcurrentDictionary<string, object>();
            System.Threading.Tasks.Parallel.ForEach(_requests, request =>
            {
                context.LogDebug("Submitting {0} Telegram request for {1}", request.Key, input.ToSafeDisplayString());
                try
                {
                    results[request.Key] = request.Value(input, context, _telegram);
                }
                catch (Exception ex)
                {
                    context.LogWarning("Exception while submitting {0} Telegram request for {1}: {2}", request.Key, input.ToSafeDisplayString(), ex.ToString());
                }
            });
            return input.Clone(results).Yield();
        }
    }
}
