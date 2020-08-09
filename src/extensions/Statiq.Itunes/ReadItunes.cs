using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iTunesPodcastFinder;
using iTunesPodcastFinder.Models;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Itunes
{
    /// <summary>
    /// iTunes metadata output keys.
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
    /// Outputs metadata for information from iTunes.
    /// </summary>
    /// <category>Metadata</category>
    public class ReadItunes : ParallelModule
    {
        private readonly PodcastFinder _itunes;

        private string _podcastId;

        private string _country;

        private ItunesOutputKeys _outputKeys;

        /// <summary>
        /// Creates a connection to the iTunes API.
        /// </summary>
        public ReadItunes()
        {
            _itunes = new PodcastFinder();
        }

        /// <summary>
        /// Sets the podcast id.
        /// </summary>
        /// <param name="podcastId">iTunes podcast id.</param>
        /// <returns>The current instance.</returns>
        public ReadItunes WithPodcastId(string podcastId)
        {
            _podcastId = podcastId;
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
            _country = country;
            return this;
        }

        /// <summary>
        /// Sets the output metadata keys.
        /// </summary>
        /// <param name="outputKeys">Output metadata keys. <see cref="ItunesOutputKeys"/>.</param>
        /// <returns>The current instance.</returns>
        public ReadItunes WithOutputKeys(ItunesOutputKeys outputKeys)
        {
            _outputKeys = outputKeys;
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
            context.LogInformation($"Getting podcast information for {input.Source}...");

            // Extract the Podcast id and country from itunes podcast Url stored in metadata
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
                context.LogWarning($"Could not get podcast id from \"{ItunesKeys.ItunesLink}\" metadata key");
                return;
            }

            // Get the podcast data
            context.LogInformation($"Getting podcast data for id = {_podcastId}");
            try
            {
                Podcast podcast = await _itunes.GetPodcastAsync(_podcastId.Replace("id", string.Empty), _country ?? "us");
                if (podcast != null)
                {
                    // Set the metadata
                    if (_outputKeys == 0 || _outputKeys.HasFlag(ItunesOutputKeys.ArtWork))
                    {
                        metadata.TryAdd(nameof(podcast.ArtWork), podcast.ArtWork);
                    }
                    if (_outputKeys == 0 || _outputKeys.HasFlag(ItunesOutputKeys.EpisodesCount))
                    {
                        metadata.TryAdd(nameof(podcast.EpisodesCount), podcast.EpisodesCount);
                    }
                    if (_outputKeys == 0 || _outputKeys.HasFlag(ItunesOutputKeys.ReleaseDate))
                    {
                        metadata.TryAdd(nameof(podcast.ReleaseDate), podcast.ReleaseDate);
                    }
                    if (_outputKeys == 0 || _outputKeys.HasFlag(ItunesOutputKeys.Editor))
                    {
                        metadata.TryAdd(nameof(podcast.Editor), podcast.Editor);
                    }
                    if (_outputKeys == 0 || _outputKeys.HasFlag(ItunesOutputKeys.FeedUrl))
                    {
                        metadata.TryAdd(nameof(podcast.FeedUrl), podcast.FeedUrl);
                    }
                    if (_outputKeys == 0 || _outputKeys.HasFlag(ItunesOutputKeys.Genre))
                    {
                        metadata.TryAdd(nameof(podcast.Genre), podcast.Genre);
                    }
                    if (_outputKeys == 0 || _outputKeys.HasFlag(ItunesOutputKeys.ItunesLink))
                    {
                        metadata.TryAdd(nameof(podcast.ItunesLink), podcast.ItunesLink);
                    }
                    if (_outputKeys == 0 || _outputKeys.HasFlag(ItunesOutputKeys.InnerXml))
                    {
                        metadata.TryAdd(nameof(podcast.InnerXml), podcast.InnerXml);
                    }
                    if (_outputKeys == 0 || _outputKeys.HasFlag(ItunesOutputKeys.Name))
                    {
                        metadata.TryAdd(nameof(podcast.Name), podcast.Name);
                    }
                    if (_outputKeys == 0 || _outputKeys.HasFlag(ItunesOutputKeys.Summary))
                    {
                        metadata.TryAdd(nameof(podcast.Summary), podcast.Summary);
                    }
                    if (_outputKeys == 0 || _outputKeys.HasFlag(ItunesOutputKeys.FeedType))
                    {
                        metadata.TryAdd(nameof(podcast.FeedType), podcast.FeedType);
                    }
                    if (_outputKeys == 0 || _outputKeys.HasFlag(ItunesOutputKeys.ItunesId))
                    {
                        metadata.TryAdd(nameof(podcast.ItunesId), podcast.ItunesId);
                    }
                }
                else
                {
                    context.LogWarning($"Could not get podcast data for id = {_podcastId}");
                }
            }
            catch (Exception ex)
            {
                context.LogError($"Could not get podcast data for id = {_podcastId}: {ex.Message}");
            }
        }
    }
}
