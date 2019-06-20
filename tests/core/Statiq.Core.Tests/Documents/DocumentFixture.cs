using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Statiq.Common.Documents;
using Statiq.Common.IO;
using Statiq.Common.Meta;
using Statiq.Core.Documents;
using Statiq.Core.Meta;
using Statiq.Testing;

namespace Statiq.Core.Tests.Documents
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
                MetadataDictionary initialMetadata = new MetadataDictionary();

                // When
                Document a = new Document(initialMetadata, null, null, null, null);
                Document b = new Document(initialMetadata, null, null, null, null);

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
                MetadataDictionary initialMetadata = new MetadataDictionary();
                Document document = new Document(initialMetadata, null, null, null, null);

                // When
                IDocument cloned = new Document(
                    document,
                    document.Version + 1,
                    null,
                    null,
                    null,
                    new MetadataItems());

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
                MetadataDictionary initialMetadata = new MetadataDictionary();
                initialMetadata.Add("A", "a");
                Document document = new Document(initialMetadata, null, null, null, null);
                Document cloned = new Document(
                    document,
                    document.Version + 1,
                    null,
                    null,
                    null,
                    new MetadataItems { { "B", "b" } });

                // When
                string initialA = document.String("A");
                string initialB = document.String("B");
                string clonedA = cloned.String("A");
                string clonedB = cloned.String("B");
                string withoutA = cloned.Metadata.String("A");
                string withoutB = cloned.Metadata.String("B");

                // Then
                Assert.AreEqual("a", initialA);
                Assert.IsNull(initialB);
                Assert.AreEqual("a", clonedA);
                Assert.AreEqual("b", clonedB);
                Assert.IsNull(withoutA);
                Assert.AreEqual("b", withoutB);
            }
        }
    }
}
