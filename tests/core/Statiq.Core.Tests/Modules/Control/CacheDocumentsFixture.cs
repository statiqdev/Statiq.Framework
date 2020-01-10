using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class CacheDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : CacheDocumentsFixture
        {
            [Test]
            public async Task CachesDocuments()
            {
                // Given
                TestDocument a1 = new TestDocument(new FilePath("/input/a"), "a");
                TestDocument b1 = new TestDocument(new FilePath("/input/b"), "b");
                TestDocument a2 = new TestDocument(new FilePath("/input/a"), "aa");
                TestDocument b2 = new TestDocument(new FilePath("/input/b"), "b");
                CacheDocuments cacheDocuments = new CacheDocuments(
                    new SetMetadata("Content", Config.FromDocument(async doc => await doc.GetContentStringAsync())));

                // When
                _ = await ExecuteAsync(new[] { a1, b1 }, cacheDocuments);
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a2, b2 }, cacheDocuments);

                // Then
                CollectionAssert.AreEqual(new[] { "b", "aa" }, results.Select(x => x["Content"]));
                CollectionAssert.AreEqual(new[] { b1.Id, a2.Id }, results.Select(x => x.Id));
            }
        }
    }
}
