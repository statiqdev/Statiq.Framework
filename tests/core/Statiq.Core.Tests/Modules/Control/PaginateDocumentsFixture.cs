using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class PaginateDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : PaginateDocumentsFixture
        {
            [Test]
            public async Task PaginateSetsDocumentsInMetadata()
            {
                // Given
                List<IList<string>> content = new List<IList<string>>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7,
                    EnsureInputDocument = true
                };
                PaginateDocuments paginate = new PaginateDocuments(3);
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(async d =>
                    {
                        List<string> pageContent = await d.GetChildren()
                            .ToAsyncEnumerable()
                            .SelectAwait(async x => await x.GetContentStringAsync())
                            .ToListAsync();
                        content.Add(pageContent);
                    })).ForEachDocument();

                // When
                await ExecuteAsync(count, paginate, gatherData);

                // Then
                Assert.AreEqual(3, content.Count);
                CollectionAssert.AreEqual(new[] { "1", "2", "3" }, content[0]);
                CollectionAssert.AreEqual(new[] { "4", "5", "6" }, content[1]);
                CollectionAssert.AreEqual(new[] { "7", "8" }, content[2]);
            }
        }
    }
}
