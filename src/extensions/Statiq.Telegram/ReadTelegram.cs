using System.Net.Http;
using Statiq.Common;
using Statiq.Core;
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
    public class ReadTelegram : ReadApi<TelegramBotClient>
    {
        /// <summary>
        /// Creates a connection to the Telegram API with authenticated access.
        /// </summary>
        /// <param name="accessToken">The API token to use.</param>
        /// <param name="httpClient">Http client.</param>
        public ReadTelegram(string accessToken, HttpClient httpClient = null)
            : base(
                Config.FromDocument(_ =>
            {
                accessToken.ThrowIfNullOrWhiteSpace(nameof(accessToken));

                return new TelegramBotClient(accessToken, httpClient);
            }), false,
                nameof(ReadTelegram))
        {
        }

        /// <summary>
        /// Creates a connection to the Telegram API with authenticated access using the client factory.
        /// </summary>
        /// <param name="clientFactory">The <see cref="TelegramBotClient"/> factory to use which will be called for each document.</param>
        public ReadTelegram(Config<TelegramBotClient> clientFactory)
            : base(clientFactory, false, nameof(ReadTelegram))
        {
        }
    }
}
