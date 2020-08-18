using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.ApiClient
{
    /// <summary>
    /// Outputs metadata for information from any API with its client.
    /// </summary>
    /// <remarks>
    /// This modules used to submit requests to the API with the API client. Because of the possible large
    /// number of different kinds of requests, this module does not attempt to provide a fully abstract wrapper
    /// around the API. Instead, it simplifies the housekeeping involved in setting up an
    /// API client and requires you to provide functions that fetch whatever data you need. Each request
    /// will be sent for each input document.
    /// </remarks>
    /// <category>Metadata</category>
    public class ReadApiWithClient<TClient> : ParallelSyncModule
        where TClient : class
    {
        private readonly Dictionary<string, Func<IDocument, IExecutionContext, TClient, object>> _requests
            = new Dictionary<string, Func<IDocument, IExecutionContext, TClient, object>>();

        private readonly TClient _client;

        private Action<IDocument, IExecutionContext, TClient> _init;

        private string _clientName;

        /// <summary>
        /// Creates a connection to the API using client.
        /// </summary>
        /// <param name="client">The API client to use.</param>
        public ReadApiWithClient(TClient client)
        {
            _client = client ?? throw new ArgumentException("Argument is null", nameof(client));
        }

        /// <summary>
        /// Initialize the API client.
        /// </summary>
        /// <param name="init">An API client initialization action.</param>
        /// <returns>The current module instance.</returns>
        public ReadApiWithClient<TClient> WithClientInitialization(Action<TClient> init)
        {
            if (init == null)
            {
                throw new ArgumentException("Argument is null or empty", nameof(init));
            }

            init.Invoke(_client);
            return this;
        }

        /// <summary>
        /// Initialize the API client.
        /// </summary>
        /// <param name="init">An API client initialization action.</param>
        /// <returns>The current module instance.</returns>
        public ReadApiWithClient<TClient> WithClientInitialization(Action<IExecutionContext, TClient> init)
        {
            if (init == null)
            {
                throw new ArgumentException("Argument is null or empty", nameof(init));
            }

            _init = (doc, ctx, client) => init(ctx, client);
            return this;
        }

        /// <summary>
        /// Initialize the API client.
        /// </summary>
        /// <param name="init">An API client initialization action.</param>
        /// <returns>The current module instance.</returns>
        public ReadApiWithClient<TClient> WithClientInitialization(Action<IDocument, IExecutionContext, TClient> init)
        {
            if (init == null)
            {
                throw new ArgumentException("Argument is null or empty", nameof(init));
            }

            _init = init.ThrowIfNull(nameof(init));
            return this;
        }

        /// <summary>
        /// Submits the API client name (for logging).
        /// </summary>
        /// <param name="name">Client name.</param>
        /// <returns>The current module instance.</returns>
        public ReadApiWithClient<TClient> WithClientName(string name)
        {
            _clientName = name.ThrowIfNullOrWhiteSpace(nameof(name));
            return this;
        }

        /// <summary>
        /// Submits a request to the API client. This allows you to incorporate data from the execution context in your request.
        /// </summary>
        /// <param name="key">The metadata key in which to store the return value of the request function.</param>
        /// <param name="request">A function with the request to make.</param>
        /// <returns>The current module instance.</returns>
        public ReadApiWithClient<TClient> WithRequest(string key, Func<IExecutionContext, TClient, object> request)
        {
            key.ThrowIfNullOrWhiteSpace(nameof(key));
            request.ThrowIfNull(nameof(request));

            _requests[key] = (doc, ctx, client) => request(ctx, client);
            return this;
        }

        /// <summary>
        /// Submits a request to the API. This allows you to incorporate data from the execution context and current document in your request.
        /// </summary>
        /// <param name="key">The metadata key in which to store the return value of the request function.</param>
        /// <param name="request">A function with the request to make.</param>
        /// <returns>The current module instance.</returns>
        public ReadApiWithClient<TClient> WithRequest(string key, Func<IDocument, IExecutionContext, TClient, object> request)
        {
            key.ThrowIfNullOrWhiteSpace(nameof(key));

            _requests[key] = request.ThrowIfNull(nameof(request));
            return this;
        }

        /// <inheritdoc/>
        protected override IEnumerable<IDocument> ExecuteInput(IDocument input, IExecutionContext context)
        {
            _init?.Invoke(input, context, _client);
            ConcurrentDictionary<string, object> results = new ConcurrentDictionary<string, object>();
            System.Threading.Tasks.Parallel.ForEach(_requests, request =>
            {
                context.LogDebug("Submitting {0} {1} request for {2}", request.Key, _clientName, input.ToSafeDisplayString());
                try
                {
                    results[request.Key] = request.Value(input, context, _client);
                }
                catch (Exception ex)
                {
                    context.LogWarning("Exception while submitting {0} {1} request for {2}: {3}", request.Key, _clientName, input.ToSafeDisplayString(), ex.ToString());
                }
            });
            return input.Clone(results).Yield();
        }
    }
}
