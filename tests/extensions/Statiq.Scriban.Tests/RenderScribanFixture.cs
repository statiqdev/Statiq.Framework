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

            [Test]
            public async Task ShouldRenderBuiltInProperties()
            {
                // Given
                const string input = @"id: {{ id }}
count: {{ count }}
keys:
{{~ for key in keys ~}}
  {{ key }}
{{~ end ~}}
values:
{{~ for value in values ~}}
  {{ value }}
{{~ end ~}}
{{ content_provider.media_type }}";
                const string output = @"id: {0}
count: 3
keys:
  source
  destination
  content_provider
values:
  /input/index.html
  index.html
  Statiq.Common.MemoryContent
"; // TODO: Support nested complex objects

                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/index.html"),
                    new NormalizedPath("index.html"),
                    input);
                RenderScriban scriban = new RenderScriban();

                // When
                TestDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.Content.ShouldBe(string.Format(output, document.Id), StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldSetLocalVariable()
            {
                // Given
                const string input = @"{{ x = 5 ~}}
{{ x }}
{{ x + 1 }}";
                const string output = "5\n6";
                TestDocument document = new TestDocument(input);
                RenderScriban scriban = new RenderScriban();

                // When
                TestDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task IncludesTemplateFromFileSystem()
            {
                // Given
                const string include = @"{{ y = y + 1 ~}}
This is a string with the value {{ y }}";
                const string input = @"{{ y = 0 ~}}
{{include '../include/myinclude.html' }}
{{
x = include '../include/myinclude.html'
x + "" modified""
}}";
                const string output = @"This is a string with the value 1
This is a string with the value 2 modified";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/include");
                fileProvider.AddDirectory("/input/docs");
                fileProvider.AddFile("/input/include/myinclude.html", include);
                fileProvider.AddFile("/input/docs/test.html", input);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/docs/test.html"),
                    new NormalizedPath("docs/test.html"),
                    input);
                RenderScriban scriban = new RenderScriban();

                // When
                TestDocument result = await ExecuteAsync(document, context, scriban).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}