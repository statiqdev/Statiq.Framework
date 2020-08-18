using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SpotifyAPI.Web;
using Statiq.Common;

namespace Statiq.Spotify
{
    /// <summary>
    /// Outputs metadata for information from Spotify.
    /// </summary>
    /// <remarks>
    /// This modules uses the SpotifyAPI-NET library and associated types to submit requests to Spotify. Because
    /// of the large number of different kinds of requests, this module does not attempt to provide a fully abstract wrapper
    /// around the SpotifyAPI-NET library. Instead, it simplifies the housekeeping involved in setting up an
    /// Spotify client and requires you to provide functions that fetch whatever data you need. Each request
    /// will be sent for each input document.
    /// </remarks>
    /// <category>Metadata</category>
    public class ReadSpotify : ParallelSyncModule
    {
        private readonly Dictionary<string, Func<IDocument, IExecutionContext, SpotifyClient, object>> _requests
            = new Dictionary<string, Func<IDocument, IExecutionContext, SpotifyClient, object>>();

        private SpotifyClient _spotify;

        /// <summary>
        /// Creates a connection to the Spotify API with authenticated access.
        /// </summary>
        /// <param name="token">The API token to use.</param>
        /// <param name="tokenType">The API token type (by default is "Bearer").</param>
        public ReadSpotify(string token, string tokenType = "Bearer")
        {
            _spotify = new SpotifyClient(token, tokenType);
        }

        /// <summary>
        /// Recreate a connection to the Spotify API with authenticated access with request token on demand.
        /// </summary>
        /// <param name="clientId">Spotify "CLIENT_ID" to use the API.</param>
        /// <param name="clientSecret">Spotify "CLIENT_SECRET" to use the API.</param>
        /// <returns>The current module instance.</returns>
        public ReadSpotify WithRequestTokenOnDemand(string clientId, string clientSecret)
        {
            clientId.ThrowIfNullOrEmpty(nameof(clientId));
            clientSecret.ThrowIfNullOrEmpty(nameof(clientSecret));

            SpotifyClientConfig config = SpotifyClientConfig
                .CreateDefault()
                .WithAuthenticator(new ClientCredentialsAuthenticator(clientId, clientSecret));

            _spotify = new SpotifyClient(config);
            return this;
        }

        /// <summary>
        /// Submits a request to the Spotify client. This allows you to incorporate data from the execution context in your request.
        /// </summary>
        /// <param name="key">The metadata key in which to store the return value of the request function.</param>
        /// <param name="request">A function with the request to make.</param>
        /// <returns>The current module instance.</returns>
        public ReadSpotify WithRequest(string key, Func<IExecutionContext, SpotifyClient, object> request)
        {
            key.ThrowIfNullOrEmpty(nameof(key));
            request.ThrowIfNull(nameof(request));

            _requests[key] = (doc, ctx, spotify) => request(ctx, spotify);
            return this;
        }

        /// <summary>
        /// Submits a request to the Spotify client. This allows you to incorporate data from the execution context and current document in your request.
        /// </summary>
        /// <param name="key">The metadata key in which to store the return value of the request function.</param>
        /// <param name="request">A function with the request to make.</param>
        /// <returns>The current module instance.</returns>
        public ReadSpotify WithRequest(string key, Func<IDocument, IExecutionContext, SpotifyClient, object> request)
        {
            key.ThrowIfNullOrEmpty(nameof(key));

            _requests[key] = request.ThrowIfNull(nameof(request));
            return this;
        }

        /// <inheritdoc/>
        protected override IEnumerable<IDocument> ExecuteInput(IDocument input, IExecutionContext context)
        {
            ConcurrentDictionary<string, object> results = new ConcurrentDictionary<string, object>();
            System.Threading.Tasks.Parallel.ForEach(_requests, request =>
            {
                context.LogDebug("Submitting {0} Spotify request for {1}", request.Key, input.ToSafeDisplayString());
                try
                {
                    results[request.Key] = request.Value(input, context, _spotify);
                }
                catch (Exception ex)
                {
                    context.LogWarning("Exception while submitting {0} Spotify request for {1}: {2}", request.Key, input.ToSafeDisplayString(), ex.ToString());
                }
            });
            return input.Clone(results).Yield();
        }
    }
}
