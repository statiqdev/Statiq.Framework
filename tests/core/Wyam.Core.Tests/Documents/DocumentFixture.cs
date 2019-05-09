using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Wyam.Common.Documents;
using Wyam.Common.IO;
using Wyam.Common.Meta;
using Wyam.Core.Documents;
using Wyam.Core.Meta;
using Wyam.Testing;

namespace Wyam.Core.Tests.Documents
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
                Document a = new Document(initialMetadata);
                Document b = new Document(initialMetadata);

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
                Document document = new Document(initialMetadata);

                // When
                IDocument cloned = new Document(document, new MetadataItems());

                // Then
                Assert.AreEqual(document.Id, cloned.Id);
            }
        }

        public class WitoutSettingsTests : DocumentFixture
        {
            [Test]
            public void ReturnsMetadataWithoutSettings()
            {
                // Given
                MetadataDictionary initialMetadata = new MetadataDictionary();
                initialMetadata.Add("A", "a");
                Document document = new Document(initialMetadata);
                Document cloned = new Document(document, new MetadataItems { { "B", "b" } });

                // When
                string initialA = document.String("A");
                string initialB = document.String("B");
                string clonedA = cloned.String("A");
                string clonedB = cloned.String("B");
                string withoutA = cloned.WithoutSettings.String("A");
                string withoutB = cloned.WithoutSettings.String("B");

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
