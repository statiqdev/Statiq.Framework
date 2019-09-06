using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Documents
{
    [TestFixture]
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
                IDocument document = new Document();

                // When
                IDocument cloned = document.Clone(null);

                // Then
                Assert.AreEqual(document.Id, cloned.Id);
            }

            [Test]
            public void DocumentTypeTheSameAfterClone()
            {
                // Given
                IDocument document = new CustomDocument();

                // When
                IDocument cloned = document.Clone(null);

                // Then
                cloned.ShouldBeOfType<CustomDocument>();
            }

            [Test]
            public void MembersAreCloned()
            {
                // Given
                IDocument document = new CustomDocument
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
                IDocument document = new Document(new Metadata(settings), null, null, null, null);
                IDocument cloned = document.Clone(new MetadataItems { { "A", "b" } });

                // When
                string initialValue = document.GetString("A");
                string clonedValue = cloned.GetString("A");

                // Then
                initialValue.ShouldBe("a");
                clonedValue.ShouldBe("b");
            }

            [Test]
            public void GetsPropertyMetadata()
            {
                // Given
                IDocument document = new CustomDocument
                {
                    Foo = "abc"
                };
                IDocument cloned = document.Clone(new MetadataItems { { "Foo", "xyz" } });

                // When
                string initialValue = document.GetString("Foo");
                string clonedValue = cloned.GetString("Foo");

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
                document.Keys.ShouldBe(new[] { "Id", "Source", "Destination", "ContentProvider" }, true);
            }
        }

        private class CustomDocument : Document<CustomDocument>
        {
            public string Foo { get; set; }
        }
    }
}
