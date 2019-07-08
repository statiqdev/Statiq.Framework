using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Core.Modules.Control;
using Statiq.Testing;
using Statiq.Testing.Documents;
using Statiq.Testing.Execution;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class CombineDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : CombineDocumentsFixture
        {
            [Test]
            public async Task AppendsContent()
            {
                // Given
                TestDocument a = new TestDocument("a");
                TestDocument b = new TestDocument("b");
                CombineDocuments combine = new CombineDocuments();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b }, combine);

                // Then
                CollectionAssert.AreEqual(new[] { "ab" }, await results.SelectAsync(async x => await x.GetStringAsync()));
            }

            [Test]
            public async Task MergesMetadata()
            {
                // Given
                TestDocument a = new TestDocument(new Dictionary<string, object>
                {
                    { "a", 1 },
                    { "b", 2 }
                });
                TestDocument b = new TestDocument(new Dictionary<string, object>
                {
                    { "b", 3 },
                    { "c", 4 }
                });
                CombineDocuments combine = new CombineDocuments();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b }, combine);

                // Then
                new Dictionary<string, object>
                {
                    { "a", 1 },
                    { "b", 3 },
                    { "c", 4 }
                }.ShouldBeSubsetOf(results[0]);
            }
        }

        private IEnumerable Iterate(IEnumerator iterator)
        {
            while (iterator.MoveNext())
            {
                yield return iterator.Current;
            }
        }
    }
}
