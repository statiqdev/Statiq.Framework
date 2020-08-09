using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Itunes.Tests
{
    /// <summary>
    /// Test fixture for <see cref="ReadItunes"/>.
    /// </summary>
    [TestFixture]
    public class ReadItunesFixture : BaseFixture
    {
        /// <summary>
        /// Tests for execute method.
        /// </summary>
        public class ExecuteTests : ReadItunesFixture
        {
            /// <summary>
            /// Sets the metadata from the itunes podcast Url.
            /// </summary>
            [TestCase("https://podcasts.apple.com/ru/podcast/net-rocks/id127976205")]
            [TestCase("https://podcasts.apple.com/ru/podcast/net-rocks/id127976205?country=ru")]
            public async Task SetsMetadataWithItunesUrl(string itunesUrl)
            {
                // Given
                TestDocument document = new TestDocument();
                document.TestMetadata.Add(ItunesKeys.ItunesLink, itunesUrl);
                IModule itunes = new ReadItunes();

                // When
                TestDocument result = await ExecuteAsync(document, itunes).SingleAsync();

                // Then
                result[nameof(ItunesOutputKeys.ItunesLink)].ShouldBeOfType<string>().StartsWith(itunesUrl);
            }

            /// <summary>
            /// Sets the metadata from the itunes podcast id with country code.
            /// </summary>
            [TestCase("127976205", "us")]
            [TestCase("id127976205", "ru")]
            public async Task SetsMetadataWithPodcastIdAndCountry(string podcastId, string country)
            {
                // Given
                TestDocument document = new TestDocument();
                IModule itunes = new ReadItunes()
                    .WithPodcastId(podcastId)
                    .ForCountryCode(country);

                // When
                TestDocument result = await ExecuteAsync(document, itunes).SingleAsync();

                // Then
                result[nameof(ItunesOutputKeys.ItunesId)].ShouldBeOfType<string>().Equals(podcastId?.Replace("id", string.Empty));
            }

            /// <summary>
            /// Sets the metadata from the itunes podcast id with country code and output metadata keys.
            /// </summary>
            [TestCase("127976205", "us", ItunesOutputKeys.ArtWork | ItunesOutputKeys.ItunesId)]
            public async Task SetsMetadataWithPodcastOutputKeys(string podcastId, string country, ItunesOutputKeys outputKeys)
            {
                // Given
                TestDocument document = new TestDocument();
                IModule itunes = new ReadItunes()
                    .WithPodcastId(podcastId)
                    .ForCountryCode(country)
                    .WithPodcastOutputKeys(outputKeys);

                // When
                TestDocument result = await ExecuteAsync(document, itunes).SingleAsync();

                // Then
                result.Keys.ShouldContain(nameof(ItunesOutputKeys.ArtWork));
                result.Keys.ShouldContain(nameof(ItunesOutputKeys.ItunesId));
                result.Keys.ShouldNotContain(nameof(ItunesOutputKeys.Name));
                result[nameof(ItunesOutputKeys.ItunesId)].ShouldBeOfType<string>().Equals(podcastId?.Replace("id", string.Empty));
            }

            /// <summary>
            /// Sets the metadata from the itunes podcast with serialized episodes data.
            /// </summary>
            [TestCase("127976205", "us", ItunesOutputKeys.FeedUrl | ItunesOutputKeys.ItunesId, ItunesEpisodeOutputKeys.Editor | ItunesEpisodeOutputKeys.FileUrl)]
            public async Task SetsMetadataWithEpisodesConvertedToJson(string podcastId, string country, ItunesOutputKeys outputKeys, ItunesEpisodeOutputKeys episodesOutputKeys)
            {
                // Given
                TestDocument document = new TestDocument();
                IModule itunes = new ReadItunes()
                    .WithPodcastId(podcastId)
                    .ForCountryCode(country)
                    .WithPodcastOutputKeys(outputKeys)
                    .WithEpisodesOutputKeys(episodesOutputKeys, true);

                // When
                TestDocument result = await ExecuteAsync(document, itunes).SingleAsync();

                // Then
                result.Keys.ShouldContain(nameof(ItunesOutputKeys.ItunesId));
                result.Keys.ShouldNotContain(nameof(ItunesOutputKeys.Name));
                result.Keys.ShouldContain(nameof(ItunesKeys.Episodes));
                result[nameof(ItunesOutputKeys.ItunesId)].ShouldBeOfType<string>().Equals(podcastId?.Replace("id", string.Empty));
                result[nameof(ItunesKeys.Episodes)].ShouldNotBeNull();
                JToken.Parse(result[nameof(ItunesKeys.Episodes)].ShouldBeOfType<string>());
            }

            /// <summary>
            /// Sets the metadata from the itunes podcast serialized to Json format.
            /// </summary>
            [TestCase("127976205", "us", ItunesOutputKeys.FeedUrl | ItunesOutputKeys.ItunesId, ItunesEpisodeOutputKeys.Editor | ItunesEpisodeOutputKeys.FileUrl)]
            public async Task SetsMetadataForPodcastConvertedToJson(string podcastId, string country, ItunesOutputKeys outputKeys, ItunesEpisodeOutputKeys episodesOutputKeys)
            {
                // Given
                TestDocument document = new TestDocument();
                IModule itunes = new ReadItunes()
                    .WithPodcastId(podcastId)
                    .ForCountryCode(country)
                    .WithPodcastOutputKeys(outputKeys, true)
                    .WithEpisodesOutputKeys(episodesOutputKeys);

                // When
                TestDocument result = await ExecuteAsync(document, itunes).SingleAsync();

                // Then
                result.Keys.ShouldContain(nameof(ItunesKeys.SerializedPodcastData));
                result[nameof(ItunesKeys.SerializedPodcastData)].ShouldNotBeNull();
                JToken.Parse(result[nameof(ItunesKeys.SerializedPodcastData)].ShouldBeOfType<string>());
            }
        }
    }
}