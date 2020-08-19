using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
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
    public class ReadApi<TClient> : ParallelModule
        where TClient : class
    {
        private readonly Dictionary<string, Func<IDocument, IExecutionContext, TClient, object>> _requests
            = new Dictionary<string, Func<IDocument, IExecutionContext, TClient, object>>(StringComparer.OrdinalIgnoreCase);

        private readonly Func<TClient> _clientFactory;

        private readonly string _clientName;

        private Action<IDocument, IExecutionContext, TClient> _init;

        private SemaphoreSlim _throttler;

        private int _maxDegreeOfParallelism = -1;

        private uint _requestDelay;

        /// <summary>
        /// Creates a connection to the API using client.
        /// </summary>
        /// <param name="client">The API client to use.</param>
        /// <param name="clientName">Client name (by default is "API").</param>
        public ReadApi(TClient client, string clientName = "API")
        {
            client.ThrowIfNull(nameof(client));
            _clientName = clientName.ThrowIfNullOrWhiteSpace(nameof(clientName));

            _clientFactory = () => client;
        }

        /// <summary>
        /// Creates a connection to the API using client factory.
        /// </summary>
        /// <param name="clientFactory">The API client factory to use.</param>
        /// <param name="clientName">Client name (by default is "API").</param>
        public ReadApi(Func<TClient> clientFactory, string clientName = "API")
        {
            clientFactory.ThrowIfNull(nameof(clientFactory));
            _clientName = clientName.ThrowIfNullOrWhiteSpace(nameof(clientName));

            _clientFactory = clientFactory;
        }

        /// <summary>
        /// Initialize the API client.
        /// </summary>
        /// <param name="init">An API client initialization action.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithClientInitialization(Action<TClient> init)
        {
            init.ThrowIfNull(nameof(init));

            _init = (doc, ctx, client) => init(client);
            return this;
        }

        /// <summary>
        /// Initialize the API client.
        /// </summary>
        /// <param name="init">An API client initialization action.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithClientInitialization(Action<IExecutionContext, TClient> init)
        {
            init.ThrowIfNull(nameof(init));

            _init = (doc, ctx, client) => init(ctx, client);
            return this;
        }

        /// <summary>
        /// Initialize the API client.
        /// </summary>
        /// <param name="init">An API client initialization action.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithClientInitialization(Action<IDocument, IExecutionContext, TClient> init)
        {
            _init = init.ThrowIfNull(nameof(init));
            return this;
        }

        /// <summary>
        /// Submits the maximum number of enabled concurrent tasks for executing requests.
        /// </summary>
        /// <param name="maxDegreeOfParallelism">The maximum number of enabled concurrent tasks for executing requests. Put 1 to execute requests synchronously.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithMaxDegreeOfParallelism(int maxDegreeOfParallelism)
        {
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
            return this;
        }

        /// <summary>
        /// Submits the limit of executing requests.
        /// </summary>
        /// <param name="requestLimit">The limit of executing requests.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithRequestLimit(int requestLimit)
        {
            _throttler = new SemaphoreSlim(requestLimit, requestLimit);
            return this;
        }

        /// <summary>
        /// Submits the requests delay for each request in the current instance of module.
        /// </summary>
        /// <param name="requestDelay">The requests delay in milliseconds.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithRequestDelay(uint requestDelay)
        {
            _requestDelay = requestDelay;
            return this;
        }

        /// <summary>
        /// Submits a request to the API client. This allows you to incorporate data from the execution context in your request.
        /// </summary>
        /// <param name="key">The metadata key in which to store the return value of the request function.</param>
        /// <param name="request">A function with the request to make.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithRequest(string key, Func<IExecutionContext, TClient, object> request)
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
        public ReadApi<TClient> WithRequest(string key, Func<IDocument, IExecutionContext, TClient, object> request)
        {
            key.ThrowIfNullOrWhiteSpace(nameof(key));

            _requests[key] = request.ThrowIfNull(nameof(request));
            return this;
        }

        /// <inheritdoc/>
        protected override Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            TClient client = _clientFactory.Invoke();
            _init?.Invoke(input, context, client);
            ConcurrentDictionary<string, object> results = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            System.Threading.Tasks.Parallel.ForEach(_requests, new ParallelOptions { MaxDegreeOfParallelism = _maxDegreeOfParallelism }, request =>
            {
                context.LogDebug("Submitting {0} {1} request for {2}", request.Key, _clientName, input.ToSafeDisplayString());
                try
                {
                    _throttler?.Wait();

                    object requestValue = request.Value(input, context, client);
                    if (!(requestValue is null))
                    {
                        results[request.Key] = requestValue;
                    }
                }
                catch (Exception ex)
                {
                    context.LogWarning("Exception while submitting {0} {1} request for {2}: {3}", request.Key, _clientName, input.ToSafeDisplayString(), ex.ToString());
                }
                finally
                {
                    if (_requestDelay > 0)
                    {
                        Task.WaitAll(Task.Delay((int)_requestDelay));
                    }

                    _throttler?.Release();
                }
            });

            return results.Count > 0 ? input.Clone(results).YieldAsync() : input.YieldAsync();
        }
    }
}
