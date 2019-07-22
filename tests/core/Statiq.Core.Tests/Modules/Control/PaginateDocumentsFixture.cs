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
            public async Task PaginateSetsCorrectMetadata()
            {
                // Given
                List<int> currentPage = new List<int>();
                List<int> totalPages = new List<int>();
                List<int> totalItems = new List<int>();
                List<bool> hasNextPage = new List<bool>();
                List<bool> hasPreviousPage = new List<bool>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7,
                    EnsureInputDocument = true
                };
                PaginateDocuments paginate = new PaginateDocuments(3);
                ForEachDocument gatherData = new ExecuteConfig(Config.FromDocument(
                    d =>
                    {
                        currentPage.Add(d.Int(Keys.CurrentPage));
                        totalPages.Add(d.Int(Keys.TotalPages));
                        totalItems.Add(d.Int(Keys.TotalItems));
                        hasNextPage.Add(d.Bool(Keys.HasNextPage));
                        hasPreviousPage.Add(d.Bool(Keys.HasPreviousPage));
                    })).ForEachDocument();

                // When
                await ExecuteAsync(count, paginate, gatherData);

                // Then
                CollectionAssert.AreEqual(new[] { 1, 2, 3 }, currentPage);
                CollectionAssert.AreEqual(new[] { 3, 3, 3 }, totalPages);
                CollectionAssert.AreEqual(new[] { 8, 8, 8 }, totalItems);
                CollectionAssert.AreEqual(new[] { true, true, false }, hasNextPage);
                CollectionAssert.AreEqual(new[] { false, true, true }, hasPreviousPage);
            }

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
                        IEnumerable<string> pageContent = await d.Get<IList<IDocument>>(Keys.PageDocuments).SelectAsync(async x => await x.GetStringAsync());
                        content.Add(pageContent.ToList());
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
