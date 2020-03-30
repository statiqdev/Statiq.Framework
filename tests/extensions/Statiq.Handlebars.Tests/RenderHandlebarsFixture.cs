using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Handlebars.Tests
{
    [TestFixture]
    public class RenderHandlebarsFixture : BaseFixture
    {
        public class ExecuteTests : RenderHandlebarsFixture
        {
            [Test]
            public async Task RendersHandlebars()
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
                RenderHandlebars handlebars = new RenderHandlebars();

                // When
                TestDocument result = await ExecuteAsync(document, handlebars).SingleAsync();

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
                RenderHandlebars handlebars = new RenderHandlebars().WithModel(new
                {
                    title = "My New Post",
                    body = "This is my first post!"
                });

                // When
                TestDocument result = await ExecuteAsync(document, handlebars).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersHandlebarsFromMetadata()
            {
                // Given
                const string input = "{{title}}";
                const string output = "Hello World!";
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "meta", input },
                    { "title", "Hello World!" }
                });
                RenderHandlebars handlebars = new RenderHandlebars("meta");

                // When
                IDocument result = await ExecuteAsync(document, handlebars).SingleAsync();

                // Then
                result.GetString("meta").ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersHandlebarsFromMetadataToNewKey()
            {
                // Given
                const string input = "{{title}}";
                const string output = "Hello World!";
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "meta", input },
                    { "title", "Hello World!" }
                });
                RenderHandlebars handlebars = new RenderHandlebars("meta", "meta2");

                // When
                IDocument result = await ExecuteAsync(document, handlebars).SingleAsync();

                // Then
                result.GetString("meta2").ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNothingIfMetadataKeyDoesNotExist()
            {
                // Given
                TestDocument document = new TestDocument();
                RenderHandlebars handlebars = new RenderHandlebars("meta");

                // When
                TestDocument result = await ExecuteAsync(document, handlebars).SingleAsync();

                // Then
                result.ShouldBe(document);
            }

            [Test]
            public async Task RendersHandlebarsWithPartial()
            {
                // Given
                const string input = @"{{#names}}
  {{> user}}
{{/names}}";
                const string output = @"<strong>Karen</strong><strong>Jon</strong>";
                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        {
                            "names", new[]
                            {
                                new
                                {
                                    name = "Karen"
                                },
                                new
                                {
                                    name = "Jon"
                                }
                            }
                        }
                    },
                    input);
                RenderHandlebars handlebars = new RenderHandlebars()
                    .WithPartials(Config.FromValue(new Dictionary<string, string>
                    {
                        ["user"] = "<strong>{{name}}</strong>"
                    }.AsEnumerable()));

                // When
                TestDocument result = await ExecuteAsync(document, handlebars).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}