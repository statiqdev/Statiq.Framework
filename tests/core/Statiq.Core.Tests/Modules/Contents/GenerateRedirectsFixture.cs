using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Testing.Documents;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class GenerateRedirectsFixture : BaseFixture
    {
        [Test]
        public async Task SingleRedirect()
        {
            // Given
            TestDocument redirected = new TestDocument(new MetadataItems
            {
                { Keys.RedirectFrom, new List<FilePath> { new FilePath("foo.html") } }
            });
            TestDocument notRedirected = new TestDocument();
            GenerateRedirects redirect = new GenerateRedirects();

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { redirected, notRedirected }, redirect);

            // Then
            CollectionAssert.AreEqual(new[] { "foo.html" }, results.Select(x => x.Destination.FullPath));
        }

        [TestCase("foo/bar", "foo/bar.html")]
        [TestCase("foo/bar.html", "foo/bar.html")]
        [TestCase("foo/bar.baz", "foo/bar.baz.html")]
        public async Task AddsExtension(string input, string expected)
        {
            // Given
            TestDocument redirected = new TestDocument(new MetadataItems
            {
                { Keys.RedirectFrom, new List<FilePath> { new FilePath(input) } }
            });
            TestDocument notRedirected = new TestDocument();
            GenerateRedirects redirect = new GenerateRedirects();

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { redirected, notRedirected }, redirect);

            // Then
            CollectionAssert.AreEqual(new[] { expected }, results.Select(x => x.Destination.FullPath));
        }

        [Test]
        public async Task WarnsForAbsoluteRedirectFromPath()
        {
            // Given
            TestDocument redirected = new TestDocument(new FilePath("/"), null, new MetadataItems
            {
                { Keys.RedirectFrom, new List<FilePath> { new FilePath("/foo/bar") } }
            });
            TestDocument notRedirected = new TestDocument();
            GenerateRedirects redirect = new GenerateRedirects();
            ThrowOnTraceEventType(null);

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { redirected, notRedirected }, redirect);

            // Then
            Assert.IsTrue(Listener.Messages.ToList().Single(x => x.Key == TraceEventType.Warning).Value.StartsWith("The redirect path must be relative"));
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public async Task MultipleRedirects()
        {
            // Given
            TestDocument redirected1 = new TestDocument(new MetadataItems
            {
                { Keys.RedirectFrom, new List<FilePath> { new FilePath("foo.html") } }
            });
            TestDocument redirected2 = new TestDocument(new MetadataItems
            {
                { Keys.RedirectFrom, new List<FilePath> { new FilePath("bar/baz.html") } }
            });
            GenerateRedirects redirect = new GenerateRedirects();

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { redirected1, redirected2 }, redirect);

            // Then
            CollectionAssert.AreEquivalent(new[] { "foo.html", "bar/baz.html" }, results.Select(x => x.Destination.FullPath));
        }

        [Test]
        public async Task WithAdditionalOutput()
        {
            // Given
            TestDocument redirected1 = new TestDocument(
                new FilePath("foo2.html"),
                new MetadataItems
                {
                    { Keys.RedirectFrom, new List<FilePath> { new FilePath("foo.html") } }
                });
            TestDocument redirected2 = new TestDocument(new MetadataItems
            {
                { Keys.RedirectFrom, new List<FilePath> { new FilePath("bar/baz.html") } }
            });
            GenerateRedirects redirect = new GenerateRedirects().WithAdditionalOutput(new FilePath("a/b"), x => string.Join("|", x.Select(y => $"{y.Key} {y.Value}")));

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { redirected1, redirected2 }, redirect);

            // Then
            CollectionAssert.AreEquivalent(new[] { "foo.html", "bar/baz.html", "a/b" }, results.Select(x => x.Destination.FullPath));
            Assert.IsTrue(results.Single(x => x.Destination.FullPath == "a/b").Content.Contains("foo.html /foo2.html"));
        }

        [Test]
        public async Task WithAdditionalOutputWithoutMetaRefresh()
        {
            // Given
            TestDocument redirected1 = new TestDocument(
                new FilePath("foo2.html"),
                new MetadataItems
                {
                    { Keys.RedirectFrom, new List<FilePath> { new FilePath("foo.html") } }
                });
            TestDocument redirected2 = new TestDocument(new MetadataItems
            {
                { Keys.RedirectFrom, new List<FilePath> { new FilePath("bar/baz.html") } }
            });
            GenerateRedirects redirect = new GenerateRedirects()
                .WithAdditionalOutput(new FilePath("a/b"), x => string.Join("|", x.Select(y => $"{y.Key} {y.Value}")))
                .WithMetaRefreshPages(false);

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { redirected1, redirected2 }, redirect);

            // Then
            CollectionAssert.AreEquivalent(new[] { "a/b" }, results.Select(x => x.Destination.FullPath));
        }
    }
}
