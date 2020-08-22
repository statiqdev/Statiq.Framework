using SpotifyAPI.Web;
using Statiq.Common;
using Statiq.Core;

#nullable enable
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
    public class ReadSpotify : ReadApi<SpotifyClient>
    {
        /// <summary>
        /// Creates a connection to the Spotify API with authenticated access.
        /// </summary>
        /// <param name="token">The API token to use.</param>
        /// <param name="tokenType">The API token type ("Bearer" by default).</param>
        public ReadSpotify(string token, string tokenType = "Bearer")
            : base(
                Config.FromDocument(_ =>
                {
                    token.ThrowIfNullOrWhiteSpace(nameof(token));

                    return new SpotifyClient(token, tokenType);
                }), false,
                nameof(ReadSpotify))
        {
        }

        /// <summary>
        /// Creates a connection to the Spotify API with authenticated access with request token on demand.
        /// </summary>
        /// <param name="clientId">The ClientId, defined on a spotify application in your Spotify Developer Dashboard.</param>
        /// <param name="clientSecret">The ClientSecret, defined on a spotify application in your Spotify Developer Dashboard.</param>
        /// <param name="token">An optional initial token received earlier.</param>
        public ReadSpotify(string clientId, string clientSecret, ClientCredentialsTokenResponse? token)
            : base(
                Config.FromDocument(_ =>
                {
                    clientId.ThrowIfNullOrWhiteSpace(nameof(clientId));
                    clientSecret.ThrowIfNullOrWhiteSpace(nameof(clientSecret));

                    SpotifyClientConfig config = SpotifyClientConfig
                        .CreateDefault()
                        .WithAuthenticator(new ClientCredentialsAuthenticator(clientId, clientSecret, token));

                    return new SpotifyClient(config);
                }), false,
                nameof(ReadSpotify))
        {
        }

        /// <summary>
        /// Creates a connection to the Spotify API with authenticated access using the client factory.
        /// </summary>
        /// <param name="clientFactory">The <see cref="SpotifyClient"/> factory to use which will be called for each document.</param>
        public ReadSpotify(Config<SpotifyClient> clientFactory)
            : base(clientFactory, false, nameof(ReadSpotify))
        {
        }
    }
}
