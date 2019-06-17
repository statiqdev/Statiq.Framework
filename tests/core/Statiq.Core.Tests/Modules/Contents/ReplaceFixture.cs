using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Common.Documents;
using Statiq.Common.Meta;
using Statiq.Core.Modules.Contents;
using Statiq.Testing;
using Statiq.Testing.Documents;
using Statiq.Testing.Execution;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class ReplaceFixture : BaseFixture
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
            TestDocument document = new TestDocument(input);
            Replace replace = new Replace(@"(<span>.*<\/span>)", _ => "<span>baz</span>");

            // When
            TestDocument result = await ExecuteAsync(document, replace).SingleAsync();

            // Then
            result.Content.ShouldBe(expected);
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
            TestDocument document = new TestDocument(input)
            {
                { "Fizz", "Buzz" }
            };
            Replace replace = new Replace(@"(<span>.*<\/span>)", (_, doc) => $"<div>{doc["Fizz"]}</div>");

            // When
            TestDocument result = await ExecuteAsync(document, replace).SingleAsync();

            // Then
            result.Content.ShouldBe(expected);
        }
    }
}
