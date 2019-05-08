using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Util;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Html.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class HtmlQueryFixture : BaseFixture
    {
        public class ExecuteTests : HtmlQueryFixture
        {
            [Test]
            public async Task GetOuterHtml()
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
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .GetOuterHtml("Key");

                // When
                IList<IDocument> results = await query.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Select(x => x["Key"].ToString()).ShouldBe(new[]
                {
                    "<p>This is some Foobar text</p>",
                    "<p>This is some other text</p>"
                });
            }

            [Test]
            public async Task GetOuterHtmlWithAttributes()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p foo=""bar"">This is some Foobar text</p>
                            <p foo=""baz"" foo=""bat"" a=""A"">This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .GetOuterHtml("Key");

                // When
                IList<IDocument> results = await query.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Select(x => x["Key"].ToString()).ShouldBe(new[]
                {
                    @"<p foo=""bar"">This is some Foobar text</p>",
                    @"<p foo=""baz"" a=""A"">This is some other text</p>"
                });
            }

            [Test]
            public async Task GetOuterHtmlForFirst()
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
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .GetOuterHtml("Key")
                    .First();

                // When
                IList<IDocument> results = await query.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Select(x => x["Key"].ToString()).ShouldBe(new[]
                {
                    "<p>This is some Foobar text</p>"
                });
            }

            [Test]
            public async Task GetInnerHtml()
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
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .GetInnerHtml("Key");

                // When
                IList<IDocument> results = await query.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Select(x => x["Key"].ToString()).ShouldBe(new[]
                {
                    "This is some Foobar text",
                    "This is some other text"
                });
            }

            [Test]
            public async Task GetInnerHtmlAndOuterHtml()
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
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .GetInnerHtml("InnerHtmlKey")
                    .GetOuterHtml("OuterHtmlKey");

                // When
                IList<IDocument> results = await query.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Select(x => x["InnerHtmlKey"].ToString()).ShouldBe(new[]
                {
                    "This is some Foobar text",
                    "This is some other text"
                });
                results.Select(x => x["OuterHtmlKey"].ToString()).ShouldBe(new[]
                {
                    "<p>This is some Foobar text</p>",
                    "<p>This is some other text</p>"
                });
            }

            [Test]
            public async Task SetOuterHtmlContent()
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
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .SetContent();

                // When
                List<IDocument> results = await query.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                (await results.SelectAsync(async x => await x.GetStringAsync())).ShouldBe(new[]
                {
                    "<p>This is some Foobar text</p>",
                    "<p>This is some other text</p>"
                });
            }

            [Test]
            public async Task SetInnerHtmlContent()
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
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .SetContent(false);

                // When
                List<IDocument> results = await query.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                (await results.SelectAsync(async x => await x.GetStringAsync())).ShouldBe(new[]
                {
                    "This is some Foobar text",
                    "This is some other text"
                });
            }

            [Test]
            public async Task SetOuterHtmlContentWithMetadata()
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
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .SetContent()
                    .GetInnerHtml("InnerHtmlKey")
                    .GetOuterHtml("OuterHtmlKey");

                // When
                List<IDocument> results = await query.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                (await results.SelectAsync(async x => await x.GetStringAsync())).ShouldBe(new[]
                {
                    "<p>This is some Foobar text</p>",
                    "<p>This is some other text</p>"
                });
                results.Select(x => x.String("InnerHtmlKey")).ShouldBe(new[]
                {
                    "This is some Foobar text",
                    "This is some other text"
                });
                results.Select(x => x.String("OuterHtmlKey")).ShouldBe(new[]
                {
                    "<p>This is some Foobar text</p>",
                    "<p>This is some other text</p>"
                });
            }

            [Test]
            public async Task GetTextContent()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <b>Foobar</b> text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .GetTextContent("TextContentKey");

                // When
                IList<IDocument> results = await query.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.String("TextContentKey")).ShouldBe(new[]
                {
                    "This is some Foobar text",
                    "This is some other text"
                });
            }

            [Test]
            public async Task GetAttributeValue()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p foo=""bar"">This is some <b>Foobar</b> text</p>
                            <p foo=""baz"">This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .GetAttributeValue("foo", "Foo");

                // When
                IList<IDocument> results = await query.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.String("Foo")).ShouldBe(new[]
                {
                    "bar",
                    "baz"
                });
            }

            [Test]
            public async Task GetAttributeValueWithImplicitKey()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p foo=""bar"">This is some <b>Foobar</b> text</p>
                            <p foo=""baz"">This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .GetAttributeValue("foo");

                // When
                IList<IDocument> results = await query.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.String("foo")).ShouldBe(new[]
                {
                    "bar",
                    "baz"
                });
            }

            [Test]
            public async Task GetAttributeValueWithMoreThanOneMatch()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p foo=""bar"" foo=""bat"">This is some <b>Foobar</b> text</p>
                            <p foo=""baz"">This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .GetAttributeValue("foo");

                // When
                IList<IDocument> results = await query.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Select(x => x.String("foo")).ShouldBe(new[]
                {
                    "bar",
                    "baz"
                });
            }

            [Test]
            public async Task GetAttributeValues()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p foo=""bar"" foo=""bat"" a=""A"" b=""B"">This is some <b>Foobar</b> text</p>
                            <p foo=""baz"" x=""X"">This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .GetAttributeValues();

                // When
                IList<IDocument> results = await query.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results
                    .Select(x => x.OrderBy(y => y.Key, StringComparer.OrdinalIgnoreCase))
                    .Cast<IEnumerable<KeyValuePair<string, object>>>()
                    .ShouldBe(new[]
                    {
                        new List<KeyValuePair<string, object>>
                        {
                            new KeyValuePair<string, object>("a", "A"),
                            new KeyValuePair<string, object>("b", "B"),
                            new KeyValuePair<string, object>("foo", "bar")
                        },
                        new List<KeyValuePair<string, object>>
                        {
                            new KeyValuePair<string, object>("foo", "baz"),
                            new KeyValuePair<string, object>("x", "X")
                        }
                    });
            }

            [Test]
            public async Task GetAll()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p foo=""bar"" foo=""bat"" a=""A"" b=""B"">This is some <b>Foobar</b> text</p>
                            <p foo=""baz"" x=""X"">This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext();
                HtmlQuery query = new HtmlQuery("p")
                    .GetAll();

                // When
                IList<IDocument> results = await query.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results
                    .Select(x => x.OrderBy(y => y.Key, StringComparer.OrdinalIgnoreCase))
                    .Cast<IEnumerable<KeyValuePair<string, object>>>()
                    .ShouldBe(new[]
                    {
                        new List<KeyValuePair<string, object>>
                        {
                            new KeyValuePair<string, object>("a", "A"),
                            new KeyValuePair<string, object>("b", "B"),
                            new KeyValuePair<string, object>("foo", "bar"),
                            new KeyValuePair<string, object>("InnerHtml", "This is some <b>Foobar</b> text"),
                            new KeyValuePair<string, object>("OuterHtml", @"<p foo=""bar"" a=""A"" b=""B"">This is some <b>Foobar</b> text</p>"),
                            new KeyValuePair<string, object>("TextContent", "This is some Foobar text")
                        },
                        new List<KeyValuePair<string, object>>
                        {
                            new KeyValuePair<string, object>("foo", "baz"),
                            new KeyValuePair<string, object>("InnerHtml", "This is some other text"),
                            new KeyValuePair<string, object>("OuterHtml", @"<p foo=""baz"" x=""X"">This is some other text</p>"),
                            new KeyValuePair<string, object>("TextContent", "This is some other text"),
                            new KeyValuePair<string, object>("x", "X")
                        }
                    });
            }
        }
    }
}