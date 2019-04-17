using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Util;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Json.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class GenerateJsonFixture : BaseFixture
    {
        private class Account
        {
            public string Email { get; set; }
            public bool Active { get; set; }
            public DateTime CreatedDate { get; set; }
            public IList<string> Roles { get; set; }
        }

        private static Account _jsonObject = new Account
        {
            Email = "james@example.com",
            Active = true,
            CreatedDate = new DateTime(2013, 1, 20, 0, 0, 0, DateTimeKind.Utc),
            Roles = new List<string>
            {
                "User",
                "Admin"
            }
        };

        private static string _jsonContent = @"{
  ""Email"": ""james@example.com"",
  ""Active"": true,
  ""CreatedDate"": ""2013-01-20T00:00:00Z"",
  ""Roles"": [
    ""User"",
    ""Admin""
  ]
}";

        private static string _camelCaseJsonContent = @"{
  ""email"": ""james@example.com"",
  ""active"": true,
  ""createdDate"": ""2013-01-20T00:00:00Z"",
  ""roles"": [
    ""User"",
    ""Admin""
  ]
}";

        public class ExecuteTests : GenerateJsonFixture
        {
            [Test]
            public async Task GetsObjectFromMetadata()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "JsonObject", _jsonObject }
                });
                GenerateJson generateJson = new GenerateJson("JsonObject");

                // When
                IList<IDocument> results = await generateJson.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.Content).Single().ShouldBe(_jsonContent, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task GetsObjectFromContextDelegate()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                GenerateJson generateJson = new GenerateJson((DocumentConfig)_jsonObject);

                // When
                IList<IDocument> results = await generateJson.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.Content).Single().ShouldBe(_jsonContent, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task GetsObjectFromDocumentDelegate()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "JsonObject", _jsonObject }
                });
                GenerateJson generateJson = new GenerateJson(Config.FromDocument(doc => doc.Get("JsonObject")));

                // When
                IList<IDocument> results = await generateJson.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.Content).Single().ShouldBe(_jsonContent, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task SetsMetadataKey()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "JsonObject", _jsonObject }
                });
                GenerateJson generateJson = new GenerateJson("JsonObject", "OutputKey");

                // When
                IList<IDocument> results = await generateJson.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.First(y => y.Key == "OutputKey").Value).Single().ToString()
                    .ShouldBe(_jsonContent, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotIndent()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "JsonObject", _jsonObject }
                });
                GenerateJson generateJson = new GenerateJson("JsonObject").WithIndenting(false);
                string nonIndentedJsonContent = _jsonContent
                    .Replace(" ", string.Empty)
                    .Replace("\r\n", string.Empty)
                    .Replace("\n", string.Empty);

                // When
                IList<IDocument> results = await generateJson.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.Content).Single().ShouldBe(nonIndentedJsonContent, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task GeneratesCamelCasePropertyNames()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "JsonObject", _jsonObject }
                });
                GenerateJson generateJson = new GenerateJson("JsonObject").WithCamelCase();

                // When
                IList<IDocument> results = await generateJson.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.Content).Single().ShouldBe(_camelCaseJsonContent, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task SerializesMetadataKeys()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "fuz" },
                    { "Bar", "baz" }
                });
                GenerateJson generateJson = new GenerateJson(new[] { "Bar" });

                // When
                IList<IDocument> results = await generateJson.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.Content).Single().ShouldBe(
                    @"{
  ""Bar"": ""baz""
}",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task SerializesMetadataKeysWithCamelCase()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "fuz" },
                    { "Bar", "baz" }
                });
                GenerateJson generateJson = new GenerateJson(new[] { "Bar" }).WithCamelCase();

                // When
                IList<IDocument> results = await generateJson.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.Content).Single().ShouldBe(
                    @"{
  ""bar"": ""baz""
}",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task SerializesMetadataObject()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "fuz" },
                    { "Bar", "baz" }
                });
                GenerateJson generateJson = new GenerateJson(Config.FromDocument(doc => (object)doc.GetMetadata("Bar")));

                // When
                IList<IDocument> results = await generateJson.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

            // Then
                results.Select(x => x.Content).Single().ShouldBe(
                @"{
  ""Bar"": ""baz""
}",
                StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task SerializesMetadataObjectWithCamelCase()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "fuz" },
                    { "Bar", "baz" }
                });
                GenerateJson generateJson = new GenerateJson(Config.FromDocument(doc => (object)doc.GetMetadata("Bar"))).WithCamelCase();

                // When
                IList<IDocument> results = await generateJson.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.Content).Single().ShouldBe(
                    @"{
  ""bar"": ""baz""
}",
                    StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}