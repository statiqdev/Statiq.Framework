using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Lunr.Tests
{
    [TestFixture]
    public class GenerateLunrIndexFixture : BaseFixture
    {
        public class ExecuteTests : GenerateLunrIndexFixture
        {
            [Test]
            public async Task ScriptIncludesRefAsDocumentKey()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldContain(@"documents: {""-1843551964"":{""link"":""/a/a.html"",""title"":""Foo""}}");
            }

            [Test]
            public async Task DoesNotIncludeHostInLinkByDefault()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(Keys.Host, "fizzbuzz.com");
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, context, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldContain(@"""link"":""/a/a.html""");
            }

            [Test]
            public async Task IncludesHostInLink()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(Keys.Host, "fizzbuzz.com");
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .IncludeHostInLink();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, context, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldContain(@"""link"":""http://fizzbuzz.com/a/a.html""");
            }

            [Test]
            public async Task ScriptContainsIndexFile()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldContain($@"indexFile: '/{GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".gz")}'");
            }

            [Test]
            public async Task ChangeScriptPath()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithScriptPath("foo.js");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                results.ShouldHaveSingleWithDestination("foo.js");
            }

            [Test]
            public async Task IndexPathUsesCustomScriptPathByDefault()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithScriptPath("foo.js");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                results.Select(x => x.Destination).ShouldBe(new NormalizedPath[] { "foo.js", "foo.gz" }, true);
            }

            [Test]
            public async Task IndexPathUsesScriptPathByDefault()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                results.Select(x => x.Destination).ShouldBe(
                    new NormalizedPath[]
                    {
                        GenerateLunrIndex.DefaultScriptPath,
                        GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".gz")
                    },
                    true);
            }

            [Test]
            public async Task ChangeIndexPath()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithIndexPath("bar.gz");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                results.Select(x => x.Destination).ShouldBe(
                    new NormalizedPath[]
                    {
                        GenerateLunrIndex.DefaultScriptPath,
                        "bar.gz"
                    },
                    true);
            }

            [Test]
            public async Task CanCustomizeScript()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .CustomizeScript((_, __) => "foobar");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldBe("foobar");
            }

            [TestCase((string)null)]
            [TestCase("")]
            public async Task DoesNotOutputScriptForNullOrEmptyCustomize(string script)
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .CustomizeScript((_, __) => script);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                results.ShouldNotHaveDestination(GenerateLunrIndex.DefaultScriptPath);
            }

            [TestCase("AAA", "aAA")]
            [TestCase("Aaa", "aaa")]
            [TestCase("aaa", "aaa")]
            [TestCase("aAA", "aAA")]
            public async Task DefineAdditionalFieldAsCamelCase(string fieldName, string expectedName)
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    { "AAA", "bbb" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .DefineField(fieldName, FieldType.EagerLoad);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldContain($@"""{expectedName}"":""bbb""");
            }

            [TestCase("Title")]
            [TestCase("title")]
            public async Task RemovesField(string fieldName)
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .RemoveField(fieldName);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldContain(@"documents: {""-1843551964"":{""link"":""/a/a.html""}}");
            }

            [Test]
            public async Task ClearsFields()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .ClearFields();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldContain("documents: {}");
            }

            [Test]
            public async Task ScriptDocumentsHasMultipleItems()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"b.html", "Buzz")
                {
                    { Keys.Title, "Bar" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldContain(@"documents: {""-1843551964"":{""link"":""/a/a.html"",""title"":""Foo""},""-1722774156"":{""link"":""/b.html"",""title"":""Bar""}}");
            }

            [Test]
            public async Task AdditionalFieldsIncludedOnlyWhenAvailable()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"b.html", "Buzz")
                {
                    { Keys.Title, "Bar" },
                    { "AAA", "bbb" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .DefineField("aaa", FieldType.EagerLoad);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldContain(@"documents: {""-1843551964"":{""link"":""/a/a.html"",""title"":""Foo""},""-53465798"":{""aaa"":""bbb"",""link"":""/b.html"",""title"":""Bar""}}");
            }

            [Test]
            public async Task OmitsFromSearch()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"b.html", "Buzz")
                {
                    { Keys.Title, "Bar" },
                    { LunrKeys.OmitFromSearch, true }
                };
                GenerateLunrIndex module = new GenerateLunrIndex();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldNotContain(@"""title"":""Bar""");
            }
        }
    }
}
