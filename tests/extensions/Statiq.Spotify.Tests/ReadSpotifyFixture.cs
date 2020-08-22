using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using SpotifyAPI.Web;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Spotify.Tests
{
    /// <summary>
    /// Test fixture for <see cref="ReadSpotify"/>.
    /// </summary>
    [TestFixture]
    public class ReadSpotifyFixture : BaseFixture
    {
        /// <summary>
        /// Tests for execute method.
        /// </summary>
        public class ExecuteTests : ReadSpotifyFixture
        {
            /// <summary>
            /// Sets the metadata from the requests.
            /// </summary>
            [TestCase("bearer_token")]
            public async Task SetsMetadata(string accessToken)
            {
                // Given
                TestDocument document = new TestDocument();
                IModule spotify = new ReadSpotify(accessToken)
                    .WithRequest("Foo", (ctx, spf) => 1)
                    .WithRequest("Bar", (doc, ctx, spf) => "baz");

                // When
                TestDocument result = await ExecuteAsync(document, spotify).SingleAsync();

                // Then
                result["Foo"].ShouldBe(1);
                result["Bar"].ShouldBe("baz");
            }

            /// <summary>
            /// Sets the metadata from the requests with request token on demand.
            /// </summary>
            [TestCase("CLIENT_ID", "CLIENT_SECRET")]
            public async Task SetsMetadataWithRequestTokenOnDemand(string clientId, string clientSecret)
            {
                // Given
                TestDocument document = new TestDocument();
                IModule spotify = new ReadSpotify(clientId, clientSecret, null)
                    .WithRequest("Foo", (ctx, spf) => 1)
                    .WithRequest("Bar", (doc, ctx, spf) => "baz");

                // When
                TestDocument result = await ExecuteAsync(document, spotify).SingleAsync();

                // Then
                result["Foo"].ShouldBe(1);
                result["Bar"].ShouldBe("baz");
            }

            /// <summary>
            /// Sets the metadata from the requests with client factory.
            /// </summary>
            [TestCase("bearer_token")]
            public async Task SetsMetadataWithClientFactory(string accessToken)
            {
                // Given
                TestDocument document = new TestDocument();
                IModule spotify = new ReadSpotify(Config.FromDocument(_ => new SpotifyClient(accessToken)))
                    .WithRequest("Foo", (ctx, spf) => 1)
                    .WithRequest("Bar", (doc, ctx, spf) => "baz");

                // When
                TestDocument result = await ExecuteAsync(document, spotify).SingleAsync();

                // Then
                result["Foo"].ShouldBe(1);
                result["Bar"].ShouldBe("baz");
            }
        }
    }
}
