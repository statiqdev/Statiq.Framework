using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Wyam.Common.Documents;
using Wyam.Common.Util;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Minification.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class MinifyHtmlFixture : BaseFixture
    {
        public class ExecuteTests : MinifyHtmlFixture
        {
            [Test]
            public async Task Minify()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Title</title>
                        </head>
                        <body>
                            <!-- FOO -->
                            <h1>Title</h1>
                            <p>This is<br />some text</p>
                        </body>
                    </html>";
                const string output = "<html><head><title>Title</title><body><h1>Title</h1><p>This is<br>some text";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                MinifyHtml minifyHtml = new MinifyHtml();

                // When
                IList<IDocument> results = await minifyHtml.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task MinifyWithCustomSettings()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Title</title>
                        </head>
                        <body>
                            <!-- FOO -->
                            <h1>Title</h1>
                            <p>This is<br />some text</p>
                        </body>
                    </html>";
                const string output = "<html><head><title>Title</title></head><body><!-- FOO --><h1>Title</h1><p>This is<br>some text</p></body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                MinifyHtml minifyHtml = new MinifyHtml()
                    .WithSettings(settings =>
                    {
                        settings.RemoveOptionalEndTags = false;
                        settings.RemoveHtmlComments = false;
                    });

                // When
                IList<IDocument> results = await minifyHtml.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}