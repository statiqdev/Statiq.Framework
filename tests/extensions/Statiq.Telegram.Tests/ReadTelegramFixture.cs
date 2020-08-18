using System;
using System.Net;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Telegram.Tests
{
    /// <summary>
    /// Test fixture for <see cref="ReadTelegram"/>.
    /// </summary>
    [TestFixture]
    public class ReadTelegramFixture : BaseFixture
    {
        /// <summary>
        /// Tests for execute method.
        /// </summary>
        public class ExecuteTests : ReadTelegramFixture
        {
            /// <summary>
            /// Sets the metadata from the requests.
            /// </summary>
            [TestCase("1234567:4TT8bAc8GHUspu3ERYn-KGcvsvGB9u_n4ddy")]
            public async Task SetsMetadata(string accessToken)
            {
                // Given
                TestDocument document = new TestDocument();
                IModule telegram = new ReadTelegram(accessToken)
                    .WithRequest("Foo", (ctx, yt) => 1)
                    .WithRequest("Bar", (doc, ctx, yt) => "baz");

                // When
                TestDocument result = await ExecuteAsync(document, telegram).SingleAsync();

                // Then
                result["Foo"].ShouldBe(1);
                result["Bar"].ShouldBe("baz");
            }

            /// <summary>
            /// Sets the metadata from the requests with requests timeout.
            /// </summary>
            [TestCase("1234567:4TT8bAc8GHUspu3ERYn-KGcvsvGB9u_n4ddy")]
            public async Task SetsMetadataWithRequestsTimeout(string accessToken)
            {
                // Given
                TestDocument document = new TestDocument();
                IModule telegram = new ReadTelegram(accessToken)
                    .WithRequestsTimeout(new TimeSpan(0, 0, 1))
                    .WithRequest("Foo", (ctx, yt) => 1)
                    .WithRequest("Bar", (doc, ctx, yt) => "baz");

                // When
                TestDocument result = await ExecuteAsync(document, telegram).SingleAsync();

                // Then
                result["Foo"].ShouldBe(1);
                result["Bar"].ShouldBe("baz");
            }

            /// <summary>
            /// Sets the metadata from the requests with proxy.
            /// </summary>
            [TestCase("1234567:4TT8bAc8GHUspu3ERYn-KGcvsvGB9u_n4ddy", "https://example.org", 8080, "USERNAME", "PASSWORD")]
            public async Task SetsMetadataWithProxy(string accessToken, string proxyHost, int proxyPort, string proxyUserName, string proxyPassword)
            {
                // Given
                TestDocument document = new TestDocument();
                WebProxy proxy = new WebProxy(proxyHost, proxyPort)
                {
                    // Credentials if needed:
                    Credentials = new NetworkCredential(proxyUserName, proxyPassword)
                };
                IModule telegram = new ReadTelegram(accessToken, proxy)
                    .WithRequest("Foo", (ctx, yt) => 1)
                    .WithRequest("Bar", (doc, ctx, yt) => "baz");

                // When
                TestDocument result = await ExecuteAsync(document, telegram).SingleAsync();

                // Then
                result["Foo"].ShouldBe(1);
                result["Bar"].ShouldBe("baz");
            }
        }
    }
}
