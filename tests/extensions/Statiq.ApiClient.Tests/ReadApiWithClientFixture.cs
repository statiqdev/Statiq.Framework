using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.ApiClient.Tests
{
    /// <summary>
    /// Test fixture for <see cref="ReadApiWithClient{TClient}"/>.
    /// </summary>
    [TestFixture]
    public class ReadApiWithClientFixture : BaseFixture
    {
        /// <summary>
        /// Tests for execute method.
        /// </summary>
        public class ExecuteTests : ReadApiWithClientFixture
        {
            /// <summary>
            /// Sets the metadata from the requests.
            /// </summary>
            [Test]
            public async Task SetsMetadata()
            {
                // Given
                HttpClient httpClient = new HttpClient();
                TestDocument document = new TestDocument();
                IModule client = new ReadApiWithClient<HttpClient>(httpClient)
                    .WithClientInitialization(c =>
                    {
                        c.DefaultRequestHeaders.Add("Authorization", "Bearer ...");
                    })
                    .WithClientName(nameof(HttpClient))
                    .WithRequest("Foo", (ctx, yt) => 1)
                    .WithRequest("Bar", (doc, ctx, yt) => "baz");

                // When
                TestDocument result = await ExecuteAsync(document, client).SingleAsync();

                // Then
                result["Foo"].ShouldBe(1);
                result["Bar"].ShouldBe("baz");
            }
        }
    }
}
