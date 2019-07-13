using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Testing.Documents;

namespace Statiq.YouTube.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class YouTubeFixture : BaseFixture
    {
        public class ExecuteTests : YouTubeFixture
        {
            [Test]
            public async Task SetsMetadata()
            {
                // Given
                TestDocument document = new TestDocument();
                IModule youtube = new YouTube("abcd")
                    .WithRequest("Foo", (ctx, yt) => 1)
                    .WithRequest("Bar", (doc, ctx, yt) => "baz");

                // When
                TestDocument result = await ExecuteAsync(document, youtube).SingleAsync();

                // Then
                result["Foo"].ShouldBe(1);
                result["Bar"].ShouldBe("baz");
            }
        }
    }
}