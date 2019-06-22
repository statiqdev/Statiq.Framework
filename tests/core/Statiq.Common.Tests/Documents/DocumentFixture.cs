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
                // Given, When
                Document a = new Document();
                Document b = new Document();

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
                Document document = new Document();

                // When
                IDocument cloned = document.Clone(null);

                // Then
                Assert.AreEqual(document.Id, cloned.Id);
            }

            [Test]
            public void DocumentTypeTheSameAfterClone()
            {
                // Given
                CustomDocument document = new CustomDocument();

                // When
                IDocument cloned = document.Clone(null);

                // Then
                cloned.ShouldBeOfType<CustomDocument>();
            }

            [Test]
            public void MembersAreCloned()
            {
                // Given
                CustomDocument document = new CustomDocument
                {
                    Foo = "abc"
                };

                // When
                IDocument cloned = document.Clone(null);

                // Then
                ((CustomDocument)cloned).Foo.ShouldBe("abc");
            }
        }

        public class MetadataTests : DocumentFixture
        {
            [Test]
            public void MetadataOverwritesSettings()
            {
                // Given
                MetadataItems settings = new MetadataItems
                {
                    { "A", "a" }
                };
                Document document = new Document(new Metadata(settings), null, null, null, null);
                IDocument cloned = document.Clone(new MetadataItems { { "A", "b" } });

                // When
                string initialValue = document.String("A");
                string clonedValue = cloned.String("A");

                // Then
                initialValue.ShouldBe("a");
                clonedValue.ShouldBe("b");
            }

            [Test]
            public void GetsPropertyMetadata()
            {
                // Given
                CustomDocument document = new CustomDocument
                {
                    Foo = "abc"
                };
                IDocument cloned = document.Clone(new MetadataItems { { "Foo", "xyz" } });

                // When
                string initialValue = document.String("Foo");
                string clonedValue = cloned.String("Foo");

                // Then
                initialValue.ShouldBe("abc");
                clonedValue.ShouldBe("xyz");
            }

            [Test]
            public void IncludesBaseDocumentProperties()
            {
                // Given, When
                Document document = new Document();

                // Then
                document.Keys.ShouldBe(new[] { "Id", "Source", "Destination", "ContentProvider", "HasContent" }, true);
            }
        }

        private class CustomDocument : Document<CustomDocument>
        {
            public string Foo { get; set; }
        }
    }
}
