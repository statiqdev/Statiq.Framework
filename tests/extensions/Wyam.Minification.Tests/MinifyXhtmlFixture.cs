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
    public class MinifyXhtmlFixture : BaseFixture
    {
        public class ExecuteTests : MinifyXhtmlFixture
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
                const string output = "<html><head><title>Title</title></head><body><h1>Title</h1><p>This is<br />some text</p></body></html>";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input);
                MinifyXhtml minifyXhtml = new MinifyXhtml();

                // When
                IList<IDocument> results = await minifyXhtml.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                results.Single().Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}