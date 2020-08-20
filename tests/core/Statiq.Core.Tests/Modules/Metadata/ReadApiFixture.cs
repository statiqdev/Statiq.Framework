using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
            /// Sets the metadata from the requests with throttling.
            /// </summary>
            [NonParallelizable]
            [TestCase(-1, 1, 1000u)]
            [TestCase(2, 1, 1000u)]
            [TestCase(1, 2, 1000u)]
            public async Task SetsMetadataWithThrottling(int maxDegreeOfParallelism, int requestLimit, uint requestDelay)
            {
                // Given
                double epsilon = 5d;
                HttpClient httpClient = new HttpClient();
                TestDocument document = new TestDocument();
                IModule client = new ReadApi<HttpClient>(httpClient)
                    .WithMaxDegreeOfParallelism(maxDegreeOfParallelism)
                    .WithRequestLimit(requestLimit)
                    .WithRequestDelay(requestDelay)
                    .WithRequest("Foo", (ctx, yt) =>
                    {
                        DateTime now = DateTime.Now;
                        ctx.LogDebug("Foo: DateTime now is {0}", now.ToString("HH:mm:ss.fff"));
                        return now;
                    })
                    .WithRequest("Bar", (doc, ctx, yt) =>
                    {
                        DateTime now = DateTime.Now;
                        ctx.LogDebug("Bar: DateTime now is {0}", now.ToString("HH:mm:ss.fff"));
                        return now;
                    });

                // When
                TestDocument result = await ExecuteAsync(document, client).SingleAsync();

                // Then
                DateTime fooDateTime = result["Foo"].ShouldBeOfType<DateTime>();
                DateTime barDateTime = result["Bar"].ShouldBeOfType<DateTime>();
                Math.Abs(fooDateTime.Subtract(barDateTime).TotalMilliseconds).ShouldBeGreaterThanOrEqualTo(requestDelay - epsilon);
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
