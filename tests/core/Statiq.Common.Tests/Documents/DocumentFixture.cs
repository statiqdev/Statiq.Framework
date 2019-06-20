using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Shouldly;
using Statiq.Common.Content;
using Statiq.Common.Documents;
using Statiq.Common.Execution;
using Statiq.Common.IO;
using Statiq.Common.Meta;
using Statiq.Testing;
using Statiq.Testing.Execution;

namespace Statiq.Common.Tests.Documents
{
    [TestFixture]
    [Parallelizable(ParallelScope.Self | ParallelScope.Children)]
    public class DocumentFixture : BaseFixture
    {
        public class ConstructorTests : DocumentFixture
        {
            [Test]
            public void IdIsNotTheSameForDifferentDocuments()
            {
                // Given
                TestEngine engine = new TestEngine();

                // When
                Document a = new Document(engine, null, null, null, null);
                Document b = new Document(engine, null, null, null, null);

                // Then
                Assert.AreNotEqual(a.Id, b.Id);
            }
        }

        public class CloneTests : DocumentFixture
        {
            [Test]
            public void IdIsTheSameAfterClone()
            {
                // Given
                TestEngine engine = new TestEngine();
                Document document = new Document(engine, null, null, null, null);

                // When
                IDocument cloned = document.Clone(null, null, null, null);

                // Then
                Assert.AreEqual(document.Id, cloned.Id);
            }
        }

        public class MetadataTests : DocumentFixture
        {
            [Test]
            public void ReturnsMetadataWithoutSettings()
            {
                // Given
                TestEngine engine = new TestEngine();
                engine.Settings.Add("A", "a");
                Document document = new Document(engine, null, null, null, null);
                Document cloned = document.Clone(
                    null,
                    null,
                    new MetadataItems { { "B", "b" } },
                    null);

                // When
                string initialA = document.String("A");
                string initialB = document.String("B");
                string clonedA = cloned.String("A");
                string clonedB = cloned.String("B");
                string onlyMetadataA = cloned.Metadata.String("A");
                string onlyMetadataB = cloned.Metadata.String("B");

                // Then
                Assert.AreEqual("a", initialA);
                Assert.IsNull(initialB);
                Assert.AreEqual("a", clonedA);
                Assert.AreEqual("b", clonedB);
                Assert.IsNull(onlyMetadataA);
                Assert.AreEqual("b", onlyMetadataB);
            }

            [Test]
            public void MetadataOverwritesSettings()
            {
                // Given
                TestEngine engine = new TestEngine();
                engine.Settings.Add("A", "a");
                Document document = new Document(engine, null, null, null, null);
                Document cloned = document.Clone(
                    null,
                    null,
                    new MetadataItems { { "A", "b" } },
                    null);

                // When
                string initialValue = document.String("A");
                string clonedValue = cloned.String("A");
                string onlyMetadataValue = cloned.Metadata.String("A");

                // Then
                initialValue.ShouldBe("a");
                clonedValue.ShouldBe("b");
                onlyMetadataValue.ShouldBe("b");
            }

            [Test]
            public void GetsPropertyMetadata()
            {
                // Given
                TestEngine engine = new TestEngine();
                PropertyDocument document = new PropertyDocument(engine, null, null, null, null)
                {
                    Foo = "abc"
                };
                PropertyDocument cloned = document.Clone(null, null, new MetadataItems { { "Foo", "xyz" } }, null);

                // When
                string initialValue = document.String("Foo");
                string initialOnlyMetadataValue = document.Metadata.String("Foo");
                string clonedValue = cloned.String("Foo");
                string onlyMetadataValue = cloned.Metadata.String("Foo");

                // Then
                initialValue.ShouldBe("abc");  // Hey look ma, it's coming from a document property!
                initialOnlyMetadataValue.ShouldBeNull();
                clonedValue.ShouldBe("xyz");
                onlyMetadataValue.ShouldBe("xyz");
            }

            private class PropertyDocument : Document<PropertyDocument>
            {
                public string Foo { get; set; }

                // Required constructors and overrides

                public PropertyDocument(
                   IEngine engine,
                   FilePath source,
                   FilePath destination,
                   IEnumerable<KeyValuePair<string, object>> items,
                   IContentProvider contentProvider = null)
                   : base(engine, source, destination, items, contentProvider)
                {
                }

                private PropertyDocument(
                    PropertyDocument sourceDocument,
                    FilePath source,
                    FilePath destination,
                    IEnumerable<KeyValuePair<string, object>> items,
                    IContentProvider contentProvider = null)
                    : base(sourceDocument, source, destination, items, contentProvider)
                {
                }

                public override PropertyDocument Clone(
                    FilePath source,
                    FilePath destination,
                    IEnumerable<KeyValuePair<string, object>> items,
                    IContentProvider contentProvider = null) =>
                    new PropertyDocument(this, source, destination, items, contentProvider);
            }
        }
    }
}
