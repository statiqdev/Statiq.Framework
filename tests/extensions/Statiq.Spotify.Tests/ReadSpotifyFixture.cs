using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
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
            [Test]
            public async Task SetsMetadata()
            {
                // Given
                TestDocument document = new TestDocument();
                IModule spotify = new ReadSpotify("bearer_token")
                    .WithRequest("Foo", (ctx, yt) => 1)
                    .WithRequest("Bar", (doc, ctx, yt) => "baz");

                // When
                TestDocument result = await ExecuteAsync(document, spotify).SingleAsync();

                // Then
                result["Foo"].ShouldBe(1);
                result["Bar"].ShouldBe("baz");
            }

            /// <summary>
            /// Sets the metadata from the requests with request token on demand.
            /// </summary>
            [Test]
            public async Task SetsMetadataWithRequestTokenOnDemand()
            {
                // Given
                TestDocument document = new TestDocument();
                IModule spotify = new ReadSpotify(string.Empty)
                    .WithRequestTokenOnDemand("CLIENT_ID", "CLIENT_SECRET")
                    .WithRequest("Foo", (ctx, yt) => 1)
                    .WithRequest("Bar", (doc, ctx, yt) => "baz");

                // When
                TestDocument result = await ExecuteAsync(document, spotify).SingleAsync();

                // Then
                result["Foo"].ShouldBe(1);
                result["Bar"].ShouldBe("baz");
            }
        }
    }
}
