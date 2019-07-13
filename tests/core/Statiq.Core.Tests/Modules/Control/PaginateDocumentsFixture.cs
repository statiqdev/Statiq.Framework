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
                PaginateDocuments paginate = new PaginateDocuments(3, count);
                Execute gatherData = new ExecuteDocument(
                    (d, c) =>
                    {
                        currentPage.Add(d.Int(Keys.CurrentPage));
                        totalPages.Add(d.Int(Keys.TotalPages));
                        totalItems.Add(d.Int(Keys.TotalItems));
                        hasNextPage.Add(d.Bool(Keys.HasNextPage));
                        hasPreviousPage.Add(d.Bool(Keys.HasPreviousPage));
                        return null;
                    }, false);

                // When
                await ExecuteAsync(paginate, gatherData);

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
                PaginateDocuments paginate = new PaginateDocuments(3, count);
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument<object>(async (d, c) =>
                    {
                        IEnumerable<string> pageContent = await d.Get<IList<IDocument>>(Keys.PageDocuments).SelectAsync(async x => await x.GetStringAsync());
                        content.Add(pageContent.ToList());
                        return null;
                    }), false);

                // When
                await ExecuteAsync(paginate, gatherData);

                // Then
                Assert.AreEqual(3, content.Count);
                CollectionAssert.AreEqual(new[] { "1", "2", "3" }, content[0]);
                CollectionAssert.AreEqual(new[] { "4", "5", "6" }, content[1]);
                CollectionAssert.AreEqual(new[] { "7", "8" }, content[2]);
            }

            [Test]
            public async Task SetsPreviousAndNextDocuments()
            {
                // Given
                List<IList<string>> previousPages = new List<IList<string>>();
                List<IList<string>> nextPages = new List<IList<string>>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7,
                    EnsureInputDocument = true
                };
                PaginateDocuments paginate = new PaginateDocuments(3, count);
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument<object>(async (d, c) =>
                    {
                        IList<IDocument> previousPageDocuments = d.Document(Keys.PreviousPage)?.Get<IList<IDocument>>(Keys.PageDocuments);
                        IEnumerable<string> previousPageContent = previousPageDocuments == null ? null : await previousPageDocuments.SelectAsync(async x => await x.GetStringAsync());
                        previousPages.Add(previousPageContent?.ToList());
                        IList<IDocument> nextPageDocuments = d.Document(Keys.NextPage)?.Get<IList<IDocument>>(Keys.PageDocuments);
                        IEnumerable<string> nextPageContent = nextPageDocuments == null ? null : await nextPageDocuments.SelectAsync(async x => await x.GetStringAsync());
                        nextPages.Add(nextPageContent?.ToList());
                        return null;
                    }), false);

                // When
                await ExecuteAsync(paginate, gatherData);

                // Then
                Assert.AreEqual(3, previousPages.Count);
                Assert.AreEqual(3, nextPages.Count);
                CollectionAssert.AreEqual(null, previousPages[0]);
                CollectionAssert.AreEqual(new[] { "1", "2", "3" }, previousPages[1]);
                CollectionAssert.AreEqual(new[] { "4", "5", "6" }, previousPages[2]);
                CollectionAssert.AreEqual(new[] { "4", "5", "6" }, nextPages[0]);
                CollectionAssert.AreEqual(new[] { "7", "8" }, nextPages[1]);
                CollectionAssert.AreEqual(null, nextPages[2]);
            }

            [Test]
            public async Task ExcludesDocumentsThatFailPredicate()
            {
                // Given
                List<IList<string>> content = new List<IList<string>>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7,
                    EnsureInputDocument = true
                };
                PaginateDocuments paginate = new PaginateDocuments(3, count).Where(Config.FromDocument(async doc => await doc.GetStringAsync() != "5"));
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument<object>(async (d, c) =>
                    {
                        IEnumerable<string> pageContent = await d.Get<IList<IDocument>>(Keys.PageDocuments).SelectAsync(async x => await x.GetStringAsync());
                        content.Add(pageContent.ToList());
                        return null;
                    }), false);

                // When
                await ExecuteAsync(paginate, gatherData);

                // Then
                Assert.AreEqual(3, content.Count);
                CollectionAssert.AreEqual(new[] { "1", "2", "3" }, content[0]);
                CollectionAssert.AreEqual(new[] { "4", "6", "7" }, content[1]);
                CollectionAssert.AreEqual(new[] { "8" }, content[2]);
            }
        }
    }
}
