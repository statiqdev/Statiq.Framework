using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Wyam.Common.Configuration;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Common.Meta;
using Wyam.Core.Documents;
using Wyam.Core.Execution;
using Wyam.Core.Meta;
using Wyam.Core.Modules.Control;
using Wyam.Core.Modules.Extensibility;
using Wyam.Core.Modules.Metadata;
using Wyam.Testing;
using Wyam.Testing.Execution;
using Wyam.Testing.Modules;

namespace Wyam.Core.Tests.Modules.Control
{
    [TestFixture]
    [NonParallelizable]
    public class GroupByFixture : BaseFixture
    {
        public class ExecuteTests : GroupByFixture
        {
            [Test]
            public async Task SetsCorrectMetadata()
            {
                // Given
                List<int> groupKey = new List<int>();
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7
                };
                GroupBy groupBy = new GroupBy(Config.FromDocument(d => d.Get<int>("A") % 3), count);
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(d =>
                    {
                        groupKey.Add(d.Get<int>(Keys.GroupKey));
                        return (object)null;
                    }), false);
                engine.Pipelines.Add(groupBy, gatherData);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                CollectionAssert.AreEquivalent(new[] { 0, 1, 2 }, groupKey);
            }

            [Test]
            public async Task SetsDocumentsInMetadata()
            {
                // Given
                List<IList<string>> content = new List<IList<string>>();
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7
                };
                GroupBy groupBy = new GroupBy(Config.FromDocument(d => d.Get<int>("A") % 3), count);
                OrderBy orderBy = new OrderBy(Config.FromDocument(d => d.Get<int>(Keys.GroupKey)));
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(d =>
                    {
                        content.Add(d.Get<IList<IDocument>>(Keys.GroupDocuments).Select(x => x.Content).ToList());
                        return (object)null;
                    }), false);
                engine.Pipelines.Add(groupBy, orderBy, gatherData);

                // When
                await engine.ExecuteAsync(serviceProvider);

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
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7
                };
                Core.Modules.Metadata.Meta meta = new Core.Modules.Metadata.Meta("GroupMetadata", Config.FromDocument(d => d.Get<int>("A") % 3));
                GroupBy groupBy = new GroupBy("GroupMetadata", count, meta);
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(d =>
                    {
                        groupKey.Add(d.Get<int>(Keys.GroupKey));
                        return (object)null;
                    }), false);
                engine.Pipelines.Add(groupBy, gatherData);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                CollectionAssert.AreEquivalent(new[] { 0, 1, 2 }, groupKey);
            }

            [Test]
            public async Task GroupByMetadataKeyWithMissingMetadata()
            {
                // Given
                List<int> groupKey = new List<int>();
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7
                };
                Execute meta = new ExecuteDocument(
                    Config.FromDocument((d, c) =>
                    {
                        int groupMetadata = d.Get<int>("A") % 3;
                        return groupMetadata == 0 ? d : c.GetDocument(d, new MetadataItems { { "GroupMetadata", groupMetadata } });
                    }), false);
                GroupBy groupBy = new GroupBy("GroupMetadata", count, meta);
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(d =>
                    {
                        groupKey.Add(d.Get<int>(Keys.GroupKey));
                        return (object)null;
                    }), false);
                engine.Pipelines.Add(groupBy, gatherData);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                CollectionAssert.AreEquivalent(new[] { 1, 2 }, groupKey);
            }

            [Test]
            public async Task ExcludesDocumentsThatDontMatchPredicate()
            {
                // Given
                List<int> groupKey = new List<int>();
                IServiceProvider serviceProvider = new TestServiceProvider();
                Engine engine = new Engine();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7
                };
                GroupBy groupBy = new GroupBy(Config.FromDocument(d => d.Get<int>("A") % 3), count)
                    .Where(Config.IfDocument(d => d.Get<int>("A") % 3 != 0));
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(d =>
                    {
                        groupKey.Add(d.Get<int>(Keys.GroupKey));
                        return (object)null;
                    }), false);
                engine.Pipelines.Add(groupBy, gatherData);

                // When
                await engine.ExecuteAsync(serviceProvider);

                // Then
                CollectionAssert.AreEquivalent(new[] { 1, 2 }, groupKey);
            }
        }
    }
}
