using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
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
    public class OrderDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : OrderDocumentsFixture
        {
            [Test]
            public async Task OrderByOrdersInAscendingOrder()
            {
                // Given
                List<string> content = new List<string>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 4,
                    EnsureInputDocument = true
                };
                CountModule count2 = new CountModule("A")
                {
                    AdditionalOutputs = 2
                };
                Concat concat = new Concat(count2);
                OrderDocuments orderBy = new OrderDocuments(Config.FromDocument(d => d.Get<int>("A")));
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetStringAsync());
                        return (IDocument)null;
                    }), false);

                // When
                await ExecuteAsync(count, concat, orderBy, gatherData);

                // Then
                Assert.AreEqual(8, content.Count);
                CollectionAssert.AreEqual(new[] { "1", "1", "2", "2", "3", "3", "4", "5" }, content);
            }

            [Test]
            public async Task OrderByOrdersInDescendingOrder()
            {
                // Given
                List<string> content = new List<string>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 4,
                    EnsureInputDocument = true
                };
                CountModule count2 = new CountModule("A")
                {
                    AdditionalOutputs = 2
                };
                Concat concat = new Concat(count2);
                OrderDocuments orderBy = new OrderDocuments(Config.FromDocument(d => d.Get<int>("A"))).Descending();
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetStringAsync());
                        return (IDocument)null;
                    }), false);

                // When
                await ExecuteAsync(count, concat, orderBy, gatherData);

                // Then
                Assert.AreEqual(8, content.Count);
                CollectionAssert.AreEqual(new[] { "5", "4", "3", "3", "2", "2", "1", "1" }, content);
            }

            [Test]
            public async Task OrderByOrdersThenByInAscendingOrder()
            {
                // Given
                List<string> content = new List<string>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 4,
                    EnsureInputDocument = true
                };
                CountModule count2 = new CountModule("B")
                {
                    AdditionalOutputs = 1
                };
                OrderDocuments orderBy = new OrderDocuments(Config.FromDocument(d => d.Get<int>("A")))
                    .ThenBy(Config.FromDocument(d => d.Get<int>("B")));
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetStringAsync());
                        return (IDocument)null;
                    }), false);

                // When
                await ExecuteAsync(count, count2, orderBy, gatherData);

                // Then
                Assert.AreEqual(10, content.Count); // (4+1) * (21+1)
                CollectionAssert.AreEqual(new[] { "11", "12", "23", "24", "35", "36", "47", "48", "59", "510" }, content);
            }

            [Test]
            public async Task OrderByOrdersThenByInDescendingOrder()
            {
                // Given
                List<string> content = new List<string>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 4,
                    EnsureInputDocument = true
                };
                CountModule count2 = new CountModule("B")
                {
                    AdditionalOutputs = 1
                };
                OrderDocuments orderBy = new OrderDocuments(Config.FromDocument(d => d.Get<int>("A")))
                    .ThenBy(Config.FromDocument(d => d.Get<int>("B")))
                    .Descending();
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetStringAsync());
                        return (IDocument)null;
                    }), false);

                // When
                await ExecuteAsync(count, count2, orderBy, gatherData);

                // Then
                Assert.AreEqual(10, content.Count); // (4+1) * (21+1)
                CollectionAssert.AreEqual(new[] { "12", "11", "24", "23", "36", "35", "48", "47", "510", "59" }, content);
            }

            [Test]
            public async Task OrderByOrdersDescendingThenByInDescendingOrder()
            {
                // Given
                List<string> content = new List<string>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 4,
                    EnsureInputDocument = true
                };
                CountModule count2 = new CountModule("B")
                {
                    AdditionalOutputs = 1
                };
                OrderDocuments orderBy = new OrderDocuments(Config.FromDocument(d => d.Get<int>("A")))
                    .Descending()
                    .ThenBy(Config.FromDocument(d => d.Get<int>("B")))
                    .Descending();
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetStringAsync());
                        return (IDocument)null;
                    }), false);

                // When
                await ExecuteAsync(count, count2, orderBy, gatherData);

                // Then
                Assert.AreEqual(10, content.Count); // (4+1) * (21+1)
                CollectionAssert.AreEqual(new[] { "510", "59", "48", "47", "36", "35", "24", "23", "12", "11" }, content);
            }

            [Test]
            public async Task OrderByOrdersDescendingThenByInAscendingOrder()
            {
                // Given
                List<string> content = new List<string>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 4,
                    EnsureInputDocument = true
                };
                CountModule count2 = new CountModule("B")
                {
                    AdditionalOutputs = 1
                };
                OrderDocuments orderBy = new OrderDocuments(Config.FromDocument(d => d.Get<int>("A")))
                    .Descending()
                    .ThenBy(Config.FromDocument(d => d.Get<int>("B")));
                Execute gatherData = new ExecuteDocument(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetStringAsync());
                        return (IDocument)null;
                    }), false);

                // When
                await ExecuteAsync(count, count2, orderBy, gatherData);

                // Then
                Assert.AreEqual(10, content.Count); // (4+1) * (21+1)
                CollectionAssert.AreEqual(new[] { "59", "510", "47", "48", "35", "36", "23", "24", "11", "12" }, content);
            }
        }
    }
}
