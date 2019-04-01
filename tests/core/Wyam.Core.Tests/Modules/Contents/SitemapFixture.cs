using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Common.Modules;
using Wyam.Common.Modules.Contents;
using Wyam.Common.Tracing;
using Wyam.Core.Documents;
using Wyam.Core.Execution;
using Wyam.Core.Modules.Contents;
using Wyam.Core.Modules.Metadata;
using Wyam.Testing;
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
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                if (!string.IsNullOrWhiteSpace(hostname))
                {
                    engine.Settings[Keys.Host] = hostname;
                }
                ExecutionPipeline contentPipeline = new ExecutionPipeline("Content", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, Guid.Empty, contentPipeline, serviceProvider);

                IDocument doc = await context.GetDocumentAsync("Test", new[]
                {
                    new KeyValuePair<string, object>(Keys.RelativeFilePath, "sub/testfile.html")
                });
                IDocument[] inputs = { doc };

                Core.Modules.Metadata.Meta m = new Core.Modules.Metadata.Meta(
                    Keys.SitemapItem,
                    (d, c) => new SitemapItem(d[Keys.RelativeFilePath].ToString()));
                IEnumerable<IDocument> outputs = m.Execute(inputs, context);

                Func<string, string> formatter = null;

                if (!string.IsNullOrWhiteSpace(formatterString))
                {
                    formatter = f => string.Format(formatterString, f);
                }

                // When
                Sitemap sitemap = new Sitemap(formatter);
                List<IDocument> results = sitemap.Execute(outputs.ToList(), context).ToList();

                foreach (IDocument document in inputs.Concat(outputs.ToList()))
                {
                    ((IDisposable)document).Dispose();
                }

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
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                if (!string.IsNullOrWhiteSpace(hostname))
                {
                    engine.Settings[Keys.Host] = hostname;
                }
                ExecutionPipeline contentPipeline = new ExecutionPipeline("Content", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, Guid.Empty, contentPipeline, serviceProvider);

                IDocument doc = await context.GetDocumentAsync("Test", new[]
                {
                    new KeyValuePair<string, object>(Keys.RelativeFilePath, "sub/testfile.html")
                });
                IDocument[] inputs = { doc };

                Core.Modules.Metadata.Meta m = new Core.Modules.Metadata.Meta(
                    Keys.SitemapItem,
                    (d, c) => d[Keys.RelativeFilePath].ToString());
                IEnumerable<IDocument> outputs = m.Execute(inputs, context);

                Func<string, string> formatter = null;

                if (!string.IsNullOrWhiteSpace(formatterString))
                {
                    formatter = f => string.Format(formatterString, f);
                }

                // When
                Sitemap sitemap = new Sitemap(formatter);
                List<IDocument> results = sitemap.Execute(outputs.ToList(), context).ToList();

                foreach (IDocument document in inputs.Concat(outputs.ToList()))
                {
                    ((IDisposable)document).Dispose();
                }

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
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                if (!string.IsNullOrWhiteSpace(hostname))
                {
                    engine.Settings[Keys.Host] = hostname;
                }
                ExecutionPipeline contentPipeline = new ExecutionPipeline("Content", (IModuleList)null);
                IExecutionContext context = new ExecutionContext(engine, Guid.Empty, contentPipeline, serviceProvider);

                IDocument doc = await context.GetDocumentAsync("Test", new[]
                {
                    new KeyValuePair<string, object>(Keys.RelativeFilePath, "sub/testfile.html")
                });
                IDocument[] inputs = { doc };

                Func<string, string> formatter = null;

                if (!string.IsNullOrWhiteSpace(formatterString))
                {
                    formatter = f => string.Format(formatterString, f);
                }

                // When
                Sitemap sitemap = new Sitemap(formatter);
                List<IDocument> results = sitemap.Execute(inputs.ToList(), context).ToList();

                foreach (IDocument document in inputs)
                {
                    document.Dispose();
                }

                // Then
                Assert.AreEqual(1, results.Count);
                Assert.That(results[0].Content, Does.Contain($"<loc>{expected}</loc>"));
            }
        }
    }
}
