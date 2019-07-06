using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Common.Configuration;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.Meta;
using Statiq.Core.Modules.Control;
using Statiq.Core.Modules.Extensibility;
using Statiq.Testing;
using Statiq.Testing.Modules;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class GroupByFixture : BaseFixture
    {
        public class ExecuteTests : GroupByFixture
        {
            [Test]
            public async Task SetsCorrectMetadata()
            {
                // Given
                List<int> groupKey = new List<int>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7,
                    EnsureInputDocument = true
                };
                GroupBy groupBy = new GroupBy(Config.FromDocument(d => d.Get<int>("A") % 3), count);
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(d =>
                    {
                        groupKey.Add(d.Get<int>(Keys.GroupKey));
                        return (object)null;
                    }), false);

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(groupBy, gatherData);

                // Then
                CollectionAssert.AreEquivalent(new[] { 0, 1, 2 }, groupKey);
            }

            [Test]
            public async Task SetsDocumentsInMetadata()
            {
                // Given
                List<IList<string>> content = new List<IList<string>>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7,
                    EnsureInputDocument = true
                };
                GroupBy groupBy = new GroupBy(Config.FromDocument(d => d.Get<int>("A") % 3), count);
                OrderBy orderBy = new OrderBy(Config.FromDocument(d => d.Get<int>(Keys.GroupKey)));
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(async d =>
                    {
                        IEnumerable<string> groupContent = await d.Get<IList<IDocument>>(Keys.GroupDocuments).SelectAsync(async x => await x.GetStringAsync());
                        content.Add(groupContent.ToList());
                        return (object)null;
                    }), false);

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(groupBy, orderBy, gatherData);

                // Then
                Assert.AreEqual(3, content.Count);
                CollectionAssert.AreEquivalent(new[] { "3", "6" }, content[0]);
                CollectionAssert.AreEquivalent(new[] { "1", "4", "7" }, content[1]);
                CollectionAssert.AreEquivalent(new[] { "2", "5", "8" }, content[2]);
            }

            [Test]
            public async Task GroupByMetadataKey()
            {
                // Given
                List<int> groupKey = new List<int>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7,
                    EnsureInputDocument = true
                };
                Core.Modules.Metadata.Meta meta = new Core.Modules.Metadata.Meta("GroupMetadata", Config.FromDocument(d => d.Get<int>("A") % 3));
                GroupBy groupBy = new GroupBy("GroupMetadata", count, meta);
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(d =>
                    {
                        groupKey.Add(d.Get<int>(Keys.GroupKey));
                        return (object)null;
                    }), false);

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(groupBy, gatherData);

                // Then
                CollectionAssert.AreEquivalent(new[] { 0, 1, 2 }, groupKey);
            }

            [Test]
            public async Task GroupByMetadataKeyWithMissingMetadata()
            {
                // Given
                List<int> groupKey = new List<int>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7,
                    EnsureInputDocument = true
                };
                Execute meta = new ExecuteDocument(
                    Config.FromDocument(d =>
                    {
                        int groupMetadata = d.Get<int>("A") % 3;
                        return groupMetadata == 0 ? d : d.Clone(new MetadataItems { { "GroupMetadata", groupMetadata } });
                    }), false);
                GroupBy groupBy = new GroupBy("GroupMetadata", count, meta);
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(d =>
                    {
                        groupKey.Add(d.Get<int>(Keys.GroupKey));
                        return (object)null;
                    }), false);

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(groupBy, gatherData);

                // Then
                CollectionAssert.AreEquivalent(new[] { 1, 2 }, groupKey);
            }

            [Test]
            public async Task ExcludesDocumentsThatDontMatchPredicate()
            {
                // Given
                List<int> groupKey = new List<int>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7,
                    EnsureInputDocument = true
                };
                GroupBy groupBy = new GroupBy(Config.FromDocument(d => d.Get<int>("A") % 3), count)
                    .Where(Config.FromDocument(d => d.Get<int>("A") % 3 != 0));
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(d =>
                    {
                        groupKey.Add(d.Get<int>(Keys.GroupKey));
                        return (object)null;
                    }), false);

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(groupBy, gatherData);

                // Then
                CollectionAssert.AreEquivalent(new[] { 1, 2 }, groupKey);
            }
        }
    }
}
