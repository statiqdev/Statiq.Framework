using System.Threading.Tasks;
using iTunesPodcastFinder.Models;
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
                result[nameof(Podcast.ItunesLink)].ShouldBeOfType<string>().StartsWith(itunesUrl);
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
                result[nameof(Podcast.ItunesId)].ShouldBeOfType<string>().Equals(podcastId?.Replace("id", string.Empty));
            }

            /// <summary>
            /// Sets the metadata from the itunes podcast id with country code and output metadata keys.
            /// </summary>
            [TestCase("127976205", "us", ItunesOutputKeys.ArtWork | ItunesOutputKeys.ItunesId)]
            public async Task SetsMetadataWithOutputKeys(string podcastId, string country, ItunesOutputKeys outputKeys)
            {
                // Given
                TestDocument document = new TestDocument();
                IModule itunes = new ReadItunes()
                    .WithPodcastId(podcastId)
                    .ForCountryCode(country)
                    .WithOutputKeys(outputKeys);

                // When
                TestDocument result = await ExecuteAsync(document, itunes).SingleAsync();

                // Then
                result.Keys.ShouldContain(nameof(Podcast.ArtWork));
                result.Keys.ShouldContain(nameof(Podcast.ItunesId));
                result.Keys.ShouldNotContain(nameof(Podcast.Name));
                result[nameof(Podcast.ItunesId)].ShouldBeOfType<string>().Equals(podcastId?.Replace("id", string.Empty));
            }
        }
    }
}