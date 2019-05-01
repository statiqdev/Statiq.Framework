using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Modules.Contents;
using Wyam.Common.Tracing;
using Wyam.Common.Util;
using Wyam.Core.Documents;
using Wyam.Core.Execution;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Metadata;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;
using ExecutionContext = Wyam.Core.Execution.ExecutionContext;

namespace Wyam.Core.Tests.Modules.Contents
{
    [TestFixture]
    [NonParallelizable]
    public class SitemapFixture : BaseFixture
    {
        public class ExecuteTests : SitemapFixture
        {
            [TestCase("www.example.org", null, "http://www.example.org/sub/testfile")]
            [TestCase(null, "http://www.example.com", "http://www.example.com")]
            [TestCase("www.example.org", "http://www.example.com/{0}", "http://www.example.com/sub/testfile.html")]
            public async Task SitemapGeneratedWithSitemapItem(string hostname, string formatterString, string expected)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                if (!string.IsNullOrWhiteSpace(hostname))
                {
                    context.Settings[Keys.Host] = hostname;
                }

                TestDocument doc = new TestDocument("Test")
                {
                    { Keys.RelativeFilePath, "sub/testfile.html" }
                };
                IDocument[] inputs = { doc };

                Core.Modules.Metadata.Meta m = new Core.Modules.Metadata.Meta(
                    Keys.SitemapItem,
                    Config.FromDocument(d => new SitemapItem(d[Keys.RelativeFilePath].ToString())));

                Func<string, string> formatter = null;

                if (!string.IsNullOrWhiteSpace(formatterString))
                {
                    formatter = f => string.Format(formatterString, f);
                }

                Sitemap sitemap = new Sitemap(formatter);

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(inputs, context, m, sitemap);

                // Then
                Assert.AreEqual(1, results.Count);
                Assert.That(results[0].Content, Does.Contain($"<loc>{expected}</loc>"));
            }

            [TestCase("www.example.org", null, "http://www.example.org/sub/testfile")]
            [TestCase(null, "http://www.example.com", "http://www.example.com")]
            [TestCase("www.example.org", "http://www.example.com/{0}", "http://www.example.com/sub/testfile.html")]
            public async Task SitemapGeneratedWithSitemapItemAsString(string hostname, string formatterString, string expected)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                if (!string.IsNullOrWhiteSpace(hostname))
                {
                    context.Settings[Keys.Host] = hostname;
                }

                TestDocument doc = new TestDocument("Test")
                {
                    { Keys.RelativeFilePath, "sub/testfile.html" }
                };
                IDocument[] inputs = { doc };

                Core.Modules.Metadata.Meta m = new Core.Modules.Metadata.Meta(
                    Keys.SitemapItem,
                    Config.FromDocument(d => d[Keys.RelativeFilePath].ToString()));

                Func<string, string> formatter = null;

                if (!string.IsNullOrWhiteSpace(formatterString))
                {
                    formatter = f => string.Format(formatterString, f);
                }

                Sitemap sitemap = new Sitemap(formatter);

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(inputs, context, m, sitemap);

                // Then
                Assert.AreEqual(1, results.Count);
                Assert.That(results[0].Content, Does.Contain($"<loc>{expected}</loc>"));
            }

            [TestCase("www.example.org", null, "http://www.example.org/sub/testfile")]
            [TestCase(null, "http://www.example.com", "http://www.example.com")]
            [TestCase("www.example.org", "http://www.example.com{0}", "http://www.example.com/sub/testfile")]
            public async Task SitemapGeneratedWhenNoSitemapItem(string hostname, string formatterString, string expected)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                if (!string.IsNullOrWhiteSpace(hostname))
                {
                    context.Settings[Keys.Host] = hostname;
                }

                TestDocument doc = new TestDocument("Test")
                {
                    { Keys.RelativeFilePath, "sub/testfile.html" }
                };
                IDocument[] inputs = { doc };

                Func<string, string> formatter = null;

                if (!string.IsNullOrWhiteSpace(formatterString))
                {
                    formatter = f => string.Format(formatterString, f);
                }

                Sitemap sitemap = new Sitemap(formatter);

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(inputs, context, sitemap);

                // Then
                Assert.AreEqual(1, results.Count);
                Assert.That(results[0].Content, Does.Contain($"<loc>{expected}</loc>"));
            }
        }
    }
}
