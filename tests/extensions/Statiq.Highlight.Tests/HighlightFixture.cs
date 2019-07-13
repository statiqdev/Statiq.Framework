using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Testing.JavaScript;
using TestExecutionContext = Statiq.Testing.TestExecutionContext;

namespace Statiq.Highlight.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class HighlightFixture : BaseFixture
    {
        public class ExecuteTests : HighlightFixture
        {
            [Test]
            public async Task CanHighlightCSharp()
            {
                // Given
                const string input = @"
<html>
<head>
    <title>Foobar</title>
</head>
<body>
    <h1>Title</h1>
    <p>This is some Foobar text</p>
    <pre><code class=""language-csharp"">
    class Program
    {
        static void Main(string[] args)
        {
            var invoices = new List&lt;Invoice&gt; { new Invoice { InvoiceId = 0 } };
            var oneTimeCharges = new List&lt;OneTimeCharge&gt; { new OneTimeCharge { Invoice = 0, OneTimeChargeId = 0 } };
            var otcCharges = invoices.Join(oneTimeCharges, inv =&gt; inv.InvoiceId, otc =&gt; otc.Invoice, (inv, otc) =&gt; inv.InvoiceId);
            Console.WriteLine(otcCharges.Count());
        }        
    }

    public class OneTimeCharge
    {
        public int OneTimeChargeId { get; set; }
        public int? Invoice { get; set; }
    }

    public class Invoice
    {
        public int InvoiceId { get; set; }
    }
    </code></pre>
</body>
</html>";

                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext()
                {
                    JsEngineFunc = () => new TestJsEngine()
                };

                Highlight highlight = new Highlight();

                // When
                TestDocument result = await ExecuteAsync(document, context, highlight).SingleAsync();

                // Then
                result.Content.ShouldContain("language-csharp hljs");
            }

            [Test]
            public async Task CanHighlightHtml()
            {
                const string input = @"
<html>
<head>
    <title>Foobar</title>
</head>
<body>
    <h1>Title</h1>
    <p>This is some Foobar text</p>
    <pre><code class=""language-html"">
    <html>
    <head>
    <title>Hi Mom!</title>
    </head>
    <body>
        <p>Hello, world! Pretty me up!
    </body>
    </html>
    </code></pre>
</body>
</html>";

                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext()
                {
                    JsEngineFunc = () => new TestJsEngine()
                };

                Highlight highlight = new Highlight();

                // When
                TestDocument result = await ExecuteAsync(document, context, highlight).SingleAsync();

                // Then
                result.Content.ShouldContain("language-html hljs");
            }

            [Test]
            public async Task CanHighlightAfterRazor()
            {
                // Given
                // if we execute razor before this, the code block will be escaped.
                const string input = @"
<html>
<head>
    <title>Foobar</title>
</head>
<body>
    <h1>Title</h1>
    <p>This is some Foobar text</p>
    <pre><code class=""language-html"">
    &lt;strong class=&quot;super-strong&quot;&gt;this is strong text&lt;/strong&gt;
    </code></pre>
</body>
</html>";

                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext()
                {
                    JsEngineFunc = () => new TestJsEngine()
                };

                Highlight highlight = new Highlight();

                // When
                TestDocument result = await ExecuteAsync(document, context, highlight).SingleAsync();

                // Then
                result.Content.ShouldContain("language-html hljs");
            }

            [Test]
            public async Task CanHighlightAutoCodeBlocks()
            {
                // Given
                const string input = @"
<html>
<head>
    <title>Foobar</title>
</head>
<body>
    <h1>Title</h1>
    <p>This is some Foobar text</p>
    <pre><code>
        if (foo == bar)
        {
            DoTheFooBar();
        }
    </code></pre>
</body>
</html>";

                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext()
                {
                    JsEngineFunc = () => new TestJsEngine()
                };

                Highlight highlight = new Highlight();

                // When
                TestDocument result = await ExecuteAsync(document, context, highlight).SingleAsync();

                // Then
                result.Content.ShouldContain("hljs");
            }

            [Test]
            public async Task HighlightFailsForMissingLanguage()
            {
                // Given
                const string input = @"
<html>
<head>
    <title>Foobar</title>
</head>
<body>
    <h1>Title</h1>
    <p>This is some Foobar text</p>
    <pre><code class=""language-zorg"">
    <html>
    <head>
    <title>Hi Mom!</title>
    </head>
    <body>
        <p>Hello, world! Pretty me up!
    </body>
    </html>
    </code></pre>
</body>
</html>";

                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext()
                {
                    JsEngineFunc = () => new TestJsEngine()
                };

                Highlight highlight = new Highlight();

                // When, Then
                await Should.ThrowAsync<Exception>(async () => await ExecuteAsync(document, context, highlight));
            }

            [Test]
            public async Task HighlightSucceedsForMissingLanguageWhenConfiguredNotToWarn()
            {
                // Given
                const string input = @"
<html>
<head>
    <title>Foobar</title>
</head>
<body>
    <h1>Title</h1>
    <p>This is some Foobar text</p>
    <pre><code class=""language-zorg"">
    <html>
    <head>
    <title>Hi Mom!</title>
    </head>
    <body>
        <p>Hello, world! Pretty me up!
    </body>
    </html>
    </code></pre>
</body>
</html>";

                TestDocument document = new TestDocument(input);
                TestExecutionContext context = new TestExecutionContext()
                {
                    JsEngineFunc = () => new TestJsEngine()
                };

                Highlight highlight = new Highlight()
                    .WithMissingLanguageWarning(false);

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, highlight);

                // Then
                results.ShouldNotBeEmpty();
            }
        }
    }
}