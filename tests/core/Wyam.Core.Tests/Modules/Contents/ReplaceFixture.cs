using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Meta;
using Wyam.Common.Util;
using Wyam.Core.Modules.Contents;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class ReplaceFixture
    {
        [Test]
        public async Task RecursiveReplaceWithContentFinder()
        {
            // Given
            const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <span>foo<span>bar</span></span>
                        </body>
                    </html>";
            const string expected = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <span>baz</span>
                        </body>
                    </html>";
            TestExecutionContext context = new TestExecutionContext();
            TestDocument document = new TestDocument(input);
            Replace replace = new Replace(@"(<span>.*<\/span>)", _ => "<span>baz</span>");

            // When
            IList<IDocument> results = await replace.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

            // Then
            Assert.That(results.First().Content, Is.EquivalentTo(expected));
        }

        [Test]
        public async Task ReplaceWithContentFinderUsingDocument()
        {
            // Given
            const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <span>foo<span>bar</span></span>
                        </body>
                    </html>";
            const string expected = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <div>Buzz</div>
                        </body>
                    </html>";
            TestExecutionContext context = new TestExecutionContext();
            TestDocument document = new TestDocument(input, new MetadataItems
            {
                { "Fizz", "Buzz" }
            });
            Replace replace = new Replace(@"(<span>.*<\/span>)", (_, doc) => $"<div>{doc["Fizz"]}</div>");

            // When
            IList<IDocument> results = await replace.ExecuteAsync(new[] { document }, context).ToListAsync();  // Make sure to materialize the result list

            // Then
            Assert.That(results.First().Content, Is.EquivalentTo(expected));
        }
    }
}
