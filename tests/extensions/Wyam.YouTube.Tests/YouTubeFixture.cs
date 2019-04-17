using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Modules;
using Wyam.Common.Util;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.YouTube.Tests
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
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                IModule youtube = new YouTube("abcd")
                    .WithRequest("Foo", (ctx, yt) => 1)
                    .WithRequest("Bar", (doc, ctx, yt) => "baz");

                // When
                IList<IDocument> results = await youtube.ExecuteAsync(new[] { document }, context).ToListAsync(); // Make sure to materialize the result list

                // Then
                Assert.That(results.Single()["Foo"], Is.EqualTo(1));
                Assert.That(results.Single()["Bar"], Is.EqualTo("baz"));
            }
        }
    }
}