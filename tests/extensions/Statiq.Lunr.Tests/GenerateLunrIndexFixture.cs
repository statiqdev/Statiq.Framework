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
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""-1843551964"":{""link"":""/a/a.html"",""title"":""Foo""}}");
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
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
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
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
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
            public async Task ScriptContainsResultsFile()
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
                scriptDocument.Content.ShouldContain($@"resultsFile: '/{GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json")}'");
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
                        GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"),
                        "bar.gz"
                    },
                    true);
            }

            [Test]
            public async Task FilesUseCustomScriptPathByDefault()
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
                results.Select(x => x.Destination).ShouldBe(new NormalizedPath[] { "foo.js", "foo.json", "foo.gz" }, true);
            }

            [Test]
            public async Task FilesUseScriptPathByDefault()
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
                        GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".gz"),
                        GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json")
                    },
                    true);
            }

            [Test]
            public async Task ChangeResultsPath()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithResultsPath("bar.json");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                results.Select(x => x.Destination).ShouldBe(
                    new NormalizedPath[]
                    {
                        GenerateLunrIndex.DefaultScriptPath,
                        GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".gz"),
                        "bar.json"
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
                    .DefineField(fieldName, FieldType.Result);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
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
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""-1843551964"":{""link"":""/a/a.html""}}");
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
                results.ShouldNotHaveDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldNotContain("resultsFile");
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
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""-1843551964"":{""link"":""/a/a.html"",""title"":""Foo""},""-1722774156"":{""link"":""/b.html"",""title"":""Bar""}}");
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
                    .DefineField("aaa", FieldType.Result);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""-1843551964"":{""link"":""/a/a.html"",""title"":""Foo""},""-53465798"":{""aaa"":""bbb"",""link"":""/b.html"",""title"":""Bar""}}");
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
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldNotContain(@"""title"":""Bar""");
            }

            [TestCase((int)123, "\"123\"")]
            [TestCase(-123.45F, "\"-123.45\"")]
            [TestCase(true, "\"true\"")]
            [TestCase(new string[] { "123", "456" }, "[\"123\",\"456\"]")]
            public async Task ConvertsField(object value, string expected)
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    { "AAA", value }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .DefineField("aaa", FieldType.Result);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain($@"""aaa"":{expected}");
            }

            [Test]
            public async Task IncludeRefInFields()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .DefineField("ref", FieldType.Result);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"""ref"":""-1843551964""");
            }

            [Test]
            public async Task DocumentDefinesRef()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    { "ref", "abcd" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""abcd"":{");
            }

            [Test]
            public async Task AlternateRefKey()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    { "foo", "abcd" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithReferenceKey("foo");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""abcd"":{");
            }

            // When using the default search metadata delegate, the document cache key will be used as
            // the reference value if a document doesn't have a value for a custom reference key
            [Test]
            public async Task DefaultRefValueIfNoAlternateRefKey()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    { "fizz", "abcd" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"b.html", "Buzz")
                {
                    { Keys.Title, "Bar" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithReferenceKey("fizz");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""abcd"":{""link"":""/a/a.html"",""title"":""Foo""},""-1722774156"":{""link"":""/b.html"",""title"":""Bar""}}");
            }

            [Test]
            public async Task AlternateSearchItems()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithSearchItems(Config.FromDocument<IEnumerable<IEnumerable<KeyValuePair<string, object>>>>(doc => new[]
                    {
                        new Dictionary<string, object>
                        {
                            { "link", "a" },
                            { "title", doc.GetTitle() },
                            { "content", "c" },
                            { "ref", "d" }
                        },
                        new Dictionary<string, object>
                        {
                            { "link", "e" },
                            { "title", "f" },
                            { "content", "g" },
                            { "ref", "h" }
                        }
                    }));

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""d"":{""link"":""a"",""title"":""Foo""},""h"":{""link"":""e"",""title"":""f""}}");
            }

            [Test]
            public async Task AlternateSearchItemsUsesDocumentMetadata()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithSearchItems(Config.FromDocument<IEnumerable<IEnumerable<KeyValuePair<string, object>>>>(doc => new[]
                    {
                        new Dictionary<string, object>
                        {
                            { "link", "a" },
                            { "content", "c" },
                            { "ref", "d" }
                        }
                    }));

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""d"":{""link"":""a"",""title"":""Foo""}}");
            }

            [Test]
            public async Task AlternateSearchItemsMultipleDocuments()
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
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithSearchItems(Config.FromDocument<IEnumerable<IEnumerable<KeyValuePair<string, object>>>>(doc => new[]
                    {
                        new Dictionary<string, object>
                        {
                            { "link", "a" },
                            { "title", doc.GetTitle() },
                            { "content", "c" },
                            { "ref", doc.GetTitle() }
                        }
                    }));

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""Foo"":{""link"":""a"",""title"":""Foo""},""Bar"":{""link"":""a"",""title"":""Bar""}}");
            }

            [Test]
            public async Task AlternateSearchItemsSkipsNullItem()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithSearchItems(Config.FromDocument<IEnumerable<IEnumerable<KeyValuePair<string, object>>>>(doc => new[]
                    {
                        new Dictionary<string, object>
                        {
                            { "link", "a" },
                            { "title", doc.GetTitle() },
                            { "content", "c" },
                            { "ref", "d" }
                        },
                        null
                    }));

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""d"":{""link"":""a"",""title"":""Foo""}}");
            }

            [Test]
            public async Task AlternateSearchItemsWithoutRefKey()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithSearchItems(Config.FromDocument<IEnumerable<IEnumerable<KeyValuePair<string, object>>>>(doc => new[]
                    {
                        new Dictionary<string, object>
                        {
                            { "link", "a" },
                            { "title", doc.GetTitle() },
                            { "content", "c" },
                            { "ref", "d" }
                        },
                        new Dictionary<string, object>
                        {
                            { "link", "e" },
                            { "title", "f" },
                            { "content", "g" }
                        }
                    }));

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""d"":{""link"":""a"",""title"":""Foo""}");
            }

            [Test]
            public async Task AlternateSearchItemsAreCaseInsensitive()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithSearchItems(Config.FromDocument<IEnumerable<IEnumerable<KeyValuePair<string, object>>>>(doc => new[]
                    {
                        new Dictionary<string, object>
                        {
                            { "LINK", "a" },
                            { "TITLE", doc.GetTitle() },
                            { "CONTENT", "c" },
                            { "REF", "d" }
                        }
                    }));

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""d"":{""link"":""a"",""title"":""Foo""}");
            }

            [Test]
            public async Task MetadataSearchItems()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    {
                        LunrKeys.SearchItems,
                        new[]
                        {
                            new Dictionary<string, object>
                            {
                                { "link", "a" },
                                { "title", "b" },
                                { "content", "c" },
                                { "ref", "d" }
                            },
                            new Dictionary<string, object>
                            {
                                { "link", "e" },
                                { "title", "f" },
                                { "content", "g" },
                                { "ref", "h" }
                            }
                        }
                    }
                };
                GenerateLunrIndex module = new GenerateLunrIndex();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""d"":{""link"":""a"",""title"":""b""},""h"":{""link"":""e"",""title"":""f""}}");
            }

            [Test]
            public async Task MetadataSearchItemsUsesDocumentMetadata()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    {
                        LunrKeys.SearchItems,
                        new Dictionary<string, object>
                        {
                            { "link", "a" },
                            { "content", "c" },
                            { "ref", "d" }
                        }
                    }
                };
                GenerateLunrIndex module = new GenerateLunrIndex();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""d"":{""link"":""a"",""title"":""Foo""}}");
            }

            [Test]
            public async Task MetadataSearchItemsForOneDocument()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"b.html", "Buzz")
                {
                    { Keys.Title, "Bar" },
                    {
                        LunrKeys.SearchItems,
                        new Dictionary<string, object>
                        {
                            { "link", "a" },
                            { "title", "b" },
                            { "content", "c" },
                            { "ref", "d" }
                        }
                    }
                };
                GenerateLunrIndex module = new GenerateLunrIndex();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""-1843551964"":{""link"":""/a/a.html"",""title"":""Foo""},""d"":{""link"":""a"",""title"":""b""}}");
            }

            [Test]
            public async Task NullMetadataSearchItemsFallsBackToDocument()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    { LunrKeys.SearchItems, null }
                };
                GenerateLunrIndex module = new GenerateLunrIndex();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""184806200"":{""link"":""/a/a.html"",""title"":""Foo""}}");
            }

            // A ref value must be provided for custom search items
            [Test]
            public async Task MetadataSearchItemsWithoutRefKey()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    {
                        LunrKeys.SearchItems,
                        new Dictionary<string, object>
                        {
                            { "link", "e" },
                            { "title", "f" },
                            { "content", "g" }
                        }
                    }
                };
                GenerateLunrIndex module = new GenerateLunrIndex();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                results.ShouldNotHaveDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldNotContain("resultsFile");
            }

            [Test]
            public async Task MetadataSearchItemsAreCaseInsensitive()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    {
                        LunrKeys.SearchItems,
                        new MetadataItems
                        {
                            { "LINK", "a" },
                            { "TITLE", "b" },
                            { "CONTENT", "c" },
                            { "REF", "d" }
                        }
                    }
                };
                GenerateLunrIndex module = new GenerateLunrIndex();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""d"":{""link"":""a"",""title"":""b""}");
            }

            [Test]
            public async Task CustomMetadataSearchItemsKey()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    {
                        "Fizzbuzz",
                        new Dictionary<string, object>
                        {
                            { "link", "a" },
                            { "title", "b" },
                            { "content", "c" },
                            { "ref", "d" }
                        }
                    }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithSearchItemsKey("Fizzbuzz");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                scriptDocument.Content.ShouldContain(@"{""d"":{""link"":""a"",""title"":""b""}}");
            }

            [Test]
            public async Task EmptySearchIndex()
            {
                // Given
                GenerateLunrIndex module = new GenerateLunrIndex();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new TestDocument[] { }, module);

                // Then
                results.ShouldNotHaveDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".json"));
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldNotContain("resultsFile");
            }
        }
    }
}
