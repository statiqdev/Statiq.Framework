using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using iTunesPodcastFinder;
using iTunesPodcastFinder.Models;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Itunes
{
    /// <summary>
    /// iTunes podcast metadata output keys.
    /// </summary>
    [Flags]
    public enum ItunesOutputKeys
    {
        /// <summary>
        /// <see cref="Podcast.ArtWork"/>.
        /// </summary>
        ArtWork = 1,

        /// <summary>
        /// <see cref="Podcast.EpisodesCount"/>.
        /// </summary>
        EpisodesCount = 2,

        /// <summary>
        /// <see cref="Podcast.ReleaseDate"/>.
        /// </summary>
        ReleaseDate = 4,

        /// <summary>
        /// <see cref="Podcast.Editor"/>.
        /// </summary>
        Editor = 8,

        /// <summary>
        /// <see cref="Podcast.FeedUrl"/>.
        /// </summary>
        FeedUrl = 16,

        /// <summary>
        /// <see cref="Podcast.Genre"/>.
        /// </summary>
        Genre = 32,

        /// <summary>
        /// <see cref="Podcast.ItunesLink"/>.
        /// </summary>
        ItunesLink = 64,

        /// <summary>
        /// <see cref="Podcast.InnerXml"/>.
        /// </summary>
        InnerXml = 128,

        /// <summary>
        /// <see cref="Podcast.Name"/>.
        /// </summary>
        Name = 256,

        /// <summary>
        /// <see cref="Podcast.Summary"/>.
        /// </summary>
        Summary = 512,

        /// <summary>
        /// <see cref="Podcast.FeedType"/>.
        /// </summary>
        FeedType = 1024,

        /// <summary>
        /// <see cref="Podcast.ItunesId"/>.
        /// </summary>
        ItunesId = 2048
    }

    /// <summary>
    /// iTunes podcast episode metadata output keys.
    /// </summary>
    [Flags]
    public enum ItunesEpisodeOutputKeys
    {
        /// <summary>
        /// <see cref="PodcastEpisode.Duration"/>.
        /// </summary>
        Duration = 1,

        /// <summary>
        /// <see cref="PodcastEpisode.Editor"/>.
        /// </summary>
        Editor = 2,

        /// <summary>
        /// <see cref="PodcastEpisode.EpisodeNumber"/>.
        /// </summary>
        EpisodeNumber = 4,

        /// <summary>
        /// <see cref="PodcastEpisode.FileUrl"/>.
        /// </summary>
        FileUrl = 8,

        /// <summary>
        /// <see cref="PodcastEpisode.InnerXml"/>.
        /// </summary>
        InnerXml = 16,

        /// <summary>
        /// <see cref="PodcastEpisode.PublishedDate"/>.
        /// </summary>
        PublishedDate = 32,

        /// <summary>
        /// <see cref="PodcastEpisode.Summary"/>.
        /// </summary>
        Summary = 64,

        /// <summary>
        /// <see cref="PodcastEpisode.Title"/>.
        /// </summary>
        Title = 128
    }

    /// <summary>
    /// Outputs metadata for information from iTunes.
    /// </summary>
    /// <remarks>
    /// This modules uses the Luandersonn.iTunesPodcastFinder library and associated types to submit requests to iTunes. Because
    /// of the large number of different kinds of requests, this module does not attempt to provide a fully abstract wrapper
    /// around the Luandersonn.iTunesPodcastFinder library. Instead, it simplifies the housekeeping involved in setting up an
    /// Luandersonn.iTunesPodcastFinder client and requires you to provide podcast id or Url for only one function that fetch
    /// podcast data.
    /// </remarks>
    /// <category>Metadata</category>
    public class ReadItunes : ParallelModule
    {
        private readonly PodcastFinder _itunes;

        private string _podcastId;

        private string _country;

        private bool _podcastToJson;

        private bool _episodesToJson;

        private ItunesOutputKeys _podcastOutputKeys;

        private ItunesEpisodeOutputKeys _episodesOutputKeys;

        static ReadItunes()
        {
            PodcastFinder.HttpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            });
        }

        /// <summary>
        /// Creates a connection to the iTunes API.
        /// </summary>
        public ReadItunes()
        {
            _itunes = new PodcastFinder();
            _podcastToJson = false;
            _episodesToJson = false;
        }

        /// <summary>
        /// Sets the podcast id.
        /// </summary>
        /// <param name="podcastId">iTunes podcast id.</param>
        /// <returns>The current instance.</returns>
        public ReadItunes WithPodcastId(string podcastId)
        {
            _podcastId = podcastId.ThrowIfNullOrWhiteSpace(nameof(podcastId));
            return this;
        }

        /// <summary>
        /// Sets the podcast country code.
        /// </summary>
        /// <remarks>
        /// The search uses the default store front for the specified country. For example: US. The default is US.
        /// See <see href="http://en.wikipedia.org/wiki/%20ISO_3166-1_alpha-2">here</see> for a list of ISO Country Codes.
        /// </remarks>
        /// <param name="country">The two-letter country code for the store you want to search.</param>
        /// <returns>The current instance.</returns>
        public ReadItunes ForCountryCode(string country)
        {
            _country = country.ThrowIfNullOrWhiteSpace(nameof(country));
            return this;
        }

        /// <summary>
        /// Sets the output metadata keys for iTunes podcast.
        /// </summary>
        /// <param name="podcastOutputKeys">Output metadata keys. <see cref="ItunesOutputKeys"/>.</param>
        /// <param name="toJson">Convert podcast data to json string.</param>
        /// <returns>The current instance.</returns>
        public ReadItunes WithPodcastOutputKeys(ItunesOutputKeys podcastOutputKeys, bool toJson = false)
        {
            _podcastOutputKeys = podcastOutputKeys;
            _podcastToJson = toJson;
            return this;
        }

        /// <summary>
        /// Sets the output metadata keys for iTunes podcast episodes.
        /// </summary>
        /// <param name="episodesOutputKeys">Output metadata keys. <see cref="ItunesEpisodeOutputKeys"/>.</param>
        /// <param name="toJson">Convert collection of episodes data to json string. Ignored if the same parameter setted in WithPodcastOutputKeys extension method.</param>
        /// <returns>The current instance.</returns>
        public ReadItunes WithEpisodesOutputKeys(ItunesEpisodeOutputKeys episodesOutputKeys, bool toJson = false)
        {
            _episodesOutputKeys = episodesOutputKeys;
            _episodesToJson = toJson;
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            if (input == null)
            {
                throw new ArgumentException("Argument is null", nameof(input));
            }

            ConcurrentDictionary<string, object> metadata = new ConcurrentDictionary<string, object>();
            await GetPodcastItunesDataAsync(input, context, metadata);
            return metadata.Count > 0 ? input.Clone(metadata).Yield() : input.Yield();
        }

        private async Task GetPodcastItunesDataAsync(IDocument input, IExecutionContext context, ConcurrentDictionary<string, object> metadata)
        {
            // Extract the Podcast id and country from itunes podcast Url stored in metadata with key "ItunesLink"
            if (input.ContainsKey(ItunesKeys.ItunesLink) && Uri.TryCreate(input.GetString(ItunesKeys.ItunesLink), UriKind.Absolute, out Uri itunesUrl)
                                                         && itunesUrl.Host.EndsWith("apple.com", StringComparison.OrdinalIgnoreCase)
                                                         && itunesUrl.Segments.Last().StartsWith("id", StringComparison.OrdinalIgnoreCase))
            {
                _podcastId ??= itunesUrl.Segments.Last().Trim('/').Replace("id", string.Empty);
                _country ??= itunesUrl.Query.Split('?', '&')
                    .FirstOrDefault(x => x.StartsWith("country=", StringComparison.OrdinalIgnoreCase))?
                    .Replace("country=", string.Empty);
            }
            else if (string.IsNullOrWhiteSpace(_podcastId))
            {
                context.LogWarning("Could not get podcast id from \"{1}\" metadata key", ItunesKeys.ItunesLink);
                return;
            }

            // Get the podcast data
            try
            {
                Podcast podcast = await _itunes.GetPodcastAsync(_podcastId.Replace("id", string.Empty), _country ?? "us");
                if (podcast != null)
                {
                    context.LogInformation("Getting podcast data with podcast id = {1}...", podcast.ItunesId);

                    // Set the metadata
                    if (_podcastToJson)
                    {
                        metadata.TryAdd(nameof(ItunesKeys.SerializedPodcastData), Newtonsoft.Json.JsonConvert.SerializeObject(await GetPodcastDataAsync(context, podcast)));
                    }
                    else
                    {
                        if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.ArtWork))
                        {
                            metadata.TryAdd(nameof(ItunesOutputKeys.ArtWork), podcast.ArtWork);
                        }
                        if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.EpisodesCount))
                        {
                            metadata.TryAdd(nameof(ItunesOutputKeys.EpisodesCount), podcast.EpisodesCount);
                        }
                        if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.ReleaseDate))
                        {
                            metadata.TryAdd(nameof(ItunesOutputKeys.ReleaseDate), podcast.ReleaseDate);
                        }
                        if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.Editor))
                        {
                            metadata.TryAdd(nameof(ItunesOutputKeys.Editor), podcast.Editor);
                        }
                        if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.FeedUrl))
                        {
                            metadata.TryAdd(nameof(ItunesOutputKeys.FeedUrl), podcast.FeedUrl);
                            if (!string.IsNullOrWhiteSpace(podcast.FeedUrl) && _episodesOutputKeys != 0)
                            {
                                PodcastRequestResult episodes = await _itunes.GetPodcastEpisodesAsync(podcast.FeedUrl);
                                if (episodes?.Episodes?.Any() == true)
                                {
                                    if (_episodesToJson)
                                    {
                                        metadata.TryAdd(nameof(ItunesKeys.Episodes), Newtonsoft.Json.JsonConvert.SerializeObject(GetPodcastEpisodesData(context, episodes.Episodes)));
                                    }
                                    else
                                    {
                                        metadata.TryAdd(nameof(ItunesKeys.Episodes), GetPodcastEpisodesData(context, episodes.Episodes));
                                    }
                                }
                                else
                                {
                                    context.LogWarning("Could not get podcast episodes data for {1} = {2}", nameof(ItunesOutputKeys.FeedUrl), podcast.FeedUrl);
                                }
                            }
                        }
                        else if (_episodesOutputKeys != 0)
                        {
                            context.LogWarning("Could not get podcast episodes data without setting \"{1}\" podcast output metadata key", nameof(ItunesOutputKeys.FeedUrl));
                        }
                        if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.Genre))
                        {
                            metadata.TryAdd(nameof(ItunesOutputKeys.Genre), podcast.Genre);
                        }
                        if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.ItunesLink))
                        {
                            metadata.TryAdd(nameof(ItunesOutputKeys.ItunesLink), podcast.ItunesLink);
                        }
                        if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.InnerXml))
                        {
                            metadata.TryAdd(nameof(ItunesOutputKeys.InnerXml), podcast.InnerXml);
                        }
                        if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.Name))
                        {
                            metadata.TryAdd(nameof(ItunesOutputKeys.Name), podcast.Name);
                        }
                        if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.Summary))
                        {
                            metadata.TryAdd(nameof(ItunesOutputKeys.Summary), podcast.Summary);
                        }
                        if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.FeedType))
                        {
                            metadata.TryAdd(nameof(ItunesOutputKeys.FeedType), podcast.FeedType);
                        }
                        if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.ItunesId))
                        {
                            metadata.TryAdd(nameof(ItunesOutputKeys.ItunesId), podcast.ItunesId);
                        }
                    }
                }
                else
                {
                    context.LogWarning("Could not get podcast data for id = {1}", _podcastId);
                }
            }
            catch (Exception ex)
            {
                context.LogError("Could not get podcast data for id = {1}: {2}", _podcastId, ex.Message);
            }
        }

        private async Task<dynamic> GetPodcastDataAsync(IExecutionContext context, Podcast podcast)
        {
            dynamic podcastData = new ExpandoObject();

            if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.ArtWork))
            {
                podcastData.ArtWork = podcast.ArtWork;
            }
            if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.EpisodesCount))
            {
                podcastData.EpisodesCount = podcast.EpisodesCount;
            }
            if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.ReleaseDate))
            {
                podcastData.ReleaseDate = podcast.ReleaseDate;
            }
            if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.Editor))
            {
                podcastData.Editor = podcast.Editor;
            }
            if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.FeedUrl))
            {
                podcastData.FeedUrl = podcast.FeedUrl;
                if (!string.IsNullOrWhiteSpace(podcast.FeedUrl) && _episodesOutputKeys != 0)
                {
                    PodcastRequestResult episodes = await _itunes.GetPodcastEpisodesAsync(podcast.FeedUrl);
                    if (episodes?.Episodes?.Any() == true)
                    {
                        podcastData.Episodes = GetPodcastEpisodesData(context, episodes.Episodes);
                    }
                    else
                    {
                        context.LogWarning("Could not get podcast episodes data for {1} = {2}", nameof(ItunesOutputKeys.FeedUrl), podcast.FeedUrl);
                    }
                }
            }
            else if (_episodesOutputKeys != 0)
            {
                context.LogWarning("Could not get podcast episodes data without setting \"{1}\" podcast output metadata key", nameof(ItunesOutputKeys.FeedUrl));
            }
            if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.Genre))
            {
                podcastData.Genre = podcast.Genre;
            }
            if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.ItunesLink))
            {
                podcastData.ItunesLink = podcast.ItunesLink;
            }
            if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.InnerXml))
            {
                podcastData.InnerXml = podcast.InnerXml;
            }
            if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.Name))
            {
                podcastData.Name = podcast.Name;
            }
            if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.Summary))
            {
                podcastData.Summary = podcast.Summary;
            }
            if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.FeedType))
            {
                podcastData.FeedType = podcast.FeedType;
            }
            if (_podcastOutputKeys == 0 || _podcastOutputKeys.HasFlag(ItunesOutputKeys.ItunesId))
            {
                podcastData.ItunesId = podcast.ItunesId;
            }

            return podcastData;
        }

        private IEnumerable<dynamic> GetPodcastEpisodesData(IExecutionContext context, IEnumerable<PodcastEpisode> episodes)
        {
            foreach (PodcastEpisode episode in episodes)
            {
                dynamic episodeData = new ExpandoObject();

                context.LogDebug("Getting podcast episode data with {1} = \"{2}\"...", nameof(episode.Title), episode.Title);

                if (_episodesOutputKeys.HasFlag(ItunesEpisodeOutputKeys.Editor))
                {
                    episodeData.Editor = episode.Editor;
                }
                if (_episodesOutputKeys.HasFlag(ItunesEpisodeOutputKeys.EpisodeNumber))
                {
                    episodeData.EpisodeNumber = episode.EpisodeNumber;
                }
                if (_episodesOutputKeys.HasFlag(ItunesEpisodeOutputKeys.FileUrl))
                {
                    episodeData.FileUrl = episode.FileUrl;
                }
                if (_episodesOutputKeys.HasFlag(ItunesEpisodeOutputKeys.InnerXml))
                {
                    episodeData.InnerXml = episode.InnerXml;
                }
                if (_episodesOutputKeys.HasFlag(ItunesEpisodeOutputKeys.PublishedDate))
                {
                    episodeData.PublishedDate = episode.PublishedDate;
                }
                if (_episodesOutputKeys.HasFlag(ItunesEpisodeOutputKeys.Summary))
                {
                    episodeData.Summary = episode.Summary;
                }
                if (_episodesOutputKeys.HasFlag(ItunesEpisodeOutputKeys.Title))
                {
                    episodeData.Title = episode.Title;
                }
                if (_episodesOutputKeys.HasFlag(ItunesEpisodeOutputKeys.Duration))
                {
                    episodeData.Duration = episode.Duration;
                }

                yield return episodeData;
            }
        }
    }
}
