using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Meta;
using Statiq.Testing;
using Statiq.Testing.Documents;
using Statiq.Testing.Execution;

namespace Statiq.Html.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class HeadingsFixture : BaseFixture
    {
        public class ExecuteTests : HeadingsFixture
        {
            [Test]
            public async Task SetsHeadingContent()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h1>Bar</h1>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                Headings headings = new Headings();

                // When
                TestDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.DocumentList(HtmlKeys.Headings).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Foo", "Bar" });
            }

            [Test]
            public async Task SetsHeadingMetadata()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h1>Bar</h1>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                Headings headings = new Headings().WithHeadingKey("HContent");

                // When
                TestDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.DocumentList(HtmlKeys.Headings).Select(x => x.String("HContent")).ShouldBe(new[] { "Foo", "Bar" });
            }

            [Test]
            public async Task DoesNotSetHeadingMetadataIfNull()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h1>Bar</h1>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                Headings headings = new Headings();

                // When
                TestDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.DocumentList(HtmlKeys.Headings).Select(x => x.String("HContent")).ShouldBe(new string[] { null, null });
            }

            [Test]
            public async Task OnlyGetsFirstLevelByDefault()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h2>Baz</h2>
                            <h1>Bar</h1>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                Headings headings = new Headings();

                // When
                TestDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.DocumentList(HtmlKeys.Headings).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Foo", "Bar" });
            }

            [Test]
            public async Task GetsDeeperLevels()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h2>Baz</h2>
                            <h1>Bar</h1>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                Headings headings = new Headings(3);

                // When
                TestDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.DocumentList(HtmlKeys.Headings).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Foo", "Baz", "Bar" });
            }

            [Test]
            public async Task Nesting()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h2>Baz</h2>
                            <h2>Boz</h2>
                            <h1>Bar</h1>
                            <h2>Boo</h2>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                Headings headings = new Headings(3).WithNesting();

                // When
                TestDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.DocumentList(HtmlKeys.Headings).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Foo", "Bar" });
                result.DocumentList(HtmlKeys.Headings)[0].DocumentList(Keys.Children).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Baz", "Boz" });
                result.DocumentList(HtmlKeys.Headings)[1].DocumentList(Keys.Children).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Boo" });
            }

            [Test]
            public async Task SetsChildrenWhenNotNesting()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h2>Baz</h2>
                            <h2>Boz</h2>
                            <h1>Bar</h1>
                            <h2>Boo</h2>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                Headings headings = new Headings(3);

                // When
                TestDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.DocumentList(HtmlKeys.Headings).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Foo", "Baz", "Boz", "Bar", "Boo" });
                result.DocumentList(HtmlKeys.Headings)[0].DocumentList(Keys.Children).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Baz", "Boz" });
                result.DocumentList(HtmlKeys.Headings)[3].DocumentList(Keys.Children).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Boo" });
            }

            [Test]
            public async Task SetsHeadingIdAttribute()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h1 id=""bar"">Bar</h1>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                Headings headings = new Headings();

                // When
                TestDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.DocumentList(HtmlKeys.Headings).Select(x => x.String(HtmlKeys.HeadingId)).ShouldBe(new[] { null, "bar" });
            }
        }
    }
}