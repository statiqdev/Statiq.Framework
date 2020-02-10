using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Documents
{
    [TestFixture]
    public class IDocumentEnumerableExtensionsFixture : BaseFixture
    {
        public class FilterSourcesTests : IDocumentEnumerableExtensionsFixture
        {
            [Test]
            public void FiltersSources()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument a = new TestDocument(new FilePath("/input/Foo/a.txt"));
                TestDocument b = new TestDocument(new FilePath("/theme/Foo/Bar/b.txt"));
                TestDocument c = new TestDocument(new FilePath("/Foo/Bar/c.txt"));
                TestDocument d = new TestDocument(new FilePath("/Baz/d.txt"));
                TestDocument[] documents = new[] { a, b, c, d };

                // When
                TestDocument[] results = documents.FilterSources(context, "Foo/**/*.txt").ToArray();

                // Then
                results.ShouldBe(new[] { a, b }, true);
            }
        }

        public class FilterDestinationsTests : IDocumentEnumerableExtensionsFixture
        {
            [Test]
            public void FiltersDestinations()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument a = new TestDocument(new FilePath("/input/Foo/a.txt"), new FilePath("a/b/c.txt"));
                TestDocument b = new TestDocument(new FilePath("/theme/Foo/Bar/b.txt"), new FilePath("a/x/y.txt"));
                TestDocument c = new TestDocument(new FilePath("/Foo/Bar/c.txt"), new FilePath("l/m.txt"));
                TestDocument d = new TestDocument(new FilePath("/Baz/d.txt"), new FilePath("l/n.md"));
                TestDocument[] documents = new[] { a, b, c, d };

                // When
                TestDocument[] results = documents.FilterDestinations("a/**/*.txt").ToArray();

                // Then
                results.ShouldBe(new[] { a, b }, true);
            }

            [Test]
            public void FiltersDestinationsAtRoot()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument a = new TestDocument(new FilePath("/input/Foo/a.txt"), new FilePath("c.txt"));
                TestDocument b = new TestDocument(new FilePath("/theme/Foo/Bar/b.txt"), new FilePath("a/x/y.txt"));
                TestDocument c = new TestDocument(new FilePath("/Foo/Bar/c.txt"), new FilePath("m.txt"));
                TestDocument d = new TestDocument(new FilePath("/Baz/d.txt"), new FilePath("l/n.md"));
                TestDocument[] documents = new[] { a, b, c, d };

                // When
                TestDocument[] results = documents.FilterDestinations("*.txt").ToArray();

                // Then
                results.ShouldBe(new[] { a, c }, true);
            }
        }
    }
}
