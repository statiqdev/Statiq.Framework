using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Metadata
{
    /// <summary>
    /// Test fixture for <see cref="ReadApi{TClient}"/>.
    /// </summary>
    [TestFixture]
    public class ReadApiFixture : BaseFixture
    {
        /// <summary>
        /// Tests for execute method.
        /// </summary>
        public class ExecuteTests : ReadApiFixture
        {
            /// <summary>
            /// Sets the metadata from the requests with client instance.
            /// </summary>
            [TestCase(1)]
            [TestCase(-1)]
            public async Task SetsMetadataWithClientInstance(int maxDegreeOfParallelism)
            {
                // Given
                HttpClient httpClient = new HttpClient();
                TestDocument document = new TestDocument();
                IModule client = new ReadApi<HttpClient>(httpClient, "client_name")
                    .WithClientInitialization((doc, ctx, c) =>
                    {
                        c.DefaultRequestHeaders.Add("Authorization", "Bearer ...");
                    })
                    .WithMaxDegreeOfParallelism(maxDegreeOfParallelism)
                    .WithRequest("Foo", (ctx, yt) => 1)
                    .WithRequest("Bar", (doc, ctx, yt) => "baz");

                // When
                TestDocument result = await ExecuteAsync(document, client).SingleAsync();

                // Then
                result["Foo"].ShouldBe(1);
                result["Bar"].ShouldBe("baz");
            }

            /// <summary>
            /// Sets the metadata from the requests with client factory.
            /// </summary>
            [TestCase(1)]
            [TestCase(-1)]
            public async Task SetsMetadataWithClientFactory(int maxDegreeOfParallelism)
            {
                // Given
                HttpClient httpClient = new HttpClient();
                TestDocument document = new TestDocument();
                IModule client = new ReadApi<HttpClient>(() => new HttpClient(), "client_name")
                    .WithClientInitialization((doc, ctx, c) =>
                    {
                        c.DefaultRequestHeaders.Add("Authorization", "Bearer ...");
                    })
                    .WithMaxDegreeOfParallelism(maxDegreeOfParallelism)
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
