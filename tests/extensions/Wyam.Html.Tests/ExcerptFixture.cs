using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common;
using Wyam.Common.Documents;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Html.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class ExcerptFixture : BaseFixture
    {
        public class ExecuteTests : ExcerptFixture
        {
            [Test]
            public async Task ExcerptFirstParagraph()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                Excerpt excerpt = new Excerpt();

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ShouldBe("<p>This is some Foobar text</p>");
            }

            [Test]
            public async Task ExcerptAlternateQuerySelector()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <div>This is some other text</div>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                Excerpt excerpt = new Excerpt("div");

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ShouldBe("<div>This is some other text</div>");
            }

            [Test]
            public async Task ExcerptAlternateMetadataKey()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                Excerpt excerpt = new Excerpt().WithMetadataKey("Baz");

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Baz"].ShouldBe("<p>This is some Foobar text</p>");
            }

            [Test]
            public async Task ExcerptInnerHtml()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                Excerpt excerpt = new Excerpt().WithOuterHtml(false);

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ShouldBe("This is some Foobar text");
            }

            [Test]
            public async Task NoExcerptReturnsSameDocument()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <div>This is some Foobar text</div>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                Excerpt excerpt = new Excerpt("p");

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result.ShouldBe(document);
            }

            [Test]
            public async Task SeparatorInsideParagraph()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <!-- excerpt --> Foobar text</p>
                            <p>This is other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                Excerpt excerpt = new Excerpt();

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ShouldBe("<p>This is some </p>");
            }

            [Test]
            public async Task SeparatorBetweenParagraphs()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <p>This is some other text</p>
                            <!-- excerpt -->
                            <p>This is some more text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                Excerpt excerpt = new Excerpt();

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ToString().ShouldBe(
                    @"<p>This is some Foobar text</p>
                            <p>This is some other text</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task SeparatorInsideParagraphWithSiblings()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <p>This <b>is</b> some <!-- excerpt --><i>other</i> text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                Excerpt excerpt = new Excerpt();

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ToString().ShouldBe(
                    @"<p>This is some Foobar text</p>
                            <p>This <b>is</b> some </p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AlternateSeparatorComment()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <!-- foo --> Foobar text</p>
                            <p>This is other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                Excerpt excerpt = new Excerpt().WithSeparators(new[] { "foo" });

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ShouldBe("<p>This is some </p>");
            }

            [Test]
            public async Task MultipleSeparatorComments()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <!-- excerpt --> Foobar text</p>
                            <p>This is <!-- excerpt --> other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                Excerpt excerpt = new Excerpt();

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ShouldBe("<p>This is some </p>");
            }
        }
    }
}