using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Scriban.Tests
{
    [TestFixture]
    public class RenderScribanFixture : BaseFixture
    {
        public class ExecuteTests : RenderScribanFixture
        {
            [Test]
            public async Task RendersScriban()
            {
                // Given
                const string input = @"<div class=""entry"">
  <h1>{{title}}</h1>
  <div class=""body"">
    {{body}}
  </div>
</div>";
                const string output = @"<div class=""entry"">
  <h1>My New Post</h1>
  <div class=""body"">
    This is my first post!
  </div>
</div>";
                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { Keys.Title, "My New Post" },
                        { "Body", "This is my first post!" }
                    },
                    input);
                RenderScriban scriban = new RenderScriban();

                // When
                TestDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AlternateModel()
            {
                // Given
                const string input = @"<div class=""entry"">
  <h1>{{title}}</h1>
  <div class=""body"">
    {{body}}
  </div>
</div>";
                const string output = @"<div class=""entry"">
  <h1>My New Post</h1>
  <div class=""body"">
    This is my first post!
  </div>
</div>";
                TestDocument document = new TestDocument(input);
                RenderScriban scriban = new RenderScriban().WithModel(new
                {
                    title = "My New Post",
                    body = "This is my first post!"
                });

                // When
                TestDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersScribanFromMetadata()
            {
                // Given
                const string input = "{{title}}";
                const string output = "Hello World!";
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "meta", input },
                    { "title", "Hello World!" }
                });
                RenderScriban scriban = new RenderScriban("meta");

                // When
                IDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.GetString("meta").ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersScribanFromMetadataToNewKey()
            {
                // Given
                const string input = "{{title}}";
                const string output = "Hello World!";
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "meta", input },
                    { "title", "Hello World!" }
                });
                RenderScriban scriban = new RenderScriban("meta", "meta2");

                // When
                IDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.GetString("meta2").ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNothingIfMetadataKeyDoesNotExist()
            {
                // Given
                TestDocument document = new TestDocument();
                RenderScriban scriban = new RenderScriban("meta");

                // When
                TestDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.ShouldBe(document);
            }
        }
    }
}