using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Util;
using Wyam.Core.Modules.Control;
using Wyam.Testing;
using Wyam.Testing.Documents;
using Wyam.Testing.Execution;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [NonParallelizable]
    public class CombineFixture : BaseFixture
    {
        public class ExecuteTests : CombineFixture
        {
            [Test]
            public async Task AppendsContent()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                IDocument a = new TestDocument("a");
                IDocument b = new TestDocument("b");
                Combine combine = new Combine();

                // When
                List<IDocument> results = await combine.ExecuteAsync(new[] { a, b }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEqual(new[] { "ab" }, await results.SelectAsync(async x => await x.GetStringAsync()));
            }

            [Test]
            public async Task MergesMetadata()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                IDocument a = new TestDocument(new Dictionary<string, object>
                {
                    { "a", 1 },
                    { "b", 2 }
                });
                IDocument b = new TestDocument(new Dictionary<string, object>
                {
                    { "b", 3 },
                    { "c", 4 }
                });
                Combine combine = new Combine();

                // When
                List<IDocument> results = await combine.ExecuteAsync(new[] { a, b }, context).ToListAsync();  // Make sure to materialize the result list

                // Then
                CollectionAssert.AreEquivalent(
                    new Dictionary<string, object>
                {
                    { "a", 1 },
                    { "b", 3 },
                    { "c", 4 }
                }, Iterate(results.First().GetEnumerator()));
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
