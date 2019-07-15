using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class FilterSourcesFixture : BaseFixture
    {
        public class ExecuteTests : FilterSourcesFixture
        {
            [Test]
            public async Task FiltersSources()
            {
                // Given
                TestDocument a = new TestDocument(new FilePath("/input/Foo/a.txt"));
                TestDocument b = new TestDocument(new FilePath("/theme/Foo/Bar/b.txt"));
                TestDocument c = new TestDocument(new FilePath("/Foo/Bar/c.txt"));
                TestDocument d = new TestDocument(new FilePath("/Baz/d.txt"));
                FilterSources filter = new FilterSources("Foo/**/*.txt");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b, c, d }, filter);

                // Then
                results.ShouldBe(new[] { a, b }, true);
            }
        }
    }
}
