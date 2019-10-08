using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class CreateDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : CreateDocumentsFixture
        {
            [Test]
            public async Task CountReturnsCorrectDocuments()
            {
                // Given
                CreateDocuments documents = new CreateDocuments(5);

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents);

                // Then
                Assert.AreEqual(5, results.Count);
            }

            [Test]
            public async Task ContentReturnsCorrectDocuments()
            {
                // Given
                List<string> content = new List<string>();
                CreateDocuments documents = new CreateDocuments("A", "B", "C", "D");
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetContentStringAsync());
                        return (object)null;
                    })).ForEachDocument();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, gatherData);

                // Then
                Assert.AreEqual(4, content.Count);
                CollectionAssert.AreEqual(new[] { "A", "B", "C", "D" }, content);
            }

            [Test]
            public async Task MetadataReturnsCorrectDocuments()
            {
                // Given
                List<object> values = new List<object>();
                CreateDocuments documents = new CreateDocuments(
                    new Dictionary<string, object> { { "Foo", "a" } },
                    new Dictionary<string, object> { { "Foo", "b" } },
                    new Dictionary<string, object> { { "Foo", "c" } });
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(d =>
                    {
                        values.Add(d["Foo"]);
                        return (object)null;
                    })).ForEachDocument();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, gatherData);

                // Then
                Assert.AreEqual(3, values.Count);
                CollectionAssert.AreEqual(new[] { "a", "b", "c" }, values);
            }

            [Test]
            public async Task ContentAndMetadataReturnsCorrectDocuments()
            {
                // Given
                List<string> content = new List<string>();
                List<object> values = new List<object>();
                CreateDocuments documents = new CreateDocuments(
                    Tuple.Create("A", new Dictionary<string, object> { { "Foo", "a" } }.AsEnumerable()),
                    Tuple.Create("B", new Dictionary<string, object> { { "Foo", "b" } }.AsEnumerable()),
                    Tuple.Create("C", new Dictionary<string, object> { { "Foo", "c" } }.AsEnumerable()));
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetContentStringAsync());
                        values.Add(d["Foo"]);
                        return (object)null;
                    })).ForEachDocument();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(documents, gatherData);

                // Then
                Assert.AreEqual(3, content.Count);
                Assert.AreEqual(3, values.Count);
                CollectionAssert.AreEqual(new[] { "A", "B", "C" }, content);
                CollectionAssert.AreEqual(new[] { "a", "b", "c" }, values);
            }
        }
    }
}
